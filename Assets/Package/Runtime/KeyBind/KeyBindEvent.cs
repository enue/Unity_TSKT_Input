using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    public class KeyBindEvent : KeyBind
    {
        [System.Serializable]
        public class FloatEvent : UnityEngine.Events.UnityEvent<float> { }

        [SerializeField]
        string key = default;

        public override bool BlockingSignals => false;

        [SerializeField]
        UnityEngine.Events.UnityEvent onKeyDown = default;

        [SerializeField]
        UnityEngine.Events.UnityEvent onKeyUp = default;

        [SerializeField]
        FloatEvent onKey = default;

        [SerializeField]
        FloatEvent onAxis = default;

        int previousFrame;
        float downTime;

        public override bool OnKeyDown(string key)
        {
            if (this.key != key)
            {
                return false;
            }

            Refresh();

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
            axisPositions.TryGetValue(key, out var value);
            if (value == 0f)
            {
                return false;
            }

            Refresh();

            if (onAxis == null)
            {
                return false;
            }
            if (onAxis.GetPersistentEventCount() == 0)
            {
                return false;
            }

            onAxis?.Invoke(value);
            return true;
        }

        public override bool OnKey(string key)
        {
            if (this.key != key)
            {
                return false;
            }

            Refresh();

            if (onKey != null && onKey.GetPersistentEventCount() > 0)
            {
                onKey.Invoke(Time.realtimeSinceStartup - downTime);
                return true;
            }
            return false;
        }

        public override void OnSelected()
        {
            // nop
        }

        void Refresh()
        {
            if (previousFrame < Time.frameCount - 1)
            {
                downTime = Time.realtimeSinceStartup;
            }
            previousFrame = Time.frameCount;
        }
    }
}
