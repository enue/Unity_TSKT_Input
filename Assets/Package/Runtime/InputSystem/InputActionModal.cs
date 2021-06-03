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
        protected override bool Modal => true;

        protected override void Invoke(out bool exclusive)
        {
            exclusive = true;
        }

        protected override void Activate()
        {
            // nop
        }
    }
}
