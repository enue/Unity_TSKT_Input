using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    [RequireComponent(typeof(Button))]
    public class KeyBindButton : KeyBind
    {
        [SerializeField]
        string key = default;

        public override bool BlockingSignals => false;

        Button button;
        Button Button => button ? button : (button = GetComponent<Button>());

        public override bool OnKeyDown(string key)
        {
            if (key == this.key)
            {
                if (Button.isActiveAndEnabled && Button.interactable)
                {
                    Button.onClick.Invoke();
                    return true;
                }
            }
            return false;
        }

        public override bool OnKeyUp(string key)
        {
            return false;
        }

        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            return false;
        }
        public override void OnSelected()
        {
            // nop
        }
    }
}
