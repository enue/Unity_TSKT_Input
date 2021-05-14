using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    [RequireComponent(typeof(Toggle))]
    public class KeyCodeToggle : KeyBind
    {
        [SerializeField]
        KeyCode key = default;

        public override bool BlockingSignals => false;

        Toggle toggle;
        Toggle Toggle => toggle ? toggle : (toggle = GetComponent<Toggle>());

        public override bool OnKeyDown(List<string> keys)
        {
            if (Input.GetKeyDown(key))
            {
                if (Toggle.isActiveAndEnabled && Toggle.interactable)
                {
                    Toggle.isOn = !Toggle.isOn;
                    return true;
                }
            }
            return false;
        }

        public override bool OnKeyUp(List<string> keys)
        {
            return false;
        }

        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            return false;
        }

        public override bool OnKey(List<string> keys)
        {
            return false;
        }

        public override void OnSelected()
        {
            // nop
        }
    }
}
