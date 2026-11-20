#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TSKT.Tests
{
    public class UIInputSubmitTests : UIInputTestBase
    {
        [UnityTest]
        public IEnumerator Action_ClicksButton()
        {
            var counter = new Counter();
            CreateSubmitButton("Button", canvas.transform, action1, counter);

            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);

            yield return Press(Key.Digit2);
            Assert.AreEqual(1, counter.value, "別のアクションでは押されない");

            yield return Press(Key.Digit1);
            Assert.AreEqual(2, counter.value);
        }

        [UnityTest]
        public IEnumerator NonInteractableButton_IgnoresAction()
        {
            var counter = new Counter();
            var button = CreateSubmitButton("Button", canvas.transform, action1, counter);

            button.interactable = false;
            yield return Press(Key.Digit1);
            Assert.AreEqual(0, counter.value);

            button.interactable = true;
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);
        }

        [UnityTest]
        public IEnumerator ButtonUnderNonInteractableCanvasGroup_IgnoresAction()
        {
            var counter = new Counter();
            var groupObject = CreateChild("Group", canvas.transform);
            var group = groupObject.AddComponent<CanvasGroup>();
            CreateSubmitButton("Button", groupObject.transform, action1, counter);

            group.interactable = false;
            yield return Press(Key.Digit1);
            Assert.AreEqual(0, counter.value);

            group.interactable = true;
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);
        }

        [UnityTest]
        public IEnumerator InactiveButton_IgnoresAction()
        {
            var counter = new Counter();
            var button = CreateSubmitButton("Button", canvas.transform, action1, counter);

            button.gameObject.SetActive(false);
            yield return Press(Key.Digit1);
            Assert.AreEqual(0, counter.value);

            button.gameObject.SetActive(true);
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);
        }

        [UnityTest]
        public IEnumerator Action_TogglesToggle()
        {
            var go = CreateChild("Toggle", canvas.transform);
            go.SetActive(false);
            var toggle = go.AddComponent<Toggle>();
            toggle.isOn = false;
            AddSubmit(go, action1);
            go.SetActive(true);

            yield return Press(Key.Digit1);
            Assert.IsTrue(toggle.isOn);

            yield return Press(Key.Digit1);
            Assert.IsFalse(toggle.isOn);
        }

        [UnityTest]
        public IEnumerator SameAction_ButtonEnabledLaterWins()
        {
            var first = new Counter();
            var second = new Counter();
            CreateSubmitButton("First", canvas.transform, action1, first);
            var secondButton = CreateSubmitButton("Second", canvas.transform, action1, second);

            yield return Press(Key.Digit1);
            Assert.AreEqual((0, 1), (first.value, second.value));

            secondButton.interactable = false;
            yield return Press(Key.Digit1);
            Assert.AreEqual((1, 1), (first.value, second.value), "後から有効になったほうが押せないときは、前のものに回る");

            secondButton.gameObject.SetActive(false);
            yield return Press(Key.Digit1);
            Assert.AreEqual((2, 1), (first.value, second.value));
        }

        [UnityTest]
        public IEnumerator ButtonInLowerScope_IgnoresAction()
        {
            var baseCounter = new Counter();
            var dialogCounter = new Counter();
            var baseScope = CreateScope("Base", out _);
            CreateSubmitButton("Shortcut", baseScope.transform, action1, baseCounter);

            yield return Press(Key.Digit1);
            Assert.AreEqual(1, baseCounter.value);

            var dialog = CreateScope("Dialog", out _);
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, baseCounter.value, "上にスコープが開いている間は押されない");

            CreateSubmitButton("Shortcut", dialog.transform, action1, dialogCounter);
            yield return Press(Key.Digit1);
            Assert.AreEqual((1, 1), (baseCounter.value, dialogCounter.value));

            Object.DestroyImmediate(dialog.gameObject);
            yield return Press(Key.Digit1);
            Assert.AreEqual(2, baseCounter.value);
        }
    }
}
