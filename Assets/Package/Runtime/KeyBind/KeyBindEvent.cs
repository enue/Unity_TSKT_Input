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
            if (onAxis == null)
            {
                return false;
            }
            if (onAxis.GetPersistentEventCount() == 0)
            {
                return false;
            }
            if (!axisPositions.TryGetValue(key, out var value))
            {
                return false;
            }
            if (value == 0f)
            {
                return false;
            }
            onAxis.Invoke();
            return true;
        }
        public override void OnSelected()
        {
            // nop
        }
    }
}
