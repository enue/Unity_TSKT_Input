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
        public static bool ShouldBuildNavigation { get; set; }

        [SerializeField]
        int orderInObject = 0;

        void OnEnable()
        {
            instances.Add(this);
            ShouldBuildNavigation = true;
        }

        void OnDisable()
        {
            instances.Remove(this);
            ShouldBuildNavigation = true;
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
        public Selectable? Selectable
        {
            get
            {
                if (!selectable)
                {
                    if (!TryGetComponent<Selectable>(out selectable))
                    {
                        return null;
                    }
                }
                return selectable;
            }
        }

        public abstract bool Modal { get; }
        public abstract Navigation Navigation { get; }
        public abstract void Activate();
        public abstract void Invoke(out bool exclusive);

        public static UnityEngine.Pool.PooledObject<List<InputActionUI>> BuildSortedItemsToActivate(out List<InputActionUI> result)
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
                // 降順にしたいので符号を反転させる
                uiPositions.Sort(static (x, y) => -RenderOrder.Compare(x.position, y.position));

                foreach (var (position, ui) in uiPositions)
                {
                    result.Add(ui);
                    if (ui.Modal)
                    {
                        break;
                    }
                }
            }
            return pooledObject;
        }
    }
}
