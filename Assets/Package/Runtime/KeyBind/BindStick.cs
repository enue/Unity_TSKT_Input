using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
#nullable enable

namespace TSKT
{
    public class BindStick : KeyBind
    {
        [SerializeField]
        string horizontalAxis = "";

        [SerializeField]
        string verticalAxis = "";

        [SerializeField]
        bool exclusive = true;

        [SerializeField]
        UnityEngine.Events.UnityEvent<Vector2> onStickTilt = new UnityEngine.Events.UnityEvent<Vector2>();

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
            axisPositions.TryGetValue(horizontalAxis!, out var horizontalPosition);
            axisPositions.TryGetValue(verticalAxis!, out var verticalPosition);
            if (horizontalPosition != 0f || verticalPosition != 0f)
            {
                onStickTilt!.Invoke(new Vector2(horizontalPosition, verticalPosition));
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
