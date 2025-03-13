#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace TSKT
{
    public class ClickSuppressor : MonoBehaviour
    {
        public readonly struct Handler : System.IDisposable
        {
            readonly ClickSuppressor owner;
            readonly public int Id { get; }

            public Handler(ClickSuppressor filter)
            {
                owner = filter;
                Id = filter.nextHandlerId;
            }
            public void Dispose()
            {
                owner.handlerIds.Remove(Id);
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

        InputActionReference submit;
        InputActionReference cancel;
        InputActionReference leftClick;
        InputActionReference rightClick;
        InputActionReference middleClick;

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
            cancel ??= module.cancel;
            submit ??= module.submit;
            leftClick ??= module.leftClick;
            rightClick ??= module.rightClick;
            middleClick ??= module.middleClick;

            module.cancel = null;
            module.submit = null;
            module.leftClick = null;
            module.rightClick = null;
            module.middleClick = null;

            var token = new Handler(this);
            handlerIds.Add(token.Id);
            ++nextHandlerId;

            return token;
        }

        void Disable()
        {
            module.cancel = cancel;
            module.submit = submit;
            module.submit = submit;
            module.leftClick = leftClick;
            module.rightClick = rightClick;
            module.middleClick = middleClick;
        }
    }
}
