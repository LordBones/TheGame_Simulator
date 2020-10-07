using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BonesLib.Utils
{
    public class RandomGen
    {
        //private RandomNumberGenerator rng = RandomNumberGenerator.Create();
        private byte[] buff = new byte[16];
        private int buffIndex = 4500000;
        private Random random;

        public long randomCall = 0;
        public static RandomGen Default { get; }
        

        public void ClearPseudoRandom() { random = new Random(0); random.NextBytes(buff); buffIndex = 0; randomCall = 0; }

        static RandomGen()
        {
            Default = new RandomGen(0);
        }

        public RandomGen(int seed)
        {
            random = new Random(seed);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRandomNumber_Buffered(int min, int max)
        {
            if (buffIndex+4 >= buff.Length)
            {
                random.NextBytes(buff);
                //rng.GetBytes(buff);
                buffIndex = 0;
            }

            if (min >= max) throw new Exception("Bad params  min >= max");

            randomCall++;

            uint randValue = (uint)((buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex + 3]);
            buffIndex += 4;

            uint tmp = (uint)(max - min);

            return (int)min + (int)(randValue % tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRandomNumber(int min, int max)
        {
            return random.Next(min ,max);
        }

        public double GetRandomNumberDouble_Buffered()
        {
            if (buffIndex+4 >= buff.Length)
            {
                random.NextBytes(buff);
                //rng.GetBytes(buff);
                buffIndex = 0;
            }

            randomCall++;

            uint randValue = (uint)((buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex + 3]);
            buffIndex += 4;


            return ((1.0 * randValue) / uint.MaxValue) ;
        }

        public double GetRandomNumberDouble()
        {
            return random.NextDouble();
        }

        readonly double _sqrt2 = Math.Sqrt(2.0);

        /// <summary>
        /// generate values 0.5..0 gaus probability
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private double PhiGausian(double x)
        {
            // constants
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;
           

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x) / _sqrt2;

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * FastExp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }

        public double GetRandomNumberDoubleGausisan()
        {
            double x = GetRandomNumberDouble()*6.0-3.0;
            double g = PhiGausian(x);
            return g;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>

        public int GetRandomNumberNoLinear_MinMoreOften(int maxValue, byte mutationRate)
        {
            double rnd = GetRandomNumberDouble();
            double power = 4 - 3 * (mutationRate / 255.0);

            return (int)(Math.Round(maxValue - Math.Pow(rnd, 1.0 / power) * maxValue));
        }


        public int GetRandomNumberNoLinear_MinMoreOften(int value, int leftMinValue, int rightMaxValue, byte mutationRate)
        {
            int leftDiff = value - leftMinValue + 1;
            int rightDiff = rightMaxValue - value + 1;
            int randomMark = (GetRandomNumber(0, 2) == 0) ? 1 : -1;

            int mutationMax = (randomMark >= 0) ? rightDiff : leftDiff;
            int tmp = GetRandomNumberNoLinear_MinMoreOften(mutationMax, mutationRate) * randomMark;

            return value + tmp;
        }


        public int GetRandomNumber(int min, int max, int ignore)
        {
            if (!(min <= ignore && ignore < max)) return GetRandomNumber(min, max);

            int tmp = GetRandomNumber(min, max - 1);
            return (tmp >= ignore) ? tmp + 1 : tmp;
        }


        public int GetRandomChangeValue(int oldValue, int min, int max)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            int diff = GetRandomNumber(0, leftDelta + rightDelta);

            return oldValue + diff - leftDelta;
        }


        public int GetRandomChangeValue(int oldValue, int min, int max, byte mutationRate)
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


        public int GetRandomChangeValueGuaranted(int oldValue, int min, int max, byte mutationRate)
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
        private int GetValueByMutationRate(int value, int minValue, byte mutationRate)
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
        public int DivWithMathRouding(int numerator, int denominator)
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

        public void swap<T>(ref T p1, ref T p2)
        {
            T tmp = p1;
            p1 = p2;
            p2 = tmp;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int fastAbs(int value)
        {
            return (value ^ (value >> 31)) - (value >> 31);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FastExp(double val)
        {
            double x = 1.0 + val / 1024.0;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x;
            return x;
        }
    }
}
