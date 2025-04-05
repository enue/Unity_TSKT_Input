#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Buffers;

namespace TSKT
{
    public abstract class InputActionUI : MonoBehaviour
    {
        static readonly HashSet<InputActionUI> instances = new();
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

        public static void BuildSortedItemsToActivate(IBufferWriter<InputActionUI> writer)
        {
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

                var span = writer.GetSpan(uiPositions.Count);
                var writtenCount = 0;
                foreach (var (position, ui) in uiPositions)
                {
                    span[writtenCount] = ui;
                    ++writtenCount;
                    if (ui.Modal)
                    {
                        break;
                    }
                }
                writer.Advance(writtenCount);
            }
        }
    }
}
