using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BonesLib2.Utils
{

    public static class FastDotProduct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pure(Span<float> input, Span<float> weights)
        {
            int w = 0;

            float sum4 = 0.0f;

            for (; w < input.Length && w < weights.Length; w++)
            {
                sum4 += input[w] * weights[w];

            }

            return sum4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PureUnroll(Span<float> input, Span<float> weights)
        {
            int w = 0;

            float sum1 = 0.0f;
            float sum2 = 0.0f;
            float sum3 = 0.0f;
            float sum4 = 0.0f;

            for (; (w < input.Length - 3) && (w < weights.Length - 3); w += 4)
            {
                sum4 += input[w + 3] * weights[w + 3];
                sum3 += input[w] * weights[w];
                sum2 += input[w + 1] * weights[w + 1];
                sum1 += input[w + 2] * weights[w + 2];

            }

            while (w < input.Length)
            {
                sum1 += input[w] * weights[w];
                w++;
            }

            return sum1 + sum2 + sum3 + sum4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PureUnroll8(Span<float> input, Span<float> weights)
        {
            int w = 0;

            float sum1 = 0.0f;
            float sum2 = 0.0f;
            float sum3 = 0.0f;
            float sum4 = 0.0f;
            float sum5 = 0.0f;
            float sum6 = 0.0f;
            float sum7 = 0.0f;
            float sum8 = 0.0f;

            for (; (w < input.Length - 7) && (w < weights.Length - 7); w += 8)
            {
                sum4 += input[w + 3] * weights[w + 3];
                sum3 += input[w] * weights[w];
                sum2 += input[w + 1] * weights[w + 1];
                sum1 += input[w + 2] * weights[w + 2];
                sum5 += input[w + 4] * weights[w + 4];
                sum6 += input[w + 5] * weights[w + 5];
                sum7 += input[w + 6] * weights[w + 6];
                sum8 += input[w + 7] * weights[w + 7];
            }

            while (w < input.Length)
            {
                sum1 += input[w] * weights[w];
                w++;
            }

            return sum1 + sum2 + sum3 + sum4 + sum5 +sum6+sum7+sum8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float sumN1, float sumN2) Double_PureUnroll(Span<float> input, Span<float> weights, Span<float> weights2)
        {
            int w = 0;

            float sum1 = 0.0f;
            float sum2 = 0.0f;
            float sum3 = 0.0f;
            float sum4 = 0.0f;
            float sum5 = 0.0f;
            float sum6 = 0.0f;
            float sum7 = 0.0f;
            float sum8 = 0.0f;

            for (; (w < input.Length - 3) && (w < weights.Length - 3) && (w < weights2.Length - 3); w += 4)
            {
                sum4 += input[w + 3] * weights[w + 3];
                sum3 += input[w] * weights[w];
                sum2 += input[w + 1] * weights[w + 1];
                sum1 += input[w + 2] * weights[w + 2];

                sum8 += input[w + 3] * weights2[w + 3];
                sum7 += input[w] * weights2[w];
                sum6 += input[w + 1] * weights2[w + 1];
                sum5 += input[w + 2] * weights2[w + 2];

            }

            while (w < input.Length)
            {
                sum1 += input[w] * weights[w];
                sum5 += input[w] * weights2[w];
                w++;
            }

            return (sum1 + sum2 + sum3 + sum4, sum5+ sum6+sum7+sum8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float sumN1, float sumN2) Double_Vector8(Span<float> input, Span<float> weights, Span<float> weights2)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;
            float sum2 = 0.0f;

            int wi = 0;
            int k = 0;
            var tmpSumVec = System.Numerics.Vector<float>.Zero;
            var tmpSumVec2 = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var w = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));
            var w2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights2.Slice(0, vsize));

            if (input.Length > 15)
            {
                k += wCount * w.Length;

               
                for (wi = 0; (wi < v.Length )
                    && (wi < w2.Length )
                    && (wi < w.Length )
                    ; wi ++)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[wi], w[wi]);
                    tmpSumVec2 += System.Numerics.Vector.Multiply(v[wi], w2[wi]);

                }
                
            }

            //if((v.Length & 1) == 1)
            //    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);

            // tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
                sum2 += input[k] * weights2[k];
            }

            return (System.Numerics.Vector.Sum(tmpSumVec) + sum, System.Numerics.Vector.Sum(tmpSumVec2) + sum2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float sumN1, float sumN2) Double_Vector81(Span<float> input, Span<float> weights, Span<float> weights2)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;
            float sum2 = 0.0f;

            int wi = 0;
            int k = 0;
            var tmpSumVec = System.Numerics.Vector<float>.Zero;
            var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
            var tmpSumVec3 = System.Numerics.Vector<float>.Zero;
            var tmpSumVec4 = System.Numerics.Vector<float>.Zero;
            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var w = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));
            var w2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights2.Slice(0, vsize));

            if (input.Length > 15)
            {
                k += wCount * (w.Length - (w.Length&1));


                for (wi = 0; (wi < v.Length-1)
                    && (wi < w2.Length-1)
                    && (wi < w.Length-1)
                    ; wi+=2)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[wi], w[wi]);
                    tmpSumVec2 += System.Numerics.Vector.Multiply(v[wi], w2[wi]);
                    tmpSumVec3 += System.Numerics.Vector.Multiply(v[wi+1], w[wi+1]);
                    tmpSumVec4 += System.Numerics.Vector.Multiply(v[wi+1], w2[wi+1]);

                }
                tmpSumVec += tmpSumVec3;
                tmpSumVec2 += tmpSumVec4;
            }

            //if((v.Length & 1) == 1)
            //    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);

            // tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
                sum2 += input[k] * weights2[k];
            }

            return (System.Numerics.Vector.Sum(tmpSumVec) + sum, System.Numerics.Vector.Sum(tmpSumVec2) + sum2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float sumN1, float sumN2, float sumN3, float sumN4) Quad_Vector8(Span<float> input, Span<float> weights, Span<float> weights2, Span<float> weights3, Span<float> weights4)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;
            float sum2 = 0.0f;
            float sum3 = 0.0f;
            float sum4 = 0.0f;

            int wi = 0;
            int k = 0;
            var tmpSumVec = System.Numerics.Vector<float>.Zero;
            var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
            var tmpSumVec3 = System.Numerics.Vector<float>.Zero;
            var tmpSumVec4 = System.Numerics.Vector<float>.Zero;


            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var w = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));
            var w2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights2.Slice(0, vsize));
            var w3 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights3.Slice(0, vsize));
            var w4 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights4.Slice(0, vsize));

            if (input.Length > 15)
            {
                k += wCount * w.Length;


                for (wi = 0; (wi < v.Length)
                    && (wi < w2.Length)
                    && (wi < w.Length)
                    && (wi < w3.Length)
                    && (wi < w4.Length)
                    ; wi++)
                {
                    var kk = v[wi];
                    tmpSumVec += System.Numerics.Vector.Multiply(kk, w[wi]);
                    tmpSumVec2 += System.Numerics.Vector.Multiply(kk, w2[wi]);
                    tmpSumVec3 += System.Numerics.Vector.Multiply(kk, w3[wi]);
                    tmpSumVec4 += System.Numerics.Vector.Multiply(kk, w4[wi]);
                }

            }

            for (; k < input.Length; k++)
            {
                var inp = input[k];
                sum += inp * weights[k];
                sum2 += inp * weights2[k];
                sum3 += inp * weights3[k];
                sum4 += inp * weights4[k];

            }

            return (System.Numerics.Vector.Sum(tmpSumVec) + sum, System.Numerics.Vector.Sum(tmpSumVec2) + sum2
               , System.Numerics.Vector.Sum(tmpSumVec3) + sum3, System.Numerics.Vector.Sum(tmpSumVec4) + sum4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;

            int w = 0;
            int k = 0;
            var tmpSumVec = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length > 15)
            {
                k += wCount * (v2.Length - (v2.Length & 1));

                var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
                for (w = 0; (w < v.Length - 1)
                    && (w < v2.Length - 1)
                    ; w += 2)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                    tmpSumVec2 += System.Numerics.Vector.Multiply(v[w + 1], v2[w + 1]);
                   
                }
                tmpSumVec += tmpSumVec2;
            }

            //if((v.Length & 1) == 1)
            //    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);

            // tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
            }

            return System.Numerics.Vector.Sum(tmpSumVec) +
                    sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8_1(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;

            int w = 0;
            int k = 0;
            var tmpSumVec = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length >=System.Numerics.Vector<float>.Count)
            {
                k += wCount* v2.Length ;

                for (w = 0; (w < v.Length )
                    && (w < v2.Length )
                    ; w ++)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                  
                }
                
            }

            //if((v.Length & 1) == 1)
            //    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);

            // tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
            }

            return System.Numerics.Vector.Sum(tmpSumVec) +
                    sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8Dot(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

           

            int w = 0;
            int k = 0;
            var tmpSumVec = 0.0f;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length > 15)
            {
                k+= wCount*(v2.Length - (v2.Length&1));
                var tmpSumVec2 = 0.0f;
                for (w = 0; (w < v.Length - 1)
                    && (w < v2.Length - 1)
                    ; w += 2)
                {
                    tmpSumVec += System.Numerics.Vector.Dot(v[w], v2[w]);
                    tmpSumVec2 += System.Numerics.Vector.Dot(v[w + 1], v2[w + 1]);
                }
                tmpSumVec += tmpSumVec2;
            }

            //if((v.Length & 1) == 1)
            //    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);

            // tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                tmpSumVec += input[k] * weights[k];
            }

            return tmpSumVec
                    ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8x(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;

            int w = 0;
            int k = 0;
            var tmpSumVec = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length > 15)
            {
                var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
                for (w = 0; (w < v.Length - 1)
                    && (w < v2.Length - 1)
                    ; w += 2)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                    tmpSumVec2 += System.Numerics.Vector.Multiply(v[w + 1], v2[w + 1]);

                }

                k += wCount * (v.Length - (v.Length & 1));
                tmpSumVec += tmpSumVec2;
            }

            //if((v.Length & 1) == 1)
            //    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);

            // tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
            }

            return System.Numerics.Vector.Sum(tmpSumVec) +  sum;
        }

        



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8r(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;

            int w = 0;
            int k = 0;

            var tmpSumVec = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length > 31)
            {
                var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
                var tmpSumVec3 = System.Numerics.Vector<float>.Zero;
                var tmpSumVec4 = System.Numerics.Vector<float>.Zero;

                for (w = 0; (w < v.Length - 3)
                    && (w < v2.Length - 3)
                    ; w += 4)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                    tmpSumVec2 +=
                        System.Numerics.Vector.Multiply(v[w + 2], v2[w + 2]);
                    tmpSumVec3 += System.Numerics.Vector.Multiply(v[w + 3], v2[w + 3]);
                    tmpSumVec4 +=
                        System.Numerics.Vector.Multiply(v[w + 1], v2[w + 1]);

                    //k += wCount * 4;
                }


                tmpSumVec += tmpSumVec2;
                tmpSumVec += tmpSumVec3 += tmpSumVec4;
                //tmpSumVec += tmpSumVec4;
                k += wCount * (v.Length - (v.Length & 3));
            }

            while (w < v.Length && w < v2.Length)
            {
                tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                w++;
                k += wCount;
            }


            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
            }

            return System.Numerics.Vector.Sum(tmpSumVec) +  sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8r1(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;

            int w = 0;
            int k = 0;

            var tmpSumVec = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length > 63)
            {
                var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
                var tmpSumVec3 = System.Numerics.Vector<float>.Zero;
                var tmpSumVec4 = System.Numerics.Vector<float>.Zero;

                for (w = 0; (w < v.Length - 7)
                    && (w < v2.Length - 7)
                    ; w += 8)
                {
                    tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                    tmpSumVec2 +=
                        System.Numerics.Vector.Multiply(v[w + 4], v2[w + 4]);
                    tmpSumVec3 += System.Numerics.Vector.Multiply(v[w + 5], v2[w + 5]);
                    tmpSumVec4 +=
                        System.Numerics.Vector.Multiply(v[w + 1], v2[w + 1]);

                    tmpSumVec += System.Numerics.Vector.Multiply(v[w + 2], v2[w + 2]);
                    tmpSumVec2 +=
                        System.Numerics.Vector.Multiply(v[w + 6], v2[w + 6]);
                    tmpSumVec3 += System.Numerics.Vector.Multiply(v[w + 7], v2[w + 7]);
                    tmpSumVec4 +=
                        System.Numerics.Vector.Multiply(v[w + 3], v2[w + 3]);

                    //k += wCount * 4;
                }


                tmpSumVec += tmpSumVec2;
                tmpSumVec += tmpSumVec3 += tmpSumVec4;
                //tmpSumVec += tmpSumVec4;
                k += wCount * (v.Length - (v.Length & 7));
            }

            while (w < v.Length && w < v2.Length)
            {
                tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                w++;
                k += wCount;
            }

            //tmpSumVec += tmpSumVec2;

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
            }



            return System.Numerics.Vector.Sum(tmpSumVec) +

                    //return
                    sum;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector8r4(Span<float> input, Span<float> weights)
        {
            int wCount = System.Numerics.Vector<float>.Count;
            int vsize = input.Length & (~(wCount - 1));

            float sum = 0.0f;

            int w = 0;
            int k = 0;

            var tmpSumVec = System.Numerics.Vector<float>.Zero;

            var v = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(input.Slice(0, vsize));
            var v2 = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(weights.Slice(0, vsize));

            if (input.Length > 31)
            {
                var tmpSumVec2 = System.Numerics.Vector<float>.Zero;
                //var tmpSumVec3 = System.Numerics.Vector<float>.Zero;
                //var tmpSumVec4 = System.Numerics.Vector<float>.Zero;

                for (w = 0; (w < v.Length - 3)
                    && (w < v2.Length - 3)
                    ; w += 4)
                {
                    // new System.Numerics.Vector<float>(input[])
                    var vi = v[w];
                    var vn = v2[w];
                    var vi2 = v[w + 1];
                    var vn2 = v2[w + 1];

                    tmpSumVec += System.Numerics.Vector.Multiply(vi, vn);
                    tmpSumVec2 +=
                        System.Numerics.Vector.Multiply(vi2, vn2);

                    vi = v[w + 2];
                    vn = v2[w + 2];
                    vi2 = v[w + 3];
                    vn2 = v2[w + 3];


                    tmpSumVec2 += System.Numerics.Vector.Multiply(vi, vn);
                    tmpSumVec +=
                        System.Numerics.Vector.Multiply(vi2, vn2);

                    k += System.Numerics.Vector<float>.Count * 4;
                }

                tmpSumVec += tmpSumVec2;
                //tmpSumVec+=tmpSumVec3;
                //tmpSumVec += tmpSumVec4;
            }

            while (w < v.Length && w < v2.Length)
            {
                tmpSumVec += System.Numerics.Vector.Multiply(v[w], v2[w]);
                w++;
                k += wCount;
            }

            for (; k < input.Length; k++)
            {
                sum += input[k] * weights[k];
            }

            return System.Numerics.Vector.Sum(tmpSumVec) +    sum;
        }



        public static float Vector41(Span<float> input, Span<float> weights)
        {
            float sum = 0.0f;

            int w = 0;

            var sumV = new System.Numerics.Vector4();
            ReadOnlySpan<System.Numerics.Vector4> v = MemoryMarshal.Cast<float, System.Numerics.Vector4>(input);
            ReadOnlySpan<System.Numerics.Vector4> v2 = MemoryMarshal.Cast<float, System.Numerics.Vector4>(weights);
            for (int k = 0; (k < v.Length) && (k < v2.Length); k += 1)
            {
                sumV += System.Numerics.Vector4.Multiply(v[k], v2[k]);
                w += 16;
            }

            while (w < input.Length)
            {
                sum += input[w] * weights[w];
                w++;
            }

            return
            sumV.W + sumV.X + sumV.Z + sumV.Y;
        }

        public static float Vector41r(Span<float> input, Span<float> weights)
        {
            int mod = input.Length & (3);
            int w = 0;
            var sumV = new System.Numerics.Vector4();
            ReadOnlySpan<System.Numerics.Vector4> v = MemoryMarshal.Cast<float, System.Numerics.Vector4>(input.Slice(0, input.Length - mod));
            ReadOnlySpan<System.Numerics.Vector4> v2 = MemoryMarshal.Cast<float, System.Numerics.Vector4>(weights.Slice(0, input.Length - mod));
            int k = 0;
            for (; (k < v.Length - 1) && (k < v2.Length - 1); k += 2)
            {
                sumV += System.Numerics.Vector4.Multiply(v[k], v2[k])
                 + System.Numerics.Vector4.Multiply(v[k + 1], v2[k + 1]);
                w += 8;
            }

            //if((v.Length & 1) == 1)
            //{
            //    sumV += System.Numerics.Vector4.Multiply(v[k], v2[k]);
            //    w += 4;
            //}

            float sum = 0.0f;
            while (w < input.Length)
            {
                sum += input[w] * weights[w];
                w++;
            }
            return sumV.W + sumV.X + sumV.Z + sumV.Y + sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Vector41unroll(Span<float> input, Span<float> weights)
        {


            var sumV = new System.Numerics.Vector4();
            var sumVt = new System.Numerics.Vector4();
            ReadOnlySpan<System.Numerics.Vector4> v = MemoryMarshal.Cast<float, System.Numerics.Vector4>(input);
            ReadOnlySpan<System.Numerics.Vector4> v2 = MemoryMarshal.Cast<float, System.Numerics.Vector4>(weights);
            int k = 0;
            for (k = 0; (k < v.Length - 3) && (k < v2.Length - 3); k += 4)
            {
                sumV += System.Numerics.Vector4.Multiply(v[k], v2[k])
                 + System.Numerics.Vector4.Multiply(v[k + 1], v2[k + 1])
                   //+ System.Numerics.Vector4.Multiply(v[k + 2], v2[k + 2 ])
                   //+ System.Numerics.Vector4.Multiply(v[k + 3], v2[k + 3])
                   //;
                   //sumVt +=
                   + System.Numerics.Vector4.Multiply(v[k + 2], v2[k + 2])
                 + System.Numerics.Vector4.Multiply(v[k + 3], v2[k + 3]);
            }

            //sumV+=sumVt;

            int kk = (v.Length & 3);
            if (kk == 1)
            {
                sumV += System.Numerics.Vector4.Multiply(v[k], v2[k]);
            }
            else if (kk == 2)
            {
                sumV += System.Numerics.Vector4.Multiply(v[k], v2[k])
                 + System.Numerics.Vector4.Multiply(v[k + 1], v2[k + 1])
                    ;
            }
            else if (kk == 3)
            {
                sumV += System.Numerics.Vector4.Multiply(v[k], v2[k])
                     + System.Numerics.Vector4.Multiply(v[k + 1], v2[k + 1])
                 + System.Numerics.Vector4.Multiply(v[k + 2], v2[k + 2])
                ;
            }

            return sumV.W + sumV.X + sumV.Z + sumV.Y;
        }
    }

}
