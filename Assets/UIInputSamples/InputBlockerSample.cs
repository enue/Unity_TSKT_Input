#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace TSKT.Samples
{
    /// <summary>
    /// 一定時間だけ入力を止めるサンプル。
    /// InputBlocker.Block()の戻り値をusingで受けると、スコープを抜けた時に解除される。
    /// </summary>
    public class InputBlockerSample : MonoBehaviour
    {
        [SerializeField]
        Text status = default!;

        float remaining;

        public async void BlockForSeconds(float seconds)
        {
            try
            {
                using (InputBlocker.Block())
                {
                    remaining = seconds;
                    await Awaitable.WaitForSecondsAsync(seconds, destroyCancellationToken);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 再生停止やシーン破棄で中断された。usingによりブロックは解除済み
            }
        }

        void Update()
        {
            if (InputBlocker.IsBlocked)
            {
                remaining = Mathf.Max(0f, remaining - Time.deltaTime);
                status.text = $"InputBlocker.IsBlocked = true ({remaining:0.0}s)";
            }
            else
            {
                status.text = "InputBlocker.IsBlocked = false";
            }
        }
    }
}
