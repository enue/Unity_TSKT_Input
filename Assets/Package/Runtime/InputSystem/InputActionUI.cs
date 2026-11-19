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
        bool warnedCanvasNotFound;

        /// <summary>
        /// Canvasの外に置かれている場合はnull
        /// </summary>
        Canvas? RootCanvas
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
                    var canvas = GetComponentInParent<Canvas>();
                    rootCanvas = canvas ? canvas.rootCanvas : null;
                }
                return rootCanvas ? rootCanvas : null;
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

                    var itemRootCanvas = item.RootCanvas;
                    if (!itemRootCanvas)
                    {
                        // Canvasの外にあると描画順を決められないので対象外
                        if (!item.warnedCanvasNotFound)
                        {
                            item.warnedCanvasNotFound = true;
                            Debug.LogWarning(item.GetType().Name + " requires a parent Canvas : " + item.name, item);
                        }
                        continue;
                    }
                    item.warnedCanvasNotFound = false;

                    var position = new RenderOrder(itemRootCanvas!, item.transform, item.orderInObject);

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
