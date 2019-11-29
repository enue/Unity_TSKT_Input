using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    public class KeyBindEvent : KeyBind
    {
        [SerializeField]
        string key = default;

        public override bool BlockingSignals => false;

        [SerializeField]
        UnityEngine.Events.UnityEvent onKeyDown = default;

        [SerializeField]
        UnityEngine.Events.UnityEvent onKeyUp = default;

        [SerializeField]
        UnityEngine.Events.UnityEvent onAxis = default;

        public override bool OnKeyDown(string key)
        {
            if (this.key != key)
            {
                return false;
            }
            if (onKeyDown != null && onKeyDown.GetPersistentEventCount() > 0)
            {
                onKeyDown.Invoke();
                return true;
            }
            return false;
        }

        public override bool OnKeyUp(string key)
        {
            if (this.key != key)
            {
                return false;
            }
            if (onKeyUp != null && onKeyUp.GetPersistentEventCount() > 0)
            {
                onKeyUp.Invoke();
                return true;
            }
            return false;
        }

        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            foreach (var pair in axisPositions)
            {
                if (pair.Value == 0f)
                {
                    continue;
                }

                if (pair.Key != key)
                {
                    continue;
                }
                if (onAxis != null && onAxis.GetPersistentEventCount() > 0)
                {
                    onAxis.Invoke();
                    return true;
                }
            }
            return false;
        }
        public override void OnSelected()
        {
            // nop
        }
    }
}
