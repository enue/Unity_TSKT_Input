using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    [RequireComponent(typeof(Button))]
    public class CancelKeyButton : KeyBind
    {
        Button button;
        Button Button => button ? button : (button = GetComponent<Button>());

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

        public override bool OnKeyUp(string key)
        {
            return false;
        }
        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            return false;
        }
        public override bool OnKey(string key)
        {
            return false;
        }

        public override void OnSelected()
        {
            // nop
        }
    }
}
