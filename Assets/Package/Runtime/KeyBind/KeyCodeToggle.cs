using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
#nullable enable

namespace TSKT
{
    [RequireComponent(typeof(Toggle))]
    public class KeyCodeToggle : KeyBind
    {
        [SerializeField]
        UnityEngine.InputSystem.InputAction action = default!;

        [SerializeField]
        bool blockingSignals = false;
        public override bool BlockingSignals => blockingSignals;

        Toggle? toggle;
        Toggle Toggle => toggle ? toggle! : (toggle = GetComponent<Toggle>());

        public override void OnSelected()
        {
            // nop
        }

        public override void Execute(out bool exclusive)
        {
            if (action.triggered)
            {
                if (Toggle.isActiveAndEnabled && Toggle.interactable)
                {
                    Toggle.isOn = !Toggle.isOn;
                    exclusive = true;
                    return;
                }
            }
            exclusive = false;
        }
    }
}
