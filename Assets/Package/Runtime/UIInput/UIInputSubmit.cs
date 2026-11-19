#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TSKT
{
    /// <summary>
    /// アクションが発火したら、自身のGameObjectにSubmitイベントを送る。
    /// Buttonなら押下の表示を経てonClickが呼ばれ、Toggleなら切り替わる。
    /// ISubmitHandlerを実装した自作コンポーネントにも届く。
    /// 選択中のオブジェクトにはUIモジュールがSubmitを送るので、UI/Submitを割り当てると二重に実行される。
    /// </summary>
    public class UIInputSubmit : MonoBehaviour, IUIInputReceiver
    {
        [SerializeField]
        InputActionReference action = default!;

        InputAction? registeredAction;
        Selectable? selectable;
        bool selectableCached;

        public Transform Transform => transform;

        public bool CanHandle
        {
            get
            {
                if (!isActiveAndEnabled)
                {
                    return false;
                }
                if (!selectableCached)
                {
                    selectableCached = true;
                    TryGetComponent(out selectable);
                }
                // Selectableがある場合は、CanvasGroupを含めて操作可能かを見る
                if (selectable)
                {
                    return selectable!.IsActive() && selectable.IsInteractable();
                }
                return true;
            }
        }

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
            ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }
}
