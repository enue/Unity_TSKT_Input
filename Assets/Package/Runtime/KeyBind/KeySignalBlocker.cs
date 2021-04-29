﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace TSKT
{
    public class KeySignalBlocker : KeyBind
    {
        public override bool BlockingSignals => true;

        public override bool OnKeyDown(List<string> keys)
        {
            return true;
        }

        public override bool OnKeyUp(string button)
        {
            return true;
        }

        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            return true;
        }

        public override bool OnKey(string key)
        {
            return true;
        }

        public override void OnSelected()
        {
            // nop
        }
    }
}
