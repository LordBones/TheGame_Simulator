using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BonesLib.Utils
{
    public static class NeuronActivationFunction
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static float ActivationFunction_ReLu(float sum)
        //{
        //    return (sum < 0) ? 0.01f * sum : sum;

        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ActivationFunction_ReLu(float sum)
        {
            return (sum < 0) ? 0.01f * sum :
                (sum > 1) ? 1.0f+ (sum-1.0f) * 0.1f : sum;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ActivationFunction_tanh(float sum)
        {
            float eSum = FastExp(sum);
            float eSumNeg = FastExp(-sum);

            return ((eSum - eSumNeg) / (eSum + eSumNeg));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FastExp(float val)
        {
            float x = 1.0f + val / 1024;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x;
            return x;
        }
    }
}
