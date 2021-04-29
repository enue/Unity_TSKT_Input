using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace TSKT
{
    public class BindStick : KeyBind
    {
        [SerializeField]
        string horizontalAxis = default;

        [SerializeField]
        string verticalAxis = default;

        [SerializeField]
        bool exclusive = true;

        [SerializeField]
        UnityEngine.Events.UnityEvent<Vector2> onStickTilt = default;

        public override bool BlockingSignals => false;

        public override bool OnKeyDown(List<string> keys)
        {
            return false;
        }

        public override bool OnKeyUp(List<string> keys)
        {
            return false;
        }

        public override bool OnAxis(Dictionary<string, float> axisPositions)
        {
            axisPositions.TryGetValue(horizontalAxis, out var horizontalPosition);
            axisPositions.TryGetValue(verticalAxis, out var verticalPosition);
            if (horizontalPosition != 0f || verticalPosition != 0f)
            {
                onStickTilt.Invoke(new Vector2(horizontalPosition, verticalPosition));
                return exclusive;
            }

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
