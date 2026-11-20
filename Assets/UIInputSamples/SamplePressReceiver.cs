#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TSKT.Samples
{
    /// <summary>
    /// IUIInputPressReceiverを実装して、押下と解放を受け取るサンプル。
    /// 押している間は経過時間を表示し、離したら(または中断されたら)結果を表示する。
    /// </summary>
    public class SamplePressReceiver : MonoBehaviour, IUIInputPressReceiver
    {
        [SerializeField]
        InputActionReference action = default!;

        [SerializeField]
        Text label = default!;

        [SerializeField]
        string displayName = "Hold";

        InputAction? registeredAction;
        bool pressing;
        float pressedTime;
        string lastResult = "not pressed yet";

        public Transform Transform => transform;
        public bool CanHandle => isActiveAndEnabled;

        void OnEnable()
        {
            registeredAction = action.action;
            UIInputDispatcher.Register(registeredAction, this);
        }

        void OnDisable()
        {
            if (registeredAction != null)
            {
                // 押している最中なら、ここでOnReleased(canceled: true)が呼ばれる
                UIInputDispatcher.Unregister(registeredAction, this);
                registeredAction = null;
            }
        }

        public void OnPressed(InputAction action)
        {
            pressing = true;
            pressedTime = Time.unscaledTime;
        }

        public void OnPerformed(InputAction action)
        {
            // 押下と解放だけを使うので何もしない
        }

        public void OnReleased(InputAction action, bool canceled)
        {
            pressing = false;
            var duration = Time.unscaledTime - pressedTime;
            lastResult = canceled
                ? $"canceled after {duration:0.0}s"
                : $"released after {duration:0.0}s";
        }

        void Update()
        {
            label.text = pressing
                ? $"{displayName}: pressing {Time.unscaledTime - pressedTime:0.0}s"
                : $"{displayName}: {lastResult}";
        }
    }
}
