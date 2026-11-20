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
    /// Top以外のUIInputModalScopeは、CanvasGroupのblocksRaycastsを切ってクリックを受けなくする。
    /// interactableには触らないので、下になったスコープのボタンが無効の見た目になることはない。
    /// 下になったスコープのSelectableは、ナビゲーションをNoneにしてキー操作の移動先から外す(Topに戻る時に元へ戻す)。
    /// それでも下のスコープへ選択が移った場合は、同じフレームのうちに選択を戻す。
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

        /// <summary>
        /// Topの配下で最後に選択されていたもの。選択が下のスコープへ移った時に、ここへ戻す。
        /// </summary>
        static GameObject? lastSelectedInTop;

        static readonly List<Selectable> selectableBuffer = new();

        CanvasGroup? canvasGroup;
        readonly List<(Selectable selectable, Navigation.Mode mode)> suppressedNavigations = new();
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
            originalBlocksRaycasts = canvasGroup!.blocksRaycasts;
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
            RestoreNavigations();
            if (canvasGroup)
            {
                canvasGroup!.blocksRaycasts = originalBlocksRaycasts;
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
                lastSelectedInTop = null;
                return;
            }

            var top = Top;

            // 入れ子のスコープがあると対象が変わるので、いったん全部戻してから外し直す
            foreach (var it in stack)
            {
                it.RestoreNavigations();
            }
            foreach (var it in stack)
            {
                it.Apply(it == top);
                if (it != top)
                {
                    it.SuppressNavigations(top!);
                }
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
            lastSelectedInTop = null;
        }

        /// <summary>
        /// 選択が下のスコープへ移っていたら戻す。UIInputDispatcherがLateUpdateで呼ぶ。
        /// 下のスコープのSelectableもinteractableのままなので、キーやパッドのナビゲーションで選択が移りうる。
        /// EventSystemが入力を処理するUpdateのあと、同じフレームのうちに戻す。
        /// どのスコープにも属さないものが選択された場合は、何もしない。
        /// </summary>
        internal static void GuardSelection()
        {
            var top = Top;
            if (!top)
            {
                return;
            }
            var eventSystem = EventSystem.current;
            if (!eventSystem)
            {
                return;
            }
            var selected = eventSystem.currentSelectedGameObject;
            if (!selected)
            {
                return;
            }
            var selectedTransform = selected.transform;
            if (selectedTransform.IsChildOf(top!.transform))
            {
                lastSelectedInTop = selected;
                return;
            }
            if (!IsInCoveredScope(selectedTransform, top))
            {
                return;
            }

            var target = lastSelectedInTop;
            if (!target || !target!.activeInHierarchy || !target.transform.IsChildOf(top.transform))
            {
                target = null;
            }
            eventSystem.SetSelectedGameObject(target);
        }

        static bool IsInCoveredScope(Transform target, UIInputModalScope top)
        {
            foreach (var it in stack)
            {
                if (it != top && target.IsChildOf(it.transform))
                {
                    return true;
                }
            }
            return false;
        }

        void Apply(bool isTop)
        {
            if (!canvasGroup)
            {
                return;
            }

            // interactableは変えない。変えると配下のSelectableが無効の見た目になる
            canvasGroup!.blocksRaycasts = isTop && originalBlocksRaycasts;

            // クリックを受けなくなった他のスコープの子になっている場合、親のCanvasGroupに巻き込まれないようにする
            canvasGroup.ignoreParentGroups = originalIgnoreParentGroups
                || (isTop && HasAncestorInStack());
        }

        /// <summary>
        /// 配下のSelectableを、キー操作の移動先から外す。
        /// interactableを切らない代わりの処置。uGUIは、ナビゲーションがNoneのSelectableを移動先に選ばない。
        /// あとから選択を戻すだけでは、移動と決定を同じフレームに押された時に下のボタンが押されてしまう。
        /// </summary>
        void SuppressNavigations(UIInputModalScope top)
        {
            GetComponentsInChildren(true, selectableBuffer);
            foreach (var it in selectableBuffer)
            {
                // このスコープの子になっているTopの配下は、操作できるままにする
                if (it.transform.IsChildOf(top.transform))
                {
                    continue;
                }
                var navigation = it.navigation;
                if (navigation.mode == Navigation.Mode.None)
                {
                    continue;
                }
                suppressedNavigations.Add((it, navigation.mode));
                navigation.mode = Navigation.Mode.None;
                it.navigation = navigation;
            }
            selectableBuffer.Clear();
        }

        void RestoreNavigations()
        {
            foreach (var (selectable, mode) in suppressedNavigations)
            {
                if (!selectable)
                {
                    continue;
                }
                var navigation = selectable.navigation;
                // 外している間に利用側が書き換えていたら、そちらを優先する
                if (navigation.mode == Navigation.Mode.None)
                {
                    navigation.mode = mode;
                    selectable.navigation = navigation;
                }
            }
            suppressedNavigations.Clear();
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
            lastSelectedInTop = null;
        }
    }
}
