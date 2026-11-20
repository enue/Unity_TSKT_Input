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
    /// 登録されたアクションを監視し、レシーバーのうちひとつにだけ通知する。
    /// 通知先は押下の瞬間に決まる。UIInputModalScopeで操作可能かつCanHandleがtrueのもののうち、
    /// 一番後に登録されたものが選ばれ、離されるまでそのアクションを捕捉する。
    /// 捕捉中にそのレシーバーが操作できなくなった場合は中断し、離されるまでそのアクションは誰にも通知しない。
    /// InputBlockerがブロック中は通知しない。
    /// </summary>
    // 同じシーンの他のコンポーネントより先に有効になるようにする
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public class UIInputDispatcher : MonoBehaviour
    {
        sealed class Entry
        {
            public readonly InputAction action;
            public readonly List<IUIInputReceiver> receivers = new();

            /// <summary>
            /// 押下の瞬間に選ばれ、離されるまで通知を受け取るレシーバー
            /// </summary>
            public IUIInputReceiver? captured;

            /// <summary>
            /// 押されているが、通知先がいない(押下時に候補がいなかった、または捕捉が中断された)。
            /// 離されるまで、このアクションは誰にも通知しない。
            /// </summary>
            public bool suppressed;

            public Entry(InputAction action)
            {
                this.action = action;
            }
        }

        static readonly List<UIInputDispatcher> instances = new();
        static readonly Dictionary<InputAction, Entry> entries = new();
        static readonly List<Entry> buffer = new();
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
                CancelAll();
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
            if (!entries.TryGetValue(action, out var entry))
            {
                entry = new Entry(action);
                entries.Add(action, entry);
            }
            if (!entry.receivers.Contains(receiver))
            {
                entry.receivers.Add(receiver);
            }
            if (!action.enabled)
            {
                action.Enable();
            }
            WarnIfInactive(receiver as Object);
        }

        /// <summary>
        /// receiverがactionを捕捉中だった場合は、OnReleased(canceled: true)を呼んでから外す。
        /// </summary>
        public static void Unregister(InputAction action, IUIInputReceiver receiver)
        {
            if (!entries.TryGetValue(action, out var entry))
            {
                return;
            }
            entry.receivers.Remove(receiver);
            if (entry.captured == receiver)
            {
                Cancel(entry);
            }
            RemoveIfUnused(entry);
        }

        void Update()
        {
            // 複数置かれていても、処理するのはひとつだけ
            if (instances[0] != this)
            {
                return;
            }

            // 通知中に登録内容が変わってもいいように、先に写しを取る
            buffer.Clear();
            buffer.AddRange(entries.Values);
            var blocked = InputBlocker.IsBlocked;
            foreach (var entry in buffer)
            {
                Process(entry, blocked);
                RemoveIfUnused(entry);
            }
            buffer.Clear();
        }

        static void Process(Entry entry, bool blocked)
        {
            var action = entry.action;

            // 捕捉中のレシーバーが操作できなくなっていたら中断する
            if (entry.captured != null)
            {
                var captured = entry.captured;
                if (captured is Object obj && !obj)
                {
                    // 破棄済み。通知する相手がいない
                    entry.captured = null;
                    entry.suppressed = true;
                }
                else if (blocked || !captured.CanHandle || !UIInputModalScope.IsOperable(captured.Transform))
                {
                    Cancel(entry);
                }
            }

            if (blocked)
            {
                // ブロック中に押されたものは、解除後も離されるまで通知しない
                entry.suppressed = action.IsPressed();
                return;
            }

            if (action.WasPressedThisFrame())
            {
                // 解放を取りこぼしていた場合
                if (entry.captured != null)
                {
                    Cancel(entry);
                }
                var receiver = SelectReceiver(entry.receivers);
                entry.captured = receiver;
                entry.suppressed = receiver == null;
                if (receiver is IUIInputPressReceiver pressReceiver)
                {
                    pressReceiver.OnPressed(action);
                }
            }

            if (action.WasPerformedThisFrame())
            {
                // OnPressedの中で捕捉が外れている場合があるので、ここで見直す
                if (entry.captured != null)
                {
                    entry.captured.OnPerformed(action);
                }
                else if (!entry.suppressed)
                {
                    // 押下を経ずに発火するアクション
                    SelectReceiver(entry.receivers)?.OnPerformed(action);
                }
            }

            if (action.WasReleasedThisFrame())
            {
                var captured = entry.captured;
                entry.captured = null;
                entry.suppressed = false;
                if (captured is IUIInputPressReceiver pressReceiver)
                {
                    pressReceiver.OnReleased(action, canceled: false);
                }
            }
            else if (entry.captured == null && entry.suppressed && !action.IsPressed())
            {
                // 解放の瞬間を見逃した場合(ブロック中に離されたなど)
                entry.suppressed = false;
            }
        }

        /// <summary>
        /// 捕捉を中断する。離されるまで、このアクションは誰にも通知しない。
        /// </summary>
        static void Cancel(Entry entry)
        {
            var captured = entry.captured;
            if (captured == null)
            {
                return;
            }
            entry.captured = null;
            entry.suppressed = true;
            if (captured is Object obj && !obj)
            {
                return;
            }
            if (captured is IUIInputPressReceiver pressReceiver)
            {
                pressReceiver.OnReleased(entry.action, canceled: true);
            }
        }

        static void CancelAll()
        {
            buffer.Clear();
            buffer.AddRange(entries.Values);
            foreach (var entry in buffer)
            {
                Cancel(entry);
                RemoveIfUnused(entry);
            }
            buffer.Clear();
        }

        static void RemoveIfUnused(Entry entry)
        {
            if (entry.receivers.Count > 0 || entry.captured != null || entry.suppressed)
            {
                return;
            }
            if (entries.TryGetValue(entry.action, out var current) && current == entry)
            {
                entries.Remove(entry.action);
            }
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
            entries.Clear();
            buffer.Clear();
            warned = false;
        }
    }
}
