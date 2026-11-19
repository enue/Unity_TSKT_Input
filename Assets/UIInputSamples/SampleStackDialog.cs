#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace TSKT.Samples
{
    /// <summary>
    /// 自分を複製して重ねていくダイアログのサンプル。
    /// ルートにUIInputModalScopeが付いていて、複製されるたびにスタックに積まれる。
    /// </summary>
    public class SampleStackDialog : MonoBehaviour
    {
        [SerializeField]
        Text title = default!;

        [SerializeField]
        RectTransform panel = default!;

        [SerializeField]
        Vector2 offsetPerDepth = new(48f, -36f);

        [SerializeField]
        int depth;

        /// <summary>
        /// 自分の複製をひとつ上に開く。非アクティブなテンプレートから呼んでも動く。
        /// </summary>
        public void OpenAnother()
        {
            var clone = Instantiate(this, transform.parent);
            clone.depth = depth + 1;
            clone.name = "Dialog" + clone.depth;
            clone.title.text = $"Dialog {clone.depth} (UIInputModalScope)";
            clone.panel.anchoredPosition = offsetPerDepth * (clone.depth - 1);
            clone.gameObject.SetActive(true);
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
