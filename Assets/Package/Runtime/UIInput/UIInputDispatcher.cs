#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TSKT
{
    /// <summary>
    /// UIInputの起点。シーンに手動で置く(EventSystemと同じオブジェクトなど)。
    /// 有効なUIInputDispatcherがひとつもない間は、以下が動かない。
    /// - IUIInputReceiverへの通知は行われない
    /// - UIInputModalScopeは下のスコープを操作不可にしない
    /// InputBlockerだけは、UIInputDispatcherがなくても単体で動く。
    ///
    /// 登録されたアクションを監視し、発火したらレシーバーのうちひとつにだけ通知する。
    /// 通知先は、UIInputModalScopeで操作可能かつCanHandleがtrueのもののうち、
    /// 一番後に登録されたものが選ばれる。
    /// InputBlockerがブロック中は通知しない。
    /// </summary>
    // 同じシーンの他のコンポーネントより先に有効になるようにする
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public class UIInputDispatcher : MonoBehaviour
    {
        static readonly List<UIInputDispatcher> instances = new();
        static readonly Dictionary<InputAction, List<IUIInputReceiver>> receivers = new();
        static readonly List<InputAction> performedActions = new();
        static bool warned;

        /// <summary>
        /// 有効なUIInputDispatcherがあるか。falseの間は、レシーバーへの通知とUIInputModalScopeが動かない。
        /// </summary>
        public static bool Active => instances.Count > 0;

        void OnEnable()
        {
            instances.Add(this);
            if (instances.Count == 1)
            {
                NotifyActiveChanged();
            }
        }

        void OnDisable()
        {
            instances.Remove(this);
            if (instances.Count == 0)
            {
                NotifyActiveChanged();
            }
        }

        static void NotifyActiveChanged()
        {
            UIInputModalScope.OnDispatcherActiveChanged();
        }

        /// <summary>
        /// UIInputDispatcherがない状態でレシーバーやUIInputModalScopeが使われたことを、再生中に一度だけ警告する。
        /// </summary>
        internal static void WarnIfInactive(Object? context)
        {
            if (Active || warned || !Application.isPlaying)
            {
                return;
            }
            warned = true;
            Debug.LogWarning("UIInputDispatcher is not found. UIInputSubmit and UIInputModalScope do nothing until an UIInputDispatcher is placed in the scene.", context);
        }

        /// <summary>
        /// actionが無効な場合は有効にする。
        /// 他の利用者がいるかを判別できないため、Unregisterしても無効には戻さない。
        /// </summary>
        public static void Register(InputAction action, IUIInputReceiver receiver)
        {
            if (!receivers.TryGetValue(action, out var list))
            {
                list = new List<IUIInputReceiver>();
                receivers.Add(action, list);
            }
            if (!list.Contains(receiver))
            {
                list.Add(receiver);
            }
            if (!action.enabled)
            {
                action.Enable();
            }
            WarnIfInactive(receiver as Object);
        }

        public static void Unregister(InputAction action, IUIInputReceiver receiver)
        {
            if (!receivers.TryGetValue(action, out var list))
            {
                return;
            }
            list.Remove(receiver);
            if (list.Count == 0)
            {
                receivers.Remove(action);
            }
        }

        void Update()
        {
            // 複数置かれていても、処理するのはひとつだけ
            if (instances[0] != this)
            {
                return;
            }
            if (InputBlocker.IsBlocked)
            {
                return;
            }

            // 通知中に登録内容が変わってもいいように、発火したアクションを先に集める
            performedActions.Clear();
            foreach (var action in receivers.Keys)
            {
                if (action.WasPerformedThisFrame())
                {
                    performedActions.Add(action);
                }
            }

            foreach (var action in performedActions)
            {
                // 先に通知したレシーバーによって登録が外れている場合がある
                if (!receivers.TryGetValue(action, out var list))
                {
                    continue;
                }
                var receiver = SelectReceiver(list);
                receiver?.OnPerformed(action);
            }
            performedActions.Clear();
        }

        static IUIInputReceiver? SelectReceiver(List<IUIInputReceiver> list)
        {
            // 後から登録されたものを優先する
            for (int i = list.Count - 1; i >= 0; --i)
            {
                var it = list[i];
                // 破棄済みのコンポーネントが残っていた場合に備える
                if (it is Object obj && !obj)
                {
                    continue;
                }
                if (!it.CanHandle)
                {
                    continue;
                }
                if (!UIInputModalScope.IsOperable(it.Transform))
                {
                    continue;
                }
                return it;
            }
            return null;
        }

        // ReloadDomainなしで再生した場合に前回の状態を持ち越さない
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            instances.Clear();
            receivers.Clear();
            performedActions.Clear();
            warned = false;
        }
    }
}
