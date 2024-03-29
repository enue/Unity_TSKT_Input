﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
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
        AppKey[] appKeys = default!;
        public AppKey[] AppKeys => appKeys;

        [SerializeField]
        AppAxis[] appAxes = default!;
        public AppAxis[] AppAxes => appAxes;

        [SerializeField]
        AxisButton[] axisButtons = default!;
        public AxisButton[] AxisButtons => axisButtons;

    }
}
