using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    [RequireComponent(typeof(Button))]
    public class CancelKeyButton : KeyBind
    {
        Button? button;
        Button Button => button ? button! : (button = GetComponent<Button>());

        [SerializeField]
        bool blockingSignals = false;
        public override bool BlockingSignals => blockingSignals;

        public override bool OnKeyDown(List<string> keys)
        {
            if (keys.Contains(InputSetting.Instance.Cancel))
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
