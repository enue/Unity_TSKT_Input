#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TSKT
{
    [RequireComponent(typeof(Selectable))]
    public class InputActionSelectable : InputActionUI
    {
        public override bool Modal => false;
        [SerializeField]
        Navigation.Mode navigationMode = Navigation.Mode.Automatic;
        public override Navigation.Mode NavigationMode => navigationMode;

        public override void Invoke(out bool exclusive)
        {
            exclusive = false;
        }

        public override void Activate()
        {
            // nop
        }
    }
}
