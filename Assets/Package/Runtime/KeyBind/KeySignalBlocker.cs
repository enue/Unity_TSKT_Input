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

        public override bool OnKeyDown(List<string> keys)
        {
            return true;
        }

        public override bool OnKeyUp(List<string> buttons)
        {
            return true;
        }

        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            return true;
        }

        public override bool OnKey(List<string> keys)
        {
            return true;
        }

        public override void OnSelected()
        {
            // nop
        }
    }
}
