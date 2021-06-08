#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TSKT
{
    public abstract class InputActionUI : MonoBehaviour
    {
        static readonly HashSet<InputActionUI> instances = new HashSet<InputActionUI>();
        static bool Modified { get; set; }

        [SerializeField]
        int orderInObject = 0;

        void OnEnable()
        {
            instances.Add(this);
            Modified = true;
        }

        void OnDisable()
        {
            instances.Remove(this);
            Modified = true;
        }

        Canvas? rootCanvas;
        Canvas RootCanvas
        {
            get
            {
                if (rootCanvas)
                {
                    if (!transform.IsChildOf(rootCanvas!.transform))
                    {
                        rootCanvas = null;
                    }
                }
                if (!rootCanvas)
                {
                    rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
                }
                return rootCanvas!;
            }
        }

        Selectable? selectable;
        Selectable? Selectable
        {
            get
            {
                if (!selectable)
                {
                    TryGetComponent(out selectable);
                }
                return selectable;
            }
        }

        protected abstract bool Modal { get; }
        protected abstract bool Navigated { get; }
        protected abstract void Activate();
        protected abstract void Invoke(out bool exclusive);
        static int lastUpdatedFrame;

        void Update()
        {
            if (lastUpdatedFrame == Time.frameCount)
            {
                return;
            }
            lastUpdatedFrame = Time.frameCount;

            using (BuildSortedItems(out var sortedItems))
            {
                if (Modified)
                {
                    Modified = false;

                    foreach (var it in Selectable.allSelectablesArray)
                    {
                        var navigation = it!.navigation;
                        navigation.mode = Navigation.Mode.None;
                        it.navigation = navigation;
                    }
                    {
                        GameObject? selectdGameObject = null;
                        var underModal = false;
                        foreach (var it in sortedItems)
                        {
                            var selectable = it.Selectable;
                            if (selectable)
                            {
                                var navigation = selectable!.navigation;
                                if (it.Navigated && !underModal)
                                {
                                    navigation.mode = Navigation.Mode.Automatic;
                                    if (!selectdGameObject)
                                    {
                                        selectdGameObject = it.gameObject;
                                    }
                                }
                                selectable.navigation = navigation;
                            }
                            underModal |= it.Modal;
                        }
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectdGameObject);
                    }
                    foreach (var it in sortedItems)
                    {
                        it.Activate();
                        if (it.Modal)
                        {
                            break;
                        }
                    }
                }

                foreach (var item in sortedItems)
                {
                    item.Invoke(out var exclusive);
                    if (exclusive)
                    {
                        break;
                    }
                    if (item.Modal)
                    {
                        break; ;
                    }
                }
            }
        }

        static UnityEngine.Pool.PooledObject<List<InputActionUI>> BuildSortedItems(out List<InputActionUI> result)
        {
            var pooledObject = UnityEngine.Pool.ListPool<InputActionUI>.Get(out result);
            using (UnityEngine.Pool.ListPool<(RenderOrder position, InputActionUI ui)>.Get(out var uiPositions))
            {
                RenderOrder? maxInterceptor = default;

                foreach (var item in instances)
                {
                    if (!item)
                    {
                        continue;
                    }
                    if (!item.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    var position = new RenderOrder(item.RootCanvas, item.transform, item.orderInObject);

                    if (maxInterceptor.HasValue)
                    {
                        if (maxInterceptor > position)
                        {
                            continue;
                        }
                    }

                    uiPositions.Add((position, item));

                    if (item.Modal)
                    {
                        maxInterceptor = position;
                    }
                }
                // メソッドを直接渡すとGCが発生するのでラムダにする
                // 降順にしたいので符号を反転させる
                uiPositions.Sort((x, y) => -RenderOrder.Compare(x.position, y.position));

                foreach (var (position, ui) in uiPositions)
                {
                    result.Add(ui);
                }
            }
            return pooledObject;
        }
    }
}
