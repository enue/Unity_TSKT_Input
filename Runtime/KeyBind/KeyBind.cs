﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TSKT
{
    public abstract class KeyBind : MonoBehaviour
    {
        struct KeyBindBuffer
        {
            int lastRefreshedTime;
            List<(RenderOrder position, KeyBind keyBind)> keyBindPositions;

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
                if (keyBindPositions == null)
                {
                    keyBindPositions = new List<(RenderOrder position, KeyBind keyBind)>();
                }
                else
                {
                    keyBindPositions.Clear();
                }
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
                    var position = new RenderOrder(keyBind.RootCanvas, keyBind.transform);

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
                    items = new List<KeyBind>(KeyBind.Instances.Count);
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

                keyBindPositions.Clear();
            }
        }

        static HashSet<KeyBind> Instances { get; } = new HashSet<KeyBind>();
        static bool Modified { get; set; }

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

        Canvas rootCanvas;
        public Canvas RootCanvas
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
            if (!transform.IsChildOf(rootCanvas.transform))
            {
                rootCanvas = null;
            }
        }

        public abstract bool BlockingSignals { get; }
        public abstract bool OnKeyDown(string key);
        public abstract bool OnKeyUp(string key);
        public abstract bool OnAxis(Dictionary<string, float> axisPositions);
        public abstract void OnSelected();

        static KeyBindBuffer keyBindsBuffer;

        public static void SendSignals(
            List<string> downKeys,
            List<string> upKeys,
            Dictionary<string, float> axisPositions)
        {
            SendSelected();
            foreach (var it in downKeys)
            {
                if (SendOnKeyDown(it))
                {
                    return;
                }
            }
            foreach (var it in upKeys)
            {
                if (SendOnKeyUp(it))
                {
                    return;
                }
            }
            SendOnAxis(axisPositions);
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

        static bool SendOnKeyDown(string key)
        {
            foreach (var keyBind in keyBindsBuffer.Items)
            {
                if (keyBind.OnKeyDown(key))
                {
                    return true;
                }
                if (keyBind.BlockingSignals)
                {
                    break;
                }
            }

            return false;
        }

        static bool SendOnKeyUp(string key)
        {
            foreach (var keyBind in keyBindsBuffer.Items)
            {
                if (keyBind.OnKeyUp(key))
                {
                    return true;
                }
                if (keyBind.BlockingSignals)
                {
                    break;
                }
            }

            return false;
        }

        static bool SendOnAxis(Dictionary<string, float> axisPositions)
        {
            foreach (var keyBind in keyBindsBuffer.Items)
            {
                if (keyBind.OnAxis(axisPositions))
                {
                    return true;
                }
                if (keyBind.BlockingSignals)
                {
                    break;
                }
            }

            return false;
        }
    }
}