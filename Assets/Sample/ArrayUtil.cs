using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public static class ArrayUtil
    {
        static public T MaxBy<T, S, E>(S list, System.Func<T, E> func)
            where S : IEnumerable<T>
            where E : System.IComparable<E>
        {
            var result = default(T);
            var max = default(E);
            var first = true;
            foreach (var item in list)
            {
                var v = func(item);
                if (first)
                {
                    first = false;
                    max = v;
                    result = item;
                }
                else if (max.CompareTo(v) < 0)
                {
                    max = v;
                    result = item;
                }
            }
            return result;
        }
    }
}
