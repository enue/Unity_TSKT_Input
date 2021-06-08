#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace TSKT
{
    public class InputActionSelectable : InputActionUI
    {
        protected override bool Modal => false;
        protected override bool Navigated => true;

        protected override void Invoke(out bool exclusive)
        {
            exclusive = false;
        }

        protected override void Activate()
        {
            // nop
        }
    }
}
