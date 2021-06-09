#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

        public override bool Modal => false;
        public override Navigation Navigation => new Navigation() { mode = Navigation.Mode.None };

        public override void Activate()
        {
            action.ToInputAction().Enable();
        }

        public override void Invoke(out bool exclusive)
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
