using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    public class KeyObserver : MonoBehaviour
    {
        [SerializeField]
        KeyAssign? defaultKeyAssign = default;

        [SerializeField]
        KeyAssign?[] subKeyAssigns = default!;

        public void SetKeyAssign(params KeyAssign[] keyAssigns)
        {
        }

        public bool GetButtonDown(string button)
        {
            return false;
        }

        public bool GetButton(string button)
        {
            return false;
        }
    }
}
