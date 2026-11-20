#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TSKT.Tests
{
    public class InputBlockerTests : UIInputTestBase
    {
        [Test]
        public void Block_DisablesInputModule_UntilAllHandlesAreDisposed()
        {
            Assert.IsFalse(InputBlocker.IsBlocked);
            Assert.IsTrue(module.enabled);

            var first = Block();
            var second = Block();
            Assert.IsTrue(InputBlocker.IsBlocked);
            Assert.IsFalse(module.enabled);

            first.Dispose();
            Assert.IsTrue(InputBlocker.IsBlocked, "ハンドルが残っている間はブロックが続く");
            Assert.IsFalse(module.enabled);

            second.Dispose();
            Assert.IsFalse(InputBlocker.IsBlocked);
            Assert.IsTrue(module.enabled);
        }

        [Test]
        public void DisposingTwice_DoesNotReleaseOtherHandles()
        {
            var first = Block();
            var second = Block();

            first.Dispose();
            first.Dispose();
            Assert.IsTrue(InputBlocker.IsBlocked);

            second.Dispose();
            Assert.IsFalse(InputBlocker.IsBlocked);
        }

        [Test]
        public void ModuleDisabledByUser_IsNotEnabledOnRelease()
        {
            module.enabled = false;

            var handle = Block();
            handle.Dispose();

            Assert.IsFalse(module.enabled, "元から無効だったモジュールは有効にしない");
        }

        [Test]
        public void WorksWithoutDispatcher()
        {
            dispatcher.enabled = false;

            var handle = Block();
            Assert.IsTrue(InputBlocker.IsBlocked);
            Assert.IsFalse(module.enabled);

            handle.Dispose();
            Assert.IsFalse(InputBlocker.IsBlocked);
            Assert.IsTrue(module.enabled);
        }

        [Test]
        public void EventSystemAndSelection_AreKeptWhileBlocked()
        {
            var button = CreateButton("Button", canvas.transform);
            eventSystem.SetSelectedGameObject(button.gameObject);

            var handle = Block();
            Assert.AreEqual(eventSystem, UnityEngine.EventSystems.EventSystem.current);
            Assert.AreEqual(button.gameObject, eventSystem.currentSelectedGameObject);
            handle.Dispose();
        }

        [UnityTest]
        public IEnumerator Dispatcher_DoesNotNotifyWhileBlocked()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            var handle = Block();
            yield return Press(Key.Digit1);
            Assert.AreEqual("", recorder.Take());

            handle.Dispose();
            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", recorder.Take());
        }

        [UnityTest]
        public IEnumerator SubmitKeyOnSelectedButton_IsIgnoredWhileBlocked()
        {
            var counter = new Counter();
            var button = CreateButton("Button", canvas.transform, counter);
            eventSystem.SetSelectedGameObject(button.gameObject);
            // モジュールが動き始めるまで待つ
            yield return Frames();

            yield return Press(Key.Enter);
            Assert.AreEqual(1, counter.value, "対照: ブロックしていなければEnterで押せる");

            var handle = Block();
            yield return Press(Key.Enter);
            Assert.AreEqual(1, counter.value, "ブロック中はEnterが効かない");

            handle.Dispose();
            yield return Press(Key.Enter);
            Assert.AreEqual(2, counter.value, "解除後は再び効く");
        }
    }
}
