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
        public void OnlyTopScope_IsInteractable()
        {
            var baseScope = CreateScope("Base", out _);
            Assert.AreEqual(baseScope, UIInputModalScope.Top);
            Assert.AreEqual("interactable/raycast", Flags(baseScope));

            var dialog = CreateScope("Dialog", out _);
            Assert.AreEqual(dialog, UIInputModalScope.Top);
            Assert.AreEqual("-/-", Flags(baseScope));
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
            Assert.AreEqual("-/- -/- interactable/raycast", $"{Flags(a)} {Flags(b)} {Flags(c)}");

            c.gameObject.SetActive(false);
            Assert.AreEqual("-/- interactable/raycast", $"{Flags(a)} {Flags(b)}");

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
            Assert.AreEqual("-/-", Flags(baseScope));

            Object.DestroyImmediate(dialog.gameObject);
            Assert.AreEqual("interactable/-", Flags(baseScope));
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
            Assert.AreEqual("-/-", Flags(dialog));
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
            Assert.AreEqual("-/-", Flags(parent));
            Assert.IsTrue(childButton.IsInteractable(), "親が操作不可でも、一番上の子は操作できる");
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
            Assert.AreEqual("-/-", Flags(baseScope), "ディスパッチャが有効になった時点で適用される");
            Assert.AreEqual("Dialog/Button", SelectedName());

            dispatcher.enabled = false;
            Assert.AreEqual("interactable/raycast", Flags(baseScope), "無効になったら元に戻る");
            Assert.IsNotNull(dialog);
        }
    }
}
