#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;

namespace TSKT
{
    /// <summary>
    /// UIInputDispatcherからアクションの発火を受け取る。
    /// 同じアクションに複数のレシーバーが登録されている場合、後から登録されたものが優先される。
    /// OnEnableでUIInputDispatcher.Register、OnDisableでUnregisterを呼ぶこと。
    /// </summary>
    public interface IUIInputReceiver
    {
        /// <summary>
        /// UIInputModalScopeによる操作可否の判定に使う。
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 今アクションを受け取れる状態か。falseの場合は次の候補に回る。
        /// </summary>
        bool CanHandle { get; }

        void OnPerformed(InputAction action);
    }
}
