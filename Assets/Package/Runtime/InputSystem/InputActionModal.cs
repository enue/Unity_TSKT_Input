#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace TSKT
{
    public class InputActionModal : InputActionUI
    {
        public override bool Modal => true;
        public override bool Selectable => false;

        public override void Invoke(out bool exclusive)
        {
            exclusive = true;
        }

        public override void Activate()
        {
            // nop
        }
    }
}
