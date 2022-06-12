using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    public class InputSetting : ScriptableObject
    {
        static InputSetting? instance;
        static public InputSetting Instance
        {
            get
            {
                return instance ? instance! : (instance = Resources.Load<InputSetting>("InputSetting"));
            }
        }

        [SerializeField]
        string submit = "Submit";
        public string Submit => submit;

        [SerializeField]
        string cancel = "Cancel";
        public string Cancel => cancel;
    }
}