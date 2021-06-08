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

        public override bool Modal => false;
        public override bool Selectable => false;

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
