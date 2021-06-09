﻿#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

namespace TSKT
{
    [RequireComponent(typeof(Toggle))]
    public class InputActionToggle : InputActionUI
    {
        [SerializeField]
        InputAction action = default!;

        [SerializeField]
        bool exclusive = true;

        Toggle? toggle;
        Toggle Toggle => toggle ? toggle! : (toggle = GetComponent<Toggle>());

        public override bool Modal => false;
        public override Navigation Navigation => new Navigation() { mode = Navigation.Mode.None };

        public override void Activate()
        {
            // nop
        }

        public override void Invoke(out bool exclusive)
        {
            if (action.triggered)
            {
                if (Toggle.isActiveAndEnabled && Toggle.interactable)
                {
                    Toggle.isOn = !Toggle.isOn;
                    exclusive = this.exclusive;
                    return;
                }
            }
            exclusive = false;
        }
    }
}
