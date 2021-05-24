using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    [CreateAssetMenu(fileName = "KeyAssign", menuName = "TSKT/Key assign", order = 1023)]
    public class KeyAssign : ScriptableObject
    {
        [System.Serializable]
        public struct AppKey
        {
            public string name;
            public KeyCode keyCode;
        }

        [System.Serializable]
        public struct AppAxis
        {
            public string name;
            public string axisName;
        }

        [System.Serializable]
        public struct AxisButton
        {
            public string appKey;
            public KeyCode keyCode;
            public KeyCode negativeKeyCode;
        }

        [SerializeField]
        AppKey[] appKeys = new AppKey[0];
        public AppKey[] AppKeys => appKeys;

        [SerializeField]
        AppAxis[] appAxes = new AppAxis[0];
        public AppAxis[] AppAxes => appAxes;

        [SerializeField]
        AxisButton[] axisButtons = new AxisButton[0];
        public AxisButton[] AxisButtons => axisButtons;

    }
}
