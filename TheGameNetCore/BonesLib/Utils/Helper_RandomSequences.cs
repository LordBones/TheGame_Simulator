using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.Utils
{
    internal static class Helper_RandomSequences
    {
        public static void SequenceEqualsOne(RandomGen rnd, Span<double> sequence, double maxSum = 1.0)
        {
            double tmp = maxSum;
            for (int i = 0; i < sequence.Length - 1; i++)
            {
                var oneVal = rnd.GetRandomNumberDouble() * tmp* (2/(double)(sequence.Length-i));
                sequence[i] = oneVal;
                tmp -= oneVal;
            }

            sequence[sequence.Length - 1] = tmp;
        }
    }
}
