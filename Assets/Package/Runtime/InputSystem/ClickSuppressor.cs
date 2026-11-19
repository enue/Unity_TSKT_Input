#nullable enable
using UnityEngine;

namespace TSKT
{
    /// <summary>
    /// 互換のために残している。中身はInputBlockerを呼ぶだけ。
    /// </summary>
    [System.Obsolete("Use InputBlocker.Block() instead.")]
    public class ClickSuppressor : MonoBehaviour
    {
        public readonly struct Handler : System.IDisposable
        {
            readonly System.IDisposable? inner;
            readonly public int Id { get; }

            internal Handler(System.IDisposable inner, int id)
            {
                this.inner = inner;
                Id = id;
            }

            public void Dispose()
            {
                // InputBlockerのハンドルは複数回Disposeしても安全。default(Handler)では何もしない
                inner?.Dispose();
            }
        }

        public static ClickSuppressor? Instance { get; private set; }

        static int nextHandlerId;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public Handler Enable()
        {
            var id = nextHandlerId;
            ++nextHandlerId;
            return new Handler(InputBlocker.Block(), id);
        }
    }
}
