using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    public class InputSetting
    {
        static public InputSetting Instance
        {
            get
            {
                return new InputSetting();
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
