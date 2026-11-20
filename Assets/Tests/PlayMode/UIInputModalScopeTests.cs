#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TSKT.Tests
{
    public class UIInputModalScopeTests : UIInputTestBase
    {
        static string Flags(UIInputModalScope scope)
        {
            var group = scope.GetComponent<CanvasGroup>();
            return (group.interactable ? "interactable" : "-") + "/" + (group.blocksRaycasts ? "raycast" : "-");
        }

        [Test]
        public void OnlyTopScope_BlocksRaycasts()
        {
            var baseScope = CreateScope("Base", out _);
            Assert.AreEqual(baseScope, UIInputModalScope.Top);
            Assert.AreEqual("interactable/raycast", Flags(baseScope));

            var dialog = CreateScope("Dialog", out _);
            Assert.AreEqual(dialog, UIInputModalScope.Top);
            Assert.AreEqual("interactable/-", Flags(baseScope), "下になってもinteractableは変えない");
            Assert.AreEqual("interactable/raycast", Flags(dialog));

            Object.DestroyImmediate(dialog.gameObject);
            Assert.AreEqual(baseScope, UIInputModalScope.Top);
            Assert.AreEqual("interactable/raycast", Flags(baseScope));
        }

        [Test]
        public void ThreeScopes_CloseInOrder()
        {
            var a = CreateScope("A", out _);
            var b = CreateScope("B", out _);
            var c = CreateScope("C", out _);
            Assert.AreEqual("interactable/- interactable/- interactable/raycast", $"{Flags(a)} {Flags(b)} {Flags(c)}");

            c.gameObject.SetActive(false);
            Assert.AreEqual("interactable/- interactable/raycast", $"{Flags(a)} {Flags(b)}");

            b.gameObject.SetActive(false);
            Assert.AreEqual("interactable/raycast", Flags(a));
        }

        [Test]
        public void OriginalCanvasGroupValues_AreRestored()
        {
            var baseObject = CreateChild("Base", canvas.transform);
            baseObject.SetActive(false);
            var group = baseObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var baseScope = baseObject.AddComponent<UIInputModalScope>();
            baseObject.SetActive(true);
            Assert.AreEqual("interactable/-", Flags(baseScope), "元からfalseだった値は、一番上でもfalseのまま");

            var dialog = CreateScope("Dialog", out _);
            Assert.AreEqual("interactable/-", Flags(baseScope));

            Object.DestroyImmediate(dialog.gameObject);
            Assert.AreEqual("interactable/-", Flags(baseScope));
        }

        [Test]
        public void Interactable_IsNeverTouched()
        {
            var baseObject = CreateChild("Base", canvas.transform);
            baseObject.SetActive(false);
            var group = baseObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            var baseScope = baseObject.AddComponent<UIInputModalScope>();
            baseObject.SetActive(true);
            Assert.AreEqual("-/raycast", Flags(baseScope));

            var dialog = CreateScope("Dialog", out _);
            // スコープが有効な間に利用側が書き換えても、上書きしない
            group.interactable = true;
            Object.DestroyImmediate(dialog.gameObject);
            Assert.AreEqual("interactable/raycast", Flags(baseScope));
        }

        [Test]
        public void CoveredScope_SelectablesStayInteractable()
        {
            CreateScope("Base", out var baseButton);
            CreateScope("Dialog", out _);

            // IsInteractableがfalseになると、ボタンが無効の見た目(disabledColor)になる
            Assert.IsTrue(baseButton.IsInteractable());
        }

        [Test]
        public void FirstSelected_IsSelected_AndSelectionIsRestored()
        {
            var baseScope = CreateScope("Base", out _);
            Assert.AreEqual("Base/Button", SelectedName());

            var other = CreateButton("Other", baseScope.transform);
            eventSystem.SetSelectedGameObject(other.gameObject);

            var dialog = CreateScope("Dialog", out _);
            Assert.AreEqual("Dialog/Button", SelectedName());

            Object.DestroyImmediate(dialog.gameObject);
            Assert.AreEqual("Base/Other", SelectedName(), "下のスコープで最後に選択していたものに戻る");
        }

        [Test]
        public void ScopeWithoutFirstSelected_ClearsSelectionOfLowerScope()
        {
            CreateScope("Base", out _);
            Assert.AreEqual("Base/Button", SelectedName());

            CreateScope("Dialog", out _, selectFirst: false);
            Assert.AreEqual("null", SelectedName(), "下のスコープのものを選択したままにしない");
        }

        [Test]
        public void InstantiatedActiveClone_SelectsItsOwnFirstSelected()
        {
            var dialog = CreateScope("Dialog", out var originalButton);

            var clone = Object.Instantiate(dialog.gameObject, canvas.transform);
            clone.name = "Clone";

            Assert.AreEqual(clone.GetComponent<UIInputModalScope>(), UIInputModalScope.Top);
            Assert.AreEqual("Clone/Button", SelectedName());
            Assert.AreNotEqual(originalButton.gameObject, eventSystem.currentSelectedGameObject);
            Assert.AreEqual("interactable/-", Flags(dialog));
        }

        [Test]
        public void NestedScope_IsNotBlockedByItsParentScope()
        {
            var parent = CreateScope("Parent", out _);
            var childObject = CreateChild("Child", parent.transform);
            childObject.SetActive(false);
            childObject.AddComponent<CanvasGroup>();
            var childButton = CreateButton("Button", childObject.transform);
            var child = childObject.AddComponent<UIInputModalScope>();
            childObject.SetActive(true);

            Assert.AreEqual(child, UIInputModalScope.Top);
            Assert.AreEqual("interactable/-", Flags(parent));
            Assert.AreEqual("interactable/raycast", Flags(child));
            Assert.IsTrue(childObject.GetComponent<CanvasGroup>().ignoreParentGroups, "親がクリックを受けなくても、一番上の子は受ける");
            Assert.IsTrue(childButton.IsInteractable());
        }

        [UnityTest]
        public IEnumerator Receiver_OutsideTopScope_IsNotNotified()
        {
            var baseScope = CreateScope("Base", out _);
            var dialog = CreateScope("Dialog", out _);
            // 後から登録したほうが優先されるので、スコープがなければbaseRecorderが受け取る
            var dialogRecorder = AddRecorder(action1, dialog.transform);
            var baseRecorder = AddRecorder(action1, baseScope.transform);
            var outsideRecorder = AddRecorder(action2, canvas.transform);

            yield return Press(Key.Digit1);
            Assert.AreEqual("", baseRecorder.Take());
            Assert.AreEqual("PFR", dialogRecorder.Take());

            yield return Press(Key.Digit2);
            Assert.AreEqual("", outsideRecorder.Take(), "どのスコープにも属さないレシーバーも反応しない");

            Object.DestroyImmediate(dialog.gameObject);
            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", baseRecorder.Take());
        }

        [UnityTest]
        public IEnumerator ScopeOpened_WhilePressed_Cancels()
        {
            var baseScope = CreateScope("Base", out _);
            var recorder = AddRecorder(action1, baseScope.transform);

            yield return Down(Key.Digit1);
            recorder.Take();
            var dialog = CreateScope("Dialog", out _);
            var dialogRecorder = AddRecorder(action1, dialog.transform);
            yield return Frames();

            Assert.AreEqual("C", recorder.Take());
            Assert.AreEqual("", dialogRecorder.Take(), "押したままでは、新しいスコープ側も反応しない");

            yield return Up();
            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", dialogRecorder.Take());
        }

        [Test]
        public void WithoutDispatcher_DoesNothing()
        {
            dispatcher.enabled = false;

            var baseScope = CreateScope("Base", out _);
            var dialog = CreateScope("Dialog", out _);
            Assert.AreEqual("interactable/raycast", Flags(baseScope));
            Assert.AreEqual("null", SelectedName());

            dispatcher.enabled = true;
            Assert.AreEqual("interactable/-", Flags(baseScope), "ディスパッチャが有効になった時点で適用される");
            Assert.AreEqual("Dialog/Button", SelectedName());

            dispatcher.enabled = false;
            Assert.AreEqual("interactable/raycast", Flags(baseScope), "無効になったら元に戻る");
            Assert.IsNotNull(dialog);
        }

        [Test]
        public void CoveredScope_NavigationIsSuppressed_AndRestored()
        {
            CreateScope("Base", out var baseButton);
            var explicitNavigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp = baseButton };
            baseButton.navigation = explicitNavigation;

            var dialog = CreateScope("Dialog", out var dialogButton);
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.None, baseButton.navigation.mode, "下になったら移動先から外れる");
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Automatic, dialogButton.navigation.mode);

            Object.DestroyImmediate(dialog.gameObject);
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Explicit, baseButton.navigation.mode, "一番上に戻ったら元に戻る");
            Assert.AreEqual(baseButton, baseButton.navigation.selectOnUp);
        }

        [Test]
        public void NavigationChangedWhileCovered_IsKept()
        {
            CreateScope("Base", out var baseButton);
            var dialog = CreateScope("Dialog", out _);

            baseButton.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Vertical };
            Object.DestroyImmediate(dialog.gameObject);

            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Vertical, baseButton.navigation.mode, "下になっている間に利用側が書き換えた値は上書きしない");
        }

        [Test]
        public void NestedTopScope_NavigationIsNotSuppressed()
        {
            var parent = CreateScope("Parent", out var parentButton);
            var childObject = CreateChild("Child", parent.transform);
            childObject.SetActive(false);
            childObject.AddComponent<CanvasGroup>();
            var childButton = CreateButton("Button", childObject.transform);
            childObject.AddComponent<UIInputModalScope>();
            childObject.SetActive(true);

            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.None, parentButton.navigation.mode);
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Automatic, childButton.navigation.mode, "親が下になっても、一番上の子の配下は移動先のまま");

            childObject.SetActive(false);
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Automatic, parentButton.navigation.mode);
        }

        // ---------- 下のスコープへ選択が移った場合 ----------

        static void Place(Component target, float y)
        {
            var rect = (RectTransform)target.transform;
            rect.sizeDelta = new Vector2(100f, 50f);
            rect.anchoredPosition = new Vector2(0f, y);
        }

        [UnityTest]
        public IEnumerator SelectionMovedToCoveredScope_IsRevertedInTheSameFrame()
        {
            CreateScope("Base", out var baseButton);
            CreateScope("Dialog", out _);
            Assert.AreEqual("Dialog/Button", SelectedName());
            yield return null;

            eventSystem.SetSelectedGameObject(baseButton.gameObject);
            yield return new WaitForEndOfFrame();

            Assert.AreEqual("Dialog/Button", SelectedName(), "描画の前に、元の選択へ戻っている");
        }

        [UnityTest]
        public IEnumerator SelectionMovedToCoveredScope_WithNothingToRevertTo_IsCleared()
        {
            CreateScope("Base", out var baseButton);
            CreateScope("Dialog", out _, selectFirst: false);
            yield return null;

            eventSystem.SetSelectedGameObject(baseButton.gameObject);
            yield return null;

            Assert.AreEqual("null", SelectedName());
        }

        [UnityTest]
        public IEnumerator SelectionOutsideAnyScope_IsNotReverted()
        {
            CreateScope("Dialog", out _);
            var outside = CreateButton("Outside", canvas.transform);
            yield return null;

            eventSystem.SetSelectedGameObject(outside.gameObject);
            yield return null;

            Assert.AreEqual("Canvas/Outside", SelectedName(), "スコープに属さないものの選択には手を出さない");
        }

        [UnityTest]
        public IEnumerator NavigationKey_DoesNotMoveSelectionToCoveredScope()
        {
            var baseCounter = new Counter();
            var baseScope = CreateScope("Base", out var baseButton);
            baseButton.onClick.AddListener(() => ++baseCounter.value);
            var dialog = CreateScope("Dialog", out var first);
            var second = CreateButton("Second", dialog.transform);
            // 上から Dialog/Button, Dialog/Second, Base/Button の順に並べる
            Place(first, 200f);
            Place(second, 0f);
            Place(baseButton, -200f);
            Assert.IsNotNull(baseScope);
            yield return null;

            yield return Press(Key.DownArrow);
            Assert.AreEqual("Dialog/Second", SelectedName(), "同じスコープの中は移動できる(キーのナビゲーションが効いていることの確認)");

            yield return Press(Key.DownArrow);
            Assert.AreEqual("Dialog/Second", SelectedName(), "下のスコープへは移動しない");

            // 移動と決定を同じフレームに押しても、下のスコープのボタンは押されない
            InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(Key.DownArrow, Key.Enter));
            yield return Frames();
            yield return Up();
            Assert.AreEqual(0, baseCounter.value);
            Assert.AreEqual("Dialog/Second", SelectedName());
        }
    }
}
