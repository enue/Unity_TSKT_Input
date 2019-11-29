using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public class InputSetting : ScriptableObject
    {
        static InputSetting instance;
        static public InputSetting Instance
        {
            get
            {
                return instance ?? (instance = Resources.Load<InputSetting>("InputSetting"));
            }
        }

        [SerializeField]
        string submit = "Submit";
        public string Submit => submit;

        [SerializeField]
        string cancel = "Cancel";
        public string Cancel => cancel;


#if UNITY_EDITOR
        [UnityEditor.MenuItem("TSKT/Create Input Setting")]
        static void CreateScriptableObject()
        {
            var obj = CreateInstance<InputSetting>();
            UnityEditor.AssetDatabase.CreateAsset(obj, "Assets/Resources/InputSetting.asset");
            UnityEditor.EditorUtility.SetDirty(obj);
        }
#endif
    }
}