using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

namespace TSKT
{
    [System.Obsolete]
    public readonly struct MergedKeyAssign
    {
        public readonly KeyAssign.AppKey[] keys;
        public readonly string[] distinctAppKeyNames;
        public readonly KeyAssign.AppAxis[] axes;
        public readonly KeyAssign.AxisButton[] axisButtons;

        public MergedKeyAssign(params KeyAssign[] keyAssigns)
        {
            using (UnityEngine.Pool.ListPool<KeyAssign.AppKey>.Get(out var pairs))
            {
                foreach (var it in keyAssigns)
                {
                    pairs.AddRange(it.AppKeys);
                }
                keys = pairs.ToArray();
                distinctAppKeyNames = pairs.Select(_ => _.name).ToArray();
            }

            using (UnityEngine.Pool.ListPool<KeyAssign.AppAxis>.Get(out var pairs))
            {
                foreach (var it in keyAssigns)
                {
                    pairs.AddRange(it.AppAxes);
                }
                axes = pairs.ToArray();
            }

            using (UnityEngine.Pool.ListPool<KeyAssign.AxisButton>.Get(out var pairs))
            {
                foreach (var it in keyAssigns)
                {
                    pairs.AddRange(it.AxisButtons);
                }
                axisButtons = pairs.ToArray();
            }
        }
    }
}