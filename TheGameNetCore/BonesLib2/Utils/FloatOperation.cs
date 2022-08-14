using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.Utils
{
    public class FloatOperation
    {
        private RandomGen _rnd;
        private int _precision;

        public FloatOperation(int precision):
            this(precision,new RandomGen(0))
        {

        }

        public FloatOperation(int precision, RandomGen rnd)
        {
            _precision = precision;
            _rnd = rnd;
        }

        public float RndFloat()
        {
            return RndFloat(0.0f, 1.0f);
        }
        public float RndFloat(float min, float max)
        {
            float powTen = MathF.Pow(10, _precision);
            int minInt = (int)MathF.Truncate(min * (powTen));
            int maxInt = (int)MathF.Truncate(max * (powTen));

            return _rnd.GetRandomNumber(minInt, maxInt) / powTen;
        }
    }
}
