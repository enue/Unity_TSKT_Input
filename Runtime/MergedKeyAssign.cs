using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public readonly struct MergedKeyAssign
    {
        public readonly KeyAssign.AppKey[] keys;
        public readonly string[] distinctAppKeyNames;
        public readonly KeyAssign.AppAxis[] axes;
        public readonly KeyAssign.AxisButton[] axisButtons;

        public MergedKeyAssign(params KeyAssign[] keyAssigns)
        {
            {
                var pairs = new List<KeyAssign.AppKey>();
                foreach (var it in keyAssigns)
                {
                    pairs.AddRange(it.AppKeys);
                }
                keys = pairs.ToArray();
                distinctAppKeyNames = pairs.Select(_ => _.name).ToArray();
            }

            {
                var pairs = new List<KeyAssign.AppAxis>();
                foreach (var it in keyAssigns)
                {
                    pairs.AddRange(it.AppAxes);
                }
                axes = pairs.ToArray();
            }

            {
                var pairs = new List<KeyAssign.AxisButton>();
                foreach (var it in keyAssigns)
                {
                    pairs.AddRange(it.AxisButtons);
                }
                axisButtons = pairs.ToArray();
            }
        }
    }
}