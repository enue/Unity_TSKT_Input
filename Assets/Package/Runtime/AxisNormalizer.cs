using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public class AxisNormalizer
    {
        struct Timer
        {
            float startedPressingTime;
            int previousFrame;
            int previousIndex;
            Vector2Int previousAxis;

            readonly float repeatDelay;
            readonly float repeatInterval;

            public Timer(float repeatDelay, float repeatInterval)
            {
                this.repeatDelay = repeatDelay;
                this.repeatInterval = repeatInterval;
                startedPressingTime = default;
                previousFrame = default;
                previousIndex = default;
                previousAxis = default;
            }
            public bool PushAxis(Vector2Int axis)
            {
                if (previousFrame == Time.frameCount)
                {
                    return false;
                }

                if (previousAxis != axis)
                {
                    previousAxis = axis;
                    previousFrame = 0;
                }

                if (previousFrame != Time.frameCount - 1)
                {
                    startedPressingTime = Time.realtimeSinceStartup;
                    previousFrame = Time.frameCount;
                    previousIndex = 0;
                    return true;
                }

                previousFrame = Time.frameCount;
                var elapsedTime = Time.realtimeSinceStartup - startedPressingTime;

                if (elapsedTime < repeatDelay)
                {
                    return false;
                }

                var index = Mathf.FloorToInt((elapsedTime - repeatDelay) / repeatInterval) + 1;

                var result = index != previousIndex;
                previousIndex = index;

                return result;
            }
        }

        public const float DefaultThreshold = 0.2f;
        Timer timer;
        public float HorizontalThreshold { get; set; } = DefaultThreshold;
        public float VerticalThreshold { get; set; } = DefaultThreshold;
        readonly float halfHorizontalOrVerticalAngleDegree;

        public AxisNormalizer(float horizontalOrVerticalAngleDegree = 45f, float repeatDelay = 0.5f, float repeatInterval = 0.1f)
        {
            halfHorizontalOrVerticalAngleDegree = horizontalOrVerticalAngleDegree / 2f;
            timer = new Timer(repeatDelay, repeatInterval);
        }

        bool Normalize(out Vector2Int result, Vector2 axis, bool supportEightDirections)
        {
            if (axis.x * axis.x / (HorizontalThreshold * HorizontalThreshold)
                + axis.y * axis.y / (VerticalThreshold * VerticalThreshold) < 1f)
            {
                result = default;
                return false;
            }
            // 絶対値でとっているのでangleは0-90までの値になる。
            var angle = Mathf.Atan2(Mathf.Abs(axis.y), Mathf.Abs(axis.x))
                * Mathf.Rad2Deg;

            var horizontalPulse = axis.x > 0f ? 1 : -1;
            var verticalPulse = axis.y > 0f ? 1 : -1;

            if (angle < halfHorizontalOrVerticalAngleDegree)
            {
                result = new Vector2Int(horizontalPulse, 0);
                return true;
            }
            else if (angle > 90f - halfHorizontalOrVerticalAngleDegree)
            {
                result = new Vector2Int(0, verticalPulse);
                return true;
            }
            else if (supportEightDirections)
            {
                result = new Vector2Int(horizontalPulse, verticalPulse);
                return true;
            }
            else
            {
                // 斜め入力は破棄
                result = default;
                return true;
            }
        }

        public Vector2Int GetPulse(out Vector2Int normalized, Vector2 axis, bool supportEightDirections)
        {
            if (!Normalize(out normalized, axis, supportEightDirections))
            {
                return Vector2Int.zero;
            }

            if (timer.PushAxis(normalized))
            {
                return normalized;
            }

            return Vector2Int.zero;
        }
    }
}
