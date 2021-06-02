using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
#nullable enable

namespace TSKT
{
    public class KeySignalBlocker : KeyBind
    {
        public override bool BlockingSignals => true;

        public override void Execute(out bool exclusive)
        {
            exclusive = true;
        }

        public override void OnSelected()
        {
            // nop
        }
    }
}
