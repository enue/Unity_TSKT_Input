using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
#nullable enable

namespace TSKT
{
    [RequireComponent(typeof(Button))]
    public class SubmitKeyButton : KeyBind
    {
        [SerializeField]
        bool blockingSignals = true;
        public override bool BlockingSignals => blockingSignals;

        Button? button;
        Button Button => button ? button! : (button = GetComponent<Button>());

        public override bool OnKeyDown(List<string> keys)
        {
            if (keys.Contains(InputSetting.Instance.Submit))
            {
                if (Button.isActiveAndEnabled && Button.interactable)
                {
                    Button.onClick.Invoke();
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
