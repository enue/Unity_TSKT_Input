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
        public override bool Selectable => true;

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
