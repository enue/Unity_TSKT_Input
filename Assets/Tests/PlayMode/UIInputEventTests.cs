#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TSKT.Tests
{
    public class UIInputEventTests : UIInputTestBase
    {
        UIInputEvent CreateEvent(string name, Transform parent, InputAction action, Counter counter)
        {
            var go = CreateChild(name, parent);
            go.SetActive(false);
            var target = go.AddComponent<UIInputEvent>();
            SetPrivateField(target, "action", CreateReference(action));
            target.Performed.AddListener(_ => ++counter.value);
            go.SetActive(true);
            return target;
        }

        [UnityTest]
        public IEnumerator Action_InvokesEvent_WithPerformedAction()
        {
            var counter = new Counter();
            var target = CreateEvent("Event", canvas.transform, action1, counter);
            InputAction? received = null;
            target.Performed.AddListener(x => received = x);

            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);
            Assert.AreEqual(action1, received);

            yield return Press(Key.Digit2);
            Assert.AreEqual(1, counter.value, "別のアクションでは呼ばれない");
        }

        [UnityTest]
        public IEnumerator WorksOutsideCanvas()
        {
            var counter = new Counter();
            CreateEvent("Event", root.transform, action1, counter);

            yield return Press(Key.Digit1);

            Assert.AreEqual(1, counter.value);
        }

        [UnityTest]
        public IEnumerator DisabledOrInactive_IsNotInvoked()
        {
            var counter = new Counter();
            var target = CreateEvent("Event", canvas.transform, action1, counter);

            target.enabled = false;
            yield return Press(Key.Digit1);
            Assert.AreEqual(0, counter.value);

            target.enabled = true;
            target.gameObject.SetActive(false);
            yield return Press(Key.Digit1);
            Assert.AreEqual(0, counter.value);

            target.gameObject.SetActive(true);
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);
        }

        [UnityTest]
        public IEnumerator SameAction_OnlyTheOneEnabledLaterIsInvoked()
        {
            var first = new Counter();
            var second = new Counter();
            CreateEvent("First", canvas.transform, action1, first);
            var secondEvent = CreateEvent("Second", canvas.transform, action1, second);

            yield return Press(Key.Digit1);
            Assert.AreEqual((0, 1), (first.value, second.value));

            secondEvent.gameObject.SetActive(false);
            yield return Press(Key.Digit1);
            Assert.AreEqual((1, 1), (first.value, second.value));
        }

        [UnityTest]
        public IEnumerator InLowerScope_IsNotInvoked()
        {
            var counter = new Counter();
            var baseScope = CreateScope("Base", out _);
            CreateEvent("Event", baseScope.transform, action1, counter);

            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);

            var dialog = CreateScope("Dialog", out _);
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value, "上にスコープが開いている間は呼ばれない");

            Object.DestroyImmediate(dialog.gameObject);
            yield return Press(Key.Digit1);
            Assert.AreEqual(2, counter.value);
        }

        [UnityTest]
        public IEnumerator Blocked_IsNotInvoked()
        {
            var counter = new Counter();
            CreateEvent("Event", canvas.transform, action1, counter);

            var handle = Block();
            yield return Press(Key.Digit1);
            Assert.AreEqual(0, counter.value);

            handle.Dispose();
            yield return Press(Key.Digit1);
            Assert.AreEqual(1, counter.value);
        }
    }
}
