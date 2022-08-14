using BonesLib2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfTest
{
    internal  class ValidationTest
    {
        public static float[] w ;
        public static float[] inp ;

        public static float[] sumReference;

        static ValidationTest()
        {
            int count = 80;
            w = new float[count];
            inp = new float[count];
            sumReference = new float[count];

            for(int i = 0; i < count; i++)
            {
                inp[i] = i/10.0f;
                w[i] = i ;
            }

            for (int i = 0; i < count; i++)
            {
                sumReference[i] = FastDotProduct.Pure(w.AsSpan(0, i + 1), inp.AsSpan(0, i + 1));
            }


            }

            public static void TestDot()
        {
            TestOneFunction("Pure",(tw,tinp)=> FastDotProduct.Pure(tw.AsSpan(), tinp.AsSpan()));
            TestOneFunction("PureUnroll",(tw, tinp) => FastDotProduct.PureUnroll(tw.AsSpan(), tinp.AsSpan()));
            TestOneFunction("Vector8r",(tw, tinp) => FastDotProduct.Vector8r(tw.AsSpan(), tinp.AsSpan()));
            TestOneFunction("Vector8Dot", (tw, tinp) => FastDotProduct.Vector8Dot(tw.AsSpan(), tinp.AsSpan()));
            TestOneFunction("Vector8", (tw, tinp) => FastDotProduct.Vector8(tw.AsSpan(), tinp.AsSpan()));

        }

        private static void TestOneFunction(string name,Func<ArraySegment<float>, ArraySegment<float>, float> func)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            for (int i = 1; i <= w.Length; i++)
            {
                try
                {
                    var output = func.Invoke(new ArraySegment<float>(w, 0, i), new ArraySegment<float>(inp, 0, i));
                    sb.AppendFormat("{0} ", output);

                    if(Math.Abs(sumReference[i-1]-output) > 0.001f)
                    {
                        sb2.Append("N");
                    }
                    else
                        sb2.Append("A");
                }
                catch
                {
                    sb.AppendFormat(" ### ");
                    sb2.AppendFormat("X ");
                }
            }

            Console.WriteLine(name + " -----------");
            Console.WriteLine(sb.ToString());
            Console.WriteLine(sb2.ToString());
        }


    }
}
