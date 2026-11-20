#nullable enable
using UnityEngine.InputSystem;

namespace TSKT
{
    /// <summary>
    /// 押下と解放も受け取りたいレシーバーが実装する。長押しや、押している間だけの表示などに使う。
    ///
    /// 押下の瞬間に選ばれたレシーバーが、そのアクションを離すまで捕捉する。
    /// OnPressedを受け取ったレシーバーには、必ず一度だけOnReleasedが届く。
    /// </summary>
    public interface IUIInputPressReceiver : IUIInputReceiver
    {
        /// <summary>
        /// アクションが押された。このあとOnPerformedとOnReleasedも、このレシーバーに届く。
        /// </summary>
        void OnPressed(InputAction action);

        /// <summary>
        /// アクションが離された、または捕捉が中断された。
        /// canceledがtrueなのは、離す前に以下が起きた場合。
        /// - レシーバーがUnregisterされた
        /// - CanHandleがfalseになった
        /// - 上にUIInputModalScopeが開くなどして操作不可になった
        /// - InputBlockerがブロックを始めた
        /// - UIInputDispatcherが無効になった
        /// - 解放を取りこぼしたまま、同じアクションがもう一度押された
        /// </summary>
        void OnReleased(InputAction action, bool canceled);
    }
}
