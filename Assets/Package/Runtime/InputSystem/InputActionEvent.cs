#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    public class InputActionEvent : InputActionUI
    {
        [SerializeField]
        InputActionReference action = default!;

        [SerializeField]
        bool exclusive = true;

        [SerializeField]
        UnityEngine.Events.UnityEvent<InputAction> onTriggered = default!;
        public UnityEngine.Events.UnityEvent<InputAction> OnTriggered => onTriggered;

        public override bool Modal => false;
        public override Navigation Navigation => new() { mode = Navigation.Mode.None };

        public override void Activate()
        {
            action.ToInputAction().Enable();
        }

        public override void Invoke(out bool exclusive)
        {
            if (action.ToInputAction().triggered)
            {
                onTriggered.Invoke(action);
                exclusive = this.exclusive;
                return;
            }
            exclusive = false;
        }
    }
}
