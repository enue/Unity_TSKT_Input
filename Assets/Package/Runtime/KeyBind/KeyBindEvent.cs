using UnityEngine;
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    public class KeyBindEvent : KeyBind
    {
        [SerializeField]
        UnityEngine.InputSystem.InputActionReference action = default!;

        [SerializeField]
        bool exclusive = true;

        [SerializeField]
        bool blockingSignals = false;
        public override bool BlockingSignals => blockingSignals;

        [SerializeField]
        UnityEngine.Events.UnityEvent<UnityEngine.InputSystem.InputAction> onPerformed = default!;

        public override void OnSelected()
        {
            action.ToInputAction().Enable();
        }

        public override void Execute(out bool exclusive)
        {
            if (action.ToInputAction().triggered)
            {
                onPerformed.Invoke(action);
                exclusive = this.exclusive;
            }
            exclusive = false;
        }
    }
}
