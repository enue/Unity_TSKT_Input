﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    public class KeyBindEvent : KeyBind
    {
        [System.Serializable]
        public class FloatEvent : UnityEngine.Events.UnityEvent<float> { }

        [SerializeField]
        string key = default!;

        [SerializeField]
        bool exclusive = true;

        public override bool BlockingSignals => false;

        [SerializeField]
        UnityEngine.Events.UnityEvent onKeyDown = default!;

        [SerializeField]
        UnityEngine.Events.UnityEvent onKeyUp = default!;

        [SerializeField]
        FloatEvent onKey = default!;

        [SerializeField]
        FloatEvent onAxis = default!;

        int previousFrame;
        float downTime;

        public override bool OnKeyDown(List<string> keys)
        {
            if (!keys.Contains(key))
            {
                return false;
            }

            Refresh();

            if (onKeyDown != null && onKeyDown.GetPersistentEventCount() > 0)
            {
                onKeyDown.Invoke();
                return exclusive;
            }
            return false;
        }

        public override bool OnKeyUp(List<string> keys)
        {
            if (!keys.Contains(key))
            {
                return false;
            }
            if (onKeyUp != null && onKeyUp.GetPersistentEventCount() > 0)
            {
                onKeyUp.Invoke();
                return exclusive;
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
            return exclusive;
        }

        public override bool OnKey(List<string> keys)
        {
            if (!keys.Contains(key))
            {
                return false;
            }

            Refresh();

            if (onKey != null && onKey.GetPersistentEventCount() > 0)
            {
                onKey.Invoke(Time.realtimeSinceStartup - downTime);
                return exclusive;
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
