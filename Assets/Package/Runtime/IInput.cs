using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public interface IInput
    {
        bool GetKeyDown(KeyAssign.AppKey appKey);
        bool GetKeyUp(KeyAssign.AppKey appKey);
        bool GetKey(KeyAssign.AppKey appKey);
        float GetAxis(KeyAssign.AxisButton axisButton);
        float GetAxis(KeyAssign.AppAxis appAxis);
    }
}
