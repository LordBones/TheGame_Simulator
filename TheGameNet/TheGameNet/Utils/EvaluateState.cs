using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Utils
{
    public struct EvaluateState<T, U>
        : IComparable<EvaluateState<T, U>>
        where U : struct, IComparable<U>
        where T : struct, IComparable<T>

    {
        public T Val;
        public U Val2;


        public EvaluateState(T val, U val2)
        {
            this.Val = val;
            this.Val2 = val2;
        }

        public int CompareTo(EvaluateState<T, U> other)
        {
            var tmp = this.Val.CompareTo(other.Val);
            if (tmp == 0) return this.Val2.CompareTo(other.Val2);
            else return tmp;
        }
    }
}