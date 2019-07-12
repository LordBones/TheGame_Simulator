using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Utils
{
    public static class RandomGen
    {
        private static RandomNumberGenerator rng = RandomNumberGenerator.Create();
        private static byte[] buff = new byte[2048];
        private static int buffIndex = 4500000;
        private static Random random = new Random(0);

        public static readonly int MaxPolygons = 250;
        public static long randomCall = 0;

        public static void ClearPseudoRandom() { random = new Random(0); random.NextBytes(buff); buffIndex = 0; randomCall = 0; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRandomNumber(int min, int max)
        {
            if (buffIndex >= buff.Length)
            {
                random.NextBytes(buff);
                //rng.GetBytes(buff);
                buffIndex = 0;
            }

            randomCall++;

            uint randValue = (uint)((buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex + 3]);
            buffIndex += 4;

            uint tmp = (uint)(max - min);

            return (int)min + (int)(randValue % tmp);
        }

        public static double GetRandomNumberDouble()
        {
            if (buffIndex >= buff.Length)
            {
                random.NextBytes(buff);
                //rng.GetBytes(buff);
                buffIndex = 0;
            }

            randomCall++;

            uint randValue = (uint)((buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex + 3]);
            buffIndex += 4;


            return (1.0 / uint.MaxValue) * randValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>

        public static int GetRandomNumberNoLinear_MinMoreOften(int maxValue, byte mutationRate)
        {
            double rnd = GetRandomNumberDouble();
            double power = 4 - 3 * (mutationRate / 255.0);

            return (int)(Math.Round(maxValue - Math.Pow(rnd, 1.0 / power) * maxValue));
        }


        public static int GetRandomNumberNoLinear_MinMoreOften(int value, int leftMinValue, int rightMaxValue, byte mutationRate)
        {
            int leftDiff = value - leftMinValue + 1;
            int rightDiff = rightMaxValue - value + 1;
            int randomMark = (RandomGen.GetRandomNumber(0, 2) == 0) ? 1 : -1;

            int mutationMax = (randomMark >= 0) ? rightDiff : leftDiff;
            int tmp = RandomGen.GetRandomNumberNoLinear_MinMoreOften(mutationMax, mutationRate) * randomMark;

            return value + tmp;
        }


        public static int GetRandomNumber(int min, int max, int ignore)
        {
            if (!(min <= ignore && ignore < max)) return GetRandomNumber(min, max);

            int tmp = GetRandomNumber(min, max - 1);
            return (tmp >= ignore) ? tmp + 1 : tmp;
        }


        public static int GetRandomChangeValue(int oldValue, int min, int max)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            int diff = GetRandomNumber(0, leftDelta + rightDelta);

            return oldValue + diff - leftDelta;
        }


        public static int GetRandomChangeValue(int oldValue, int min, int max, byte mutationRate)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            leftDelta = GetValueByMutationRate(leftDelta, 0, mutationRate);
            rightDelta = GetValueByMutationRate(rightDelta, 1, mutationRate);



            int diff = GetRandomNumber(0, leftDelta + rightDelta);

            return oldValue + diff - leftDelta;
        }


        public static int GetRandomChangeValueGuaranted(int oldValue, int min, int max, byte mutationRate)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            leftDelta = GetValueByMutationRate(leftDelta, 0, mutationRate);
            rightDelta = GetValueByMutationRate(rightDelta, 1, mutationRate);

            // nutne pokud je pouze jeden prvek a je soucasne v ignore
            if (leftDelta + rightDelta == 1)
                return oldValue;

            int diff = GetRandomNumber(0, leftDelta + rightDelta, leftDelta);

            return oldValue + diff - leftDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetValueByMutationRate(int value, int minValue, byte mutationRate)
        {
            return Math.Max(minValue, ((mutationRate + 1) * value) / (256));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// citatel , jmenovatel
        /// spocita deleni, s zaokrouhlovanim matematickym v pevne radove carce
        /// 
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public static int DivWithMathRouding(int numerator, int denominator)
        {
            int tmp = numerator / denominator;
            int modulo = numerator % denominator;

            tmp += modulo / ((denominator / 2) + (denominator % 2));
            /*if(((denominator /2)+(denominator&1) ) < modulo)
            {
                tmp++;
            }*/

            return tmp;
        }

        public static void swap<T>(ref T p1, ref T p2)
        {
            T tmp = p1;
            p1 = p2;
            p2 = tmp;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int fastAbs(int value)
        {
            return (value ^ (value >> 31)) - (value >> 31);

        }
    }
}
