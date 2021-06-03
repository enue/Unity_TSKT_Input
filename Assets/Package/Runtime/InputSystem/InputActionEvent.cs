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
        UnityEngine.InputSystem.InputActionReference action = default!;

        [SerializeField]
        bool exclusive = true;

        [SerializeField]
        UnityEngine.Events.UnityEvent<InputAction> onTriggered = default!;

        protected override bool Modal => false;
        protected override void Activate()
        {
            action.ToInputAction().Enable();
        }

        protected override void Invoke(out bool exclusive)
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
