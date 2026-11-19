#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace TSKT.Samples
{
    /// <summary>
    /// ボタンが押された回数を表示するだけのサンプル用コンポーネント。
    /// </summary>
    public class SampleCounter : MonoBehaviour
    {
        [SerializeField]
        Text label = default!;

        [SerializeField]
        string format = "Count: {0}";

        int count;

        void Start()
        {
            Refresh();
        }

        public void Increment()
        {
            ++count;
            Refresh();
        }

        void Refresh()
        {
            label.text = string.Format(format, count);
        }
    }
}
