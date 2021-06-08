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

        [SerializeField]
        bool selectable = true;
        public override bool Selectable => selectable;

        Toggle? toggle;
        Toggle Toggle => toggle ? toggle! : (toggle = GetComponent<Toggle>());

        public override bool Modal => false;

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
