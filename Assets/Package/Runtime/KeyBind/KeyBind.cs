using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#nullable enable

namespace TSKT
{
    public abstract class KeyBind : MonoBehaviour
    {
        struct KeyBindBuffer
        {
            int lastRefreshedTime;

            List<KeyBind> items;
            public List<KeyBind> Items
            {
                get
                {
                    if (lastRefreshedTime != Time.frameCount)
                    {
                        lastRefreshedTime = Time.frameCount;
                        Refresh();
                    }
                    return items;
                }
            }

            void Refresh()
            {
                using (UnityEngine.Pool.ListPool<(RenderOrder position, KeyBind keyBind)>.Get(out var keyBindPositions))
                {
                    RenderOrder? maxInterceptor = default;

                    foreach (var keyBind in Instances)
                    {
                        if (!keyBind)
                        {
                            continue;
                        }
                        if (!keyBind.gameObject.activeInHierarchy)
                        {
                            continue;
                        }

                        keyBind.RefreshRootCanvas();
                        UnityEngine.Assertions.Assert.IsTrue(keyBind.RootCanvas);
                        var position = new RenderOrder(keyBind.RootCanvas!, keyBind.transform, keyBind.orderInObject);

                        if (maxInterceptor.HasValue)
                        {
                            if (maxInterceptor > position)
                            {
                                continue;
                            }
                        }

                        keyBindPositions.Add((position, keyBind));

                        if (keyBind.BlockingSignals)
                        {
                            maxInterceptor = position;
                        }
                    }
                    // メソッドを直接渡すとGCが発生するのでラムダにする
                    // 降順にしたいので符号を反転させる
                    keyBindPositions.Sort((x, y) => -RenderOrder.Compare(x.position, y.position));
                    if (items == null)
                    {
                        items = new List<KeyBind>(Instances.Count);
                    }
                    else
                    {
                        items.Clear();
                    }

                    foreach (var (position, keyBind) in keyBindPositions)
                    {
                        items.Add(keyBind);
                        if (keyBind.BlockingSignals)
                        {
                            break;
                        }
                    }
                }
            }
        }

        static public HashSet<KeyBind> Instances { get; } = new HashSet<KeyBind>();
        static bool Modified { get; set; }

        [SerializeField]
        int orderInObject = 0;

        void OnEnable()
        {
            Instances.Add(this);
            Modified = true;
        }

        void OnDisable()
        {
            Instances.Remove(this);
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

        public abstract bool BlockingSignals { get; }
        public abstract void OnSelected();
        public abstract void Execute(out bool exclusive);

        static KeyBindBuffer keyBindsBuffer;
        static int lastUpdatedFrame;

        void Update()
        {
            if (lastUpdatedFrame == Time.frameCount)
            {
                return;
            }
            lastUpdatedFrame = Time.frameCount;
            SendSelected();

            foreach (var item in keyBindsBuffer.Items)
            {
                item.Execute(out var exclusive);
                if (exclusive)
                {
                    break;
                }
                if (item.BlockingSignals)
                {
                    break; ;
                }
            }
        }

        static void SendSelected()
        {
            if (Modified)
            {
                // 変更通知
                Modified = false;

                foreach (var keyBind in keyBindsBuffer.Items)
                {
                    keyBind.OnSelected();
                    if (keyBind.BlockingSignals)
                    {
                        break;
                    }
                }
            }
        }
    }
}
