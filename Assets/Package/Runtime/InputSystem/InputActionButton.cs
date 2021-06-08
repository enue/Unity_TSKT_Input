#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

namespace TSKT
{
    [RequireComponent(typeof(Button))]
    public class InputActionButton : InputActionUI
    {
        [SerializeField]
        InputActionReference action = default!;

        [SerializeField]
        bool exclusive = true;

        Button? button;
        Button Button => button ? button! : (button = GetComponent<Button>());

        protected override bool Modal => false;
        protected override bool Navigated => true;

        protected override void Activate()
        {
            action.ToInputAction().Enable();
        }

        protected override void Invoke(out bool exclusive)
        {
            if (action.ToInputAction().triggered)
            {
                if (Button.isActiveAndEnabled && Button.interactable)
                {
                    Button.onClick.Invoke();
                    exclusive = this.exclusive;
                    return;
                }
            }
            exclusive = false;
        }
    }
}
