using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    [RequireComponent(typeof(Button))]
    public class SubmitKeyButton : KeyBind
    {
        public override bool BlockingSignals => true;

        Button button;
        Button Button => button ?? (button = GetComponent<Button>());

        public override bool OnKeyDown(string key)
        {
            if (key == InputSetting.Instance.Submit)
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
