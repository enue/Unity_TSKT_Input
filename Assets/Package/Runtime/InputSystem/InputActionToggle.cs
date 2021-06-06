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

        protected override bool Modal => false;
        protected override void Activate()
        {
            // nop
        }

        protected override void Invoke(out bool exclusive)
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