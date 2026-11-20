#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace TSKT.Tests
{
    /// <summary>
    /// 同じフレームに複数のアクションが押された場合のテスト。
    /// マウスでは起きないが、キーボードやゲームパッドでは起こりうる。
    /// </summary>
    public class UIInputSimultaneousTests : UIInputTestBase
    {
        IEnumerator PressTogether(Key first, Key second)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(first, second));
            yield return Frames();
            yield return Up();
        }

        [UnityTest]
        public IEnumerator SameFrame_OnlyTheReceiverRegisteredLaterIsPerformed()
        {
            var first = new Counter();
            var second = new Counter();
            CreateSubmitButton("First", canvas.transform, action1, first);
            CreateSubmitButton("Second", canvas.transform, action2, second);

            yield return PressTogether(Key.Digit1, Key.Digit2);

            Assert.AreEqual((0, 1), (first.value, second.value), "同じフレームに実行されるのはひとつだけ");
        }

        [UnityTest]
        public IEnumerator SameFrame_RegistrationOrderDecides_NotActionOrder()
        {
            var first = new Counter();
            var second = new Counter();
            // アクションの並びを逆にしても、後から登録したほうが選ばれる
            CreateSubmitButton("First", canvas.transform, action2, first);
            CreateSubmitButton("Second", canvas.transform, action1, second);

            yield return PressTogether(Key.Digit1, Key.Digit2);

            Assert.AreEqual((0, 1), (first.value, second.value));
        }

        [UnityTest]
        public IEnumerator DifferentFrames_BothArePerformed()
        {
            var first = new Counter();
            var second = new Counter();
            CreateSubmitButton("First", canvas.transform, action1, first);
            CreateSubmitButton("Second", canvas.transform, action2, second);

            // 1を押したまま2を押す
            yield return Down(Key.Digit1);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Digit1, Key.Digit2));
            yield return Frames();
            yield return Up();

            Assert.AreEqual((1, 1), (first.value, second.value), "フレームが違えば両方実行される");
        }

        [UnityTest]
        public IEnumerator SameFrame_OkAndCancel_ThatDestroyTheDialog_OnlyOneRuns()
        {
            var ok = new Counter();
            var cancel = new Counter();
            var dialog = CreateScope("Dialog", out _);
            var okButton = CreateSubmitButton("Ok", dialog.transform, action1, ok);
            var cancelButton = CreateSubmitButton("Cancel", dialog.transform, action2, cancel);
            okButton.onClick.AddListener(() => Object.Destroy(dialog.gameObject));
            cancelButton.onClick.AddListener(() => Object.Destroy(dialog.gameObject));

            yield return PressTogether(Key.Digit1, Key.Digit2);

            Assert.AreEqual(1, ok.value + cancel.value, "OKとキャンセルの両方が実行されてはいけない");
            Assert.AreEqual((0, 1), (ok.value, cancel.value), "後から登録されたほうが選ばれる");
        }

        [UnityTest]
        public IEnumerator SameFrame_PerformedHandlerBlocksInput_OtherIsNotPerformed()
        {
            var first = new Counter();
            var second = new Counter();
            // どちらが実行されても、その処理の中で入力をブロックする(閉じる演出の開始などを想定)
            var firstButton = CreateSubmitButton("First", canvas.transform, action1, first);
            var secondButton = CreateSubmitButton("Second", canvas.transform, action2, second);
            firstButton.onClick.AddListener(() => Block());
            secondButton.onClick.AddListener(() => Block());

            yield return PressTogether(Key.Digit1, Key.Digit2);

            Assert.AreEqual((0, 1), (first.value, second.value), "ブロックを始めた処理のあとに、もう一方が実行されてはいけない");
        }

        [UnityTest]
        public IEnumerator SameFrame_PressedHandlerBlocksInput_NothingIsPerformed()
        {
            var counter = new Counter();
            var blocker = new BlockOnPress(canvas.transform, () => Block());
            UIInputDispatcher.Register(action2, blocker);
            // ボタンを後から登録して、発火の通知先に選ばれる側にする
            CreateSubmitButton("Button", canvas.transform, action1, counter);

            yield return PressTogether(Key.Digit1, Key.Digit2);
            UIInputDispatcher.Unregister(action2, blocker);

            Assert.AreEqual(0, counter.value, "押下の通知の中でブロックが始まったら、発火は通知しない");
        }

        sealed class BlockOnPress : IUIInputPressReceiver
        {
            readonly Transform transform;
            readonly System.Action onPressed;

            public BlockOnPress(Transform transform, System.Action onPressed)
            {
                this.transform = transform;
                this.onPressed = onPressed;
            }

            public Transform Transform => transform;
            public bool CanHandle => true;
            public void OnPressed(InputAction action) => onPressed();
            public void OnPerformed(InputAction action) { }
            public void OnReleased(InputAction action, bool canceled) { }
        }

        [UnityTest]
        public IEnumerator SameFrame_FirstOpensScope_OnlyOneRuns()
        {
            var opened = new Counter();
            var other = new Counter();
            var baseScope = CreateScope("Base", out _);
            var opener = CreateSubmitButton("Opener", baseScope.transform, action1, opened);
            opener.onClick.AddListener(() => CreateScope("Dialog", out _));
            CreateSubmitButton("Other", baseScope.transform, action2, other);

            yield return PressTogether(Key.Digit1, Key.Digit2);

            Assert.AreEqual(1, opened.value + other.value);
        }

        [UnityTest]
        public IEnumerator SameFrame_PressReceivers_BothGetPressedAndReleased_OnlyOneIsPerformed()
        {
            var first = AddRecorder(action1, canvas.transform);
            var second = AddRecorder(action2, canvas.transform);

            yield return PressTogether(Key.Digit1, Key.Digit2);

            // 押下と解放は両方に届く(方向キーのように同時押しを見たい用途のため)。発火はひとつだけ
            Assert.AreEqual("PR", first.Take());
            Assert.AreEqual("PFR", second.Take());
        }

        [UnityTest]
        public IEnumerator SameFrame_WinnerThatCannotHandle_IsSkipped()
        {
            var first = new Counter();
            var second = new Counter();
            CreateSubmitButton("First", canvas.transform, action1, first);
            var secondButton = CreateSubmitButton("Second", canvas.transform, action2, second);
            secondButton.interactable = false;

            yield return PressTogether(Key.Digit1, Key.Digit2);

            Assert.AreEqual((1, 0), (first.value, second.value), "後から登録されたほうが押せないなら、もう一方が実行される");
        }
    }
}
