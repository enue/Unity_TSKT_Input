#nullable enable
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TSKT
{
    /// <summary>
    /// アクションが発火したら、UnityEventを呼ぶ。
    /// Selectableを持たないオブジェクトでも使える。
    /// 同じアクションに複数のレシーバーが登録されている場合に呼ばれるのは、
    /// UIInputModalScopeで操作可能なもののうち、一番後に有効になったひとつだけ。
    /// </summary>
    public class UIInputEvent : MonoBehaviour, IUIInputReceiver
    {
        [SerializeField]
        InputActionReference action = default!;

        [SerializeField]
        UnityEvent<InputAction> performed = new();

        /// <summary>
        /// アクションが発火した時に呼ばれる。引数は発火したアクション。
        /// </summary>
        public UnityEvent<InputAction> Performed => performed;

        InputAction? registeredAction;

        public Transform Transform => transform;
        public bool CanHandle => isActiveAndEnabled;

        void OnEnable()
        {
            registeredAction = action ? action.action : null;
            if (registeredAction == null)
            {
                Debug.LogWarning("action is not assigned : " + name, this);
                return;
            }
            UIInputDispatcher.Register(registeredAction, this);
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
            performed.Invoke(action);
        }
    }
}
