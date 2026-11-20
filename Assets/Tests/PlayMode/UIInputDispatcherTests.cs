#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TSKT.Tests
{
    public class UIInputDispatcherTests : UIInputTestBase
    {
        [UnityTest]
        public IEnumerator Tap_NotifiesPressedPerformedReleasedInOrder()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Press(Key.Digit1);

            Assert.AreEqual("PFR", recorder.Take());
        }

        [UnityTest]
        public IEnumerator Hold_ReleasedIsNotifiedOnlyWhenKeyGoesUp()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Down(Key.Digit1);
            Assert.AreEqual("PF", recorder.Take());

            yield return Frames(10);
            Assert.AreEqual("", recorder.Take(), "押している間は何も届かない");

            yield return Up();
            Assert.AreEqual("R", recorder.Take());
        }

        [UnityTest]
        public IEnumerator OtherAction_IsNotNotified()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Press(Key.Digit2);

            Assert.AreEqual("", recorder.Take());
        }

        [UnityTest]
        public IEnumerator LaterRegisteredReceiver_Wins()
        {
            var first = AddRecorder(action1, canvas.transform);
            var second = AddRecorder(action1, canvas.transform);

            yield return Press(Key.Digit1);

            Assert.AreEqual("", first.Take());
            Assert.AreEqual("PFR", second.Take());
        }

        [UnityTest]
        public IEnumerator ReceiverThatCannotHandle_IsSkipped()
        {
            var first = AddRecorder(action1, canvas.transform);
            var second = AddRecorder(action1, canvas.transform);
            second.canHandle = false;

            yield return Press(Key.Digit1);

            Assert.AreEqual("PFR", first.Take());
            Assert.AreEqual("", second.Take());
        }

        [UnityTest]
        public IEnumerator Capture_StaysWithPressedReceiver()
        {
            var first = AddRecorder(action1, canvas.transform);

            yield return Down(Key.Digit1);
            var second = AddRecorder(action1, canvas.transform);
            yield return Frames();
            yield return Up();

            Assert.AreEqual("PFR", first.Take(), "押した相手に解放まで届く");
            Assert.AreEqual("", second.Take(), "押している途中で登録されても横取りしない");

            yield return Press(Key.Digit1);
            Assert.AreEqual("", first.Take());
            Assert.AreEqual("PFR", second.Take(), "次の押下からは後から登録されたほう");
        }

        [UnityTest]
        public IEnumerator Unregister_WhilePressed_CancelsImmediately()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Down(Key.Digit1);
            recorder.Take();
            UIInputDispatcher.Unregister(action1, recorder);
            Assert.AreEqual("C", recorder.Take());

            yield return Up();
            Assert.AreEqual("", recorder.Take(), "中断後の解放は届かない");
        }

        [UnityTest]
        public IEnumerator CanHandleBecomesFalse_WhilePressed_Cancels()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Down(Key.Digit1);
            recorder.Take();
            recorder.canHandle = false;
            yield return Frames();
            Assert.AreEqual("C", recorder.Take());

            recorder.canHandle = true;
            yield return Frames();
            Assert.AreEqual("", recorder.Take(), "押したまま復帰しても再開しない");

            yield return Up();
            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", recorder.Take(), "離してから押し直せば動く");
        }

        [UnityTest]
        public IEnumerator DispatcherDisabled_WhilePressed_CancelsImmediately()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Down(Key.Digit1);
            recorder.Take();
            dispatcher.enabled = false;
            Assert.AreEqual("C", recorder.Take());

            dispatcher.enabled = true;
            yield return Up();
            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", recorder.Take());
        }

        [UnityTest]
        public IEnumerator WithoutDispatcher_NothingIsNotified()
        {
            var recorder = AddRecorder(action1, canvas.transform);
            dispatcher.enabled = false;
            Assert.IsFalse(UIInputDispatcher.Active);

            yield return Press(Key.Digit1);

            Assert.AreEqual("", recorder.Take());
        }

        [UnityTest]
        public IEnumerator Blocked_WhilePressed_CancelsAndStaysSilentUntilReleased()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            yield return Down(Key.Digit1);
            recorder.Take();
            var handle = Block();
            yield return Frames();
            Assert.AreEqual("C", recorder.Take());

            handle.Dispose();
            yield return Frames(5);
            Assert.AreEqual("", recorder.Take(), "解除後も、離すまで届かない");

            yield return Up();
            Assert.AreEqual("", recorder.Take());

            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", recorder.Take());
        }

        [UnityTest]
        public IEnumerator PressedDuringBlock_IsIgnoredUntilReleased()
        {
            var recorder = AddRecorder(action1, canvas.transform);

            var handle = Block();
            yield return Down(Key.Digit1);
            handle.Dispose();
            yield return Frames(5);
            Assert.AreEqual("", recorder.Take(), "ブロック中に押されたものは、解除後も届かない");

            yield return Up();
            yield return Press(Key.Digit1);
            Assert.AreEqual("PFR", recorder.Take());
        }

        [UnityTest]
        public IEnumerator HoldInteraction_PerformsAfterDuration()
        {
            var recorder = AddRecorder(holdAction, canvas.transform);

            yield return Down(Key.H);
            Assert.AreEqual("P", recorder.Take());

            yield return new WaitForSecondsRealtime(0.5f);
            Assert.AreEqual("F", recorder.Take());

            yield return Up();
            Assert.AreEqual("R", recorder.Take());
        }

        [UnityTest]
        public IEnumerator HoldInteraction_ReleasedEarly_DoesNotPerform()
        {
            var recorder = AddRecorder(holdAction, canvas.transform);

            yield return Down(Key.H);
            yield return Up();

            Assert.AreEqual("PR", recorder.Take());
        }

        [UnityTest]
        public IEnumerator HoldInteraction_CanceledBeforeDuration_NeverPerforms()
        {
            var recorder = AddRecorder(holdAction, canvas.transform);

            yield return Down(Key.H);
            recorder.Take();
            recorder.canHandle = false;
            yield return Frames();
            recorder.canHandle = true;
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.AreEqual("C", recorder.Take(), "中断されたあとは、時間が過ぎても発火しない");
            yield return Up();
        }
    }
}
