#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TSKT
{
    /// <summary>
    /// モーダルのルートに付ける。有効になった順にスタックに積まれ、
    /// 一番最後に有効になったもの(Top)の配下だけが操作可能になる。
    /// Top以外のUIInputModalScopeはCanvasGroupで操作不可にされる。
    /// UIInputModalScopeがひとつでも有効な間、どのUIInputModalScopeにも属さないIUIInputReceiverは反応しない。
    /// 有効なUIInputDispatcherがない間は何もしない(CanvasGroupは元の値のまま)。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIInputModalScope : MonoBehaviour
    {
        static readonly List<UIInputModalScope> stack = new();
        static UIInputModalScope? previousTop;

        /// <summary>
        /// 現在操作可能なUIInputModalScope。ひとつも有効でなければnull。
        /// </summary>
        public static UIInputModalScope? Top => stack.Count > 0 ? stack[stack.Count - 1] : null;

        [SerializeField]
        [Tooltip("Topになった時に選択するオブジェクト。以前このスコープ内で選択していたものがあればそちらを優先する。")]
        Selectable? firstSelected = default;

        CanvasGroup? canvasGroup;
        bool originalInteractable;
        bool originalBlocksRaycasts;
        bool originalIgnoreParentGroups;
        GameObject? lastSelected;

        /// <summary>
        /// targetが現在操作可能な範囲にあるか。
        /// </summary>
        public static bool IsOperable(Transform target)
        {
            var top = Top;
            if (!top || !UIInputDispatcher.Active)
            {
                return true;
            }
            return target.IsChildOf(top!.transform);
        }

        internal static void OnDispatcherActiveChanged()
        {
            Refresh();
        }

        void OnEnable()
        {
            if (!canvasGroup)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            originalInteractable = canvasGroup!.interactable;
            originalBlocksRaycasts = canvasGroup.blocksRaycasts;
            originalIgnoreParentGroups = canvasGroup.ignoreParentGroups;

            stack.Add(this);
            UIInputDispatcher.WarnIfInactive(this);
            Refresh();
        }

        void OnDisable()
        {
            stack.Remove(this);
            Restore();
            Refresh();
        }

        void Restore()
        {
            if (canvasGroup)
            {
                canvasGroup!.interactable = originalInteractable;
                canvasGroup.blocksRaycasts = originalBlocksRaycasts;
                canvasGroup.ignoreParentGroups = originalIgnoreParentGroups;
            }
        }

        static void Refresh()
        {
            // UIInputDispatcherがない間は、すべて元の状態に戻して何もしない
            if (!UIInputDispatcher.Active)
            {
                foreach (var it in stack)
                {
                    it.Restore();
                }
                previousTop = null;
                return;
            }

            var top = Top;

            foreach (var it in stack)
            {
                it.Apply(it == top);
            }

            if (previousTop == top)
            {
                return;
            }

            var eventSystem = EventSystem.current;
            if (eventSystem)
            {
                var selected = eventSystem.currentSelectedGameObject;

                // 上に別のスコープが開いた場合、戻ってきた時のために選択を覚えておく
                if (previousTop && stack.Contains(previousTop!))
                {
                    if (selected && selected.transform.IsChildOf(previousTop!.transform))
                    {
                        previousTop.lastSelected = selected;
                    }
                }

                if (top)
                {
                    top!.Select(eventSystem, selected);
                }
            }

            previousTop = top;
        }

        void Apply(bool isTop)
        {
            if (!canvasGroup)
            {
                return;
            }

            canvasGroup!.interactable = isTop && originalInteractable;
            canvasGroup.blocksRaycasts = isTop && originalBlocksRaycasts;

            // 操作不可になった他のスコープの子になっている場合、親のCanvasGroupに巻き込まれないようにする
            canvasGroup.ignoreParentGroups = originalIgnoreParentGroups
                || (isTop && HasAncestorInStack());
        }

        bool HasAncestorInStack()
        {
            foreach (var it in stack)
            {
                if (it != this && transform.IsChildOf(it.transform))
                {
                    return true;
                }
            }
            return false;
        }

        void Select(EventSystem eventSystem, GameObject? currentSelected)
        {
            GameObject? target = null;
            if (lastSelected && lastSelected!.activeInHierarchy)
            {
                target = lastSelected;
            }
            // isActiveAndEnabledは使わない。アクティブな状態でInstantiateされた直後は、
            // 子のOnEnableがまだ呼ばれておらずfalseになる
            else if (firstSelected && firstSelected!.enabled && firstSelected.gameObject.activeInHierarchy)
            {
                target = firstSelected.gameObject;
            }
            lastSelected = null;

            if (target)
            {
                eventSystem.SetSelectedGameObject(target);
            }
            else if (currentSelected && !currentSelected!.transform.IsChildOf(transform))
            {
                // 下のスコープのオブジェクトを選択したままにしない
                eventSystem.SetSelectedGameObject(null);
            }
        }

        // ReloadDomainなしで再生した場合に前回の状態を持ち越さない
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            stack.Clear();
            previousTop = null;
        }
    }
}
