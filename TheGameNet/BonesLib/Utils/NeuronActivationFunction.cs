using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BonesLib.Utils
{
    public static class NeuronActivationFunction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ActivationFunction_ReLu(float sum)
        {
            return (sum < 0) ? 0.01f * sum : sum;

        }
    }
}
