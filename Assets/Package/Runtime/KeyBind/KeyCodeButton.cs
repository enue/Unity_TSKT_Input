using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
#nullable enable

namespace TSKT
{
    [RequireComponent(typeof(Button))]
    public class KeyCodeButton : KeyBind
    {
        [SerializeField]
        UnityEngine.InputSystem.InputAction action = default!;

        [SerializeField]
        bool blockingSignals = false;
        public override bool BlockingSignals => blockingSignals;

        Button? button;
        Button Button => button ? button! : (button = GetComponent<Button>());

        public override void OnSelected()
        {
            // nop
        }

        public override void Execute(out bool exclusive)
        {
            if (action.triggered)
            {
                if (Button.isActiveAndEnabled && Button.interactable)
                {
                    Button.onClick.Invoke();
                    exclusive = true;
                    return;
                }
            }
            exclusive = false;
        }
    }
}
