#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TSKT.Samples
{
    /// <summary>
    /// IUIInputReceiverを自前で実装するサンプル。Selectableを持たないオブジェクトでもアクションを受け取れる。
    /// OnEnableでRegister、OnDisableでUnregisterを呼ぶ。
    /// </summary>
    public class SampleActionReceiver : MonoBehaviour, IUIInputReceiver
    {
        [SerializeField]
        InputActionReference action = default!;

        [SerializeField]
        Text label = default!;

        [SerializeField]
        string displayName = "Receiver";

        [SerializeField]
        bool accepting = true;

        InputAction? registeredAction;
        int count;

        /// <summary>
        /// falseの間はCanHandleがfalseになり、同じアクションの次の候補に通知が回る。
        /// </summary>
        public bool Accepting
        {
            get => accepting;
            set
            {
                accepting = value;
                Refresh();
            }
        }

        public Transform Transform => transform;
        public bool CanHandle => isActiveAndEnabled && accepting;

        void OnEnable()
        {
            registeredAction = action.action;
            UIInputDispatcher.Register(registeredAction, this);
            Refresh();
        }

        void OnDisable()
        {
            if (registeredAction != null)
            {
                UIInputDispatcher.Unregister(registeredAction, this);
                registeredAction = null;
            }
        }

        public void OnPerformed(InputAction action)
        {
            ++count;
            Refresh();
        }

        void Refresh()
        {
            label.text = $"{displayName}: {count}  ({(accepting ? "accepting" : "not accepting")})";
        }
    }
}
