#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace TSKT
{
    [System.Obsolete("Use InputBlocker.Block() instead.")]
    public class ClickSuppressor : MonoBehaviour
    {
        public readonly struct Handler : System.IDisposable
        {
            readonly ClickSuppressor? owner;
            readonly public int Id { get; }

            public Handler(ClickSuppressor filter)
            {
                owner = filter;
                Id = filter.nextHandlerId;
            }
            public void Dispose()
            {
                // default(Handler)や、破棄済みのownerに対しては何もしない
                if (!owner)
                {
                    return;
                }
                // Handlerはstructなのでコピーから複数回Disposeされうる。解除済みなら何もしない
                if (!owner!.handlerIds.Remove(Id))
                {
                    return;
                }
                if (owner.handlerIds.Count == 0)
                {
                    owner.Disable();
                }
            }
        }

        public static ClickSuppressor? Instance { get; private set; }

        [SerializeField]
        InputSystemUIInputModule module = default!;

        int nextHandlerId;
        readonly HashSet<int> handlerIds = new();

        InputActionReference? submit;
        InputActionReference? cancel;
        InputActionReference? leftClick;
        InputActionReference? rightClick;
        InputActionReference? middleClick;

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
            // 抑制を始めるたびに現在の設定を取り直す。抑制中の再取得はnullを拾うのでしない
            if (handlerIds.Count == 0)
            {
                cancel = module.cancel;
                submit = module.submit;
                leftClick = module.leftClick;
                rightClick = module.rightClick;
                middleClick = module.middleClick;

                module.cancel = null;
                module.submit = null;
                module.leftClick = null;
                module.rightClick = null;
                module.middleClick = null;
            }

            var token = new Handler(this);
            handlerIds.Add(token.Id);
            ++nextHandlerId;

            return token;
        }

        void Disable()
        {
            if (!module)
            {
                return;
            }
            module.cancel = cancel;
            module.submit = submit;
            module.leftClick = leftClick;
            module.rightClick = rightClick;
            module.middleClick = middleClick;
        }
    }
}
