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
        public Canvas? RootCanvas
        {
            get
            {
                if (!rootCanvas)
                {
                    rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
                }
                return rootCanvas;
            }
        }

        public void RefreshRootCanvas()
        {
            if (!rootCanvas)
            {
                return;
            }
            if (!transform.IsChildOf(rootCanvas!.transform))
            {
                rootCanvas = null;
            }
        }

        protected abstract bool Modal { get; }
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
            using (UnityEngine.Pool.ListPool<(RenderOrder position, InputActionUI keyBind)>.Get(out var keyBindPositions))
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

                    item.RefreshRootCanvas();
                    UnityEngine.Assertions.Assert.IsTrue(item.RootCanvas);
                    var position = new RenderOrder(item.RootCanvas!, item.transform, item.orderInObject);

                    if (maxInterceptor.HasValue)
                    {
                        if (maxInterceptor > position)
                        {
                            continue;
                        }
                    }

                    keyBindPositions.Add((position, item));

                    if (item.Modal)
                    {
                        maxInterceptor = position;
                    }
                }
                // メソッドを直接渡すとGCが発生するのでラムダにする
                // 降順にしたいので符号を反転させる
                keyBindPositions.Sort((x, y) => -RenderOrder.Compare(x.position, y.position));

                foreach (var (position, keyBind) in keyBindPositions)
                {
                    result.Add(keyBind);
                    if (keyBind.Modal)
                    {
                        break;
                    }
                }
            }
            return pooledObject;
        }
    }
}
