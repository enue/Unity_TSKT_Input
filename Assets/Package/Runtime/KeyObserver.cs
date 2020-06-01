using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace TSKT
{
    public class KeyObserver : MonoBehaviour
    {
        public static KeyObserver Instance { get; private set; }

        readonly List<string> upKeys = new List<string>();
        readonly List<string> downKeys = new List<string>();
        readonly Dictionary<string, float> axisPositions = new Dictionary<string, float>();

        public IInput AppInput { get; set; } = new DefaultInput();

        MergedKeyAssign keyAssign;
        MergedKeyAssign KeyAssign
        {
            get
            {
                if (keyAssign.keys == null)
                {
                    if (defaultKeyAssign)
                    {
                        if (subKeyAssigns == null || subKeyAssigns.Length == 0)
                        {
                            SetKeyAssign(defaultKeyAssign);
                        }
                        else
                        {
                            SetKeyAssign(new[] { defaultKeyAssign }.Concat(subKeyAssigns).ToArray());
                        }
                    }
                    else
                    {
                        if (subKeyAssigns == null || subKeyAssigns.Length == 0)
                        {
                            // nop
                        }
                        else
                        {
                            SetKeyAssign(subKeyAssigns);
                        }
                    }
                }
                return keyAssign;
            }
        }

        [SerializeField]
        KeyAssign defaultKeyAssign = default;

        [SerializeField]
        KeyAssign[] subKeyAssigns = default;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetKeyAssign(params KeyAssign[] keyAssigns)
        {
            keyAssign = new MergedKeyAssign(keyAssigns);
        }

        public bool GetButtonDown(string button)
        {
            if (KeyAssign.keys == null)
            {
                return false;
            }
            foreach (var it in KeyAssign.keys)
            {
                if (it.name == button)
                {
                    if (AppInput.GetKeyDown(it))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool GetButton(string button)
        {
            if (KeyAssign.keys == null)
            {
                return false;
            }
            foreach (var it in KeyAssign.keys)
            {
                if (it.name == button)
                {
                    if (AppInput.GetKey(it))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void Update()
        {
            if (KeyAssign.keys == null)
            {
                return;
            }
            downKeys.Clear();
            upKeys.Clear();
            axisPositions.Clear();

            foreach (var it in KeyAssign.keys)
            {
                if (AppInput.GetKeyDown(it))
                {
                    downKeys.Add(it.name);
                }
                if (AppInput.GetKeyUp(it))
                {
                    upKeys.Add(it.name);
                }
                if (AppInput.GetKey(it))
                {
                    axisPositions[it.name] = 1f;
                }
            }

            foreach (var it in KeyAssign.axisButtons)
            {
                var axisPosition = AppInput.GetAxis(it);
                if (axisPosition == 0f)
                {
                    continue;
                }
                if (axisPositions.TryGetValue(it.appKey, out var current))
                {
                    if (Mathf.Abs(current) < Mathf.Abs(axisPosition))
                    {
                        axisPositions[it.appKey] = axisPosition;
                    }
                }
                else
                {
                    axisPositions[it.appKey] = axisPosition;
                }
            }
            foreach (var it in KeyAssign.axes)
            {
                var axisPosition = AppInput.GetAxis(it);
                if (axisPosition == 0f)
                {
                    continue;
                }
                if (axisPositions.TryGetValue(it.name, out var current))
                {
                    if (Mathf.Abs(current) < Mathf.Abs(axisPosition))
                    {
                        axisPositions[it.name] = axisPosition;
                    }
                }
                else
                {
                    axisPositions[it.name] = axisPosition;
                }
            }

            KeyBind.SendSignals(downKeys, upKeys, axisPositions);
        }
    }
}
