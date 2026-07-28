using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    public abstract class KeyBind : MonoBehaviour
    {
        [SerializeField]
        int orderInObject = 0;

        public abstract bool BlockingSignals { get; }
        public abstract bool OnKeyDown(List<string> keys);
        public abstract bool OnKeyUp(List<string> keys);
        public abstract bool OnKey(List<string> keys);
        public abstract bool OnAxis(Dictionary<string, float> axisPositions);
        public abstract void OnSelected();

        public static void SendSignals(
            List<string> downKeys,
            List<string> upKeys,
            Dictionary<string, float> axisPositions,
            List<string> onKeys)
        {
        }
    }
}
