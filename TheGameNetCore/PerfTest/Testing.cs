using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BonesLib.FixedForwardNN;
using BonesLib2.FixedForwardNN;
using BonesLib2.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;


namespace PerfTest
{
    public class Testing
    {
        public Testing()
        {

        }

        private void Pef()
        {
            var kk = new int[1000];
            Array.Fill(kk, 5);
        }

        private void Pef2()
        {
            Span<int> kk = stackalloc int[1000];
            kk.Fill(5);
        }

        [Benchmark]
        public void GamePerf() => Pef();

        [Benchmark]
        public void GamePerf2() => Pef2();

        [Benchmark]
        public void GamePerf3()
        {
            var kk = ArrayPool<int>.Shared.Rent(1000);
            kk.AsSpan(1000).Fill(5);
        }


    }

    [HardwareCounters(
       //HardwareCounter.CacheMisses,
        HardwareCounter.BranchMispredictions,
        HardwareCounter.TotalCycles,
        HardwareCounter.LlcMisses
        )]
    [DisassemblyDiagnoser]
    public class NNBench
    {
        FixedForwardNN ffnn;
        FixedForwardNNTest ffnn2;
        float [] input = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

        float [] expected = new float[] { 0.2f, 0.9f, 0.4f, 0.5f, 0.1f };


        public NNBench()
        {
            ffnn = new FixedForwardNN(5, 5);
            ffnn.SetTopology(new short[] { 460, 100, 50, 50 });
            ffnn2 = new FixedForwardNNTest(5, 5);
            ffnn2.SetTopology(new short[] { 460, 100, 50, 50 });

        }

        [Benchmark]
        public void Forward()
        {
           // ffnn.InitBaseWeights();

            //Array.Copy(input, ffnn.Inputs, input.Length);

            //ffnn.Evaluate();

            input.AsSpan().CopyTo(ffnn.Inputs);

            ffnn.Evaluate();
            //float error = Error(expected, ffnn.Outputs);

            // ffnn.BackPropagate(0.1f, expected, 1.0f, 1.0f);
        }

        [Benchmark]
        public void Forward2()
        {
            input.AsSpan().CopyTo(ffnn2.Inputs);

            ffnn2.Evaluate();
        }
    }

    [HardwareCounters(
       //HardwareCounter.CacheMisses,
        HardwareCounter.BranchMispredictions,
        HardwareCounter.BranchInstructions,
        HardwareCounter.TotalCycles
       
        )]
    [DisassemblyDiagnoser]
    public class BenchNeuronSum
    {
        float[] _input;
        float[] _weights;
        float[] _output;
        const int inputSize = 180;

        public BenchNeuronSum()
        {
            _input = new float[inputSize];
            _output = new float[inputSize / 10];
            _weights = new float[_output.Length * _input.Length];
            for (int i = 0; i < inputSize; i++)
            {
                _input[i] = 1.0f * i;
            }

            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = 0.001f * i;
            }
        }

        [Benchmark]
        public void SumNS_Pure()
        {
            int index = 0;
            for (int i = 0; i < _output.Length; i++)
            {

                _output[i] = FastDotProduct.PureUnroll(_input, _weights.AsSpan(index, _input.Length));
                index += _input.Length;
            }

        }

        [Benchmark]
        public void SumNS_Vector8()
        {
            int index = 0;
            for (int i = 0; i < _output.Length; i++)
            {

                _output[i] = FastDotProduct.Vector8r(_input, _weights.AsSpan(index, _input.Length));
                index+=_input.Length;
            }

        }

        [Benchmark]
        public void SumNS_Vector8_Group()
        {
            int index = 0;
            int i = 0;
            for (; i < _output.Length-1; i+=2)
            {

                var res = FastDotProduct.Double_Vector8(_input, _weights.AsSpan(index, _input.Length), _weights.AsSpan(index+ _input.Length, _input.Length));
                _output[i] = res.sumN1;
                _output[i+1] = res.sumN2;

                index += _input.Length*2;
            }

            if(i < _output.Length)
            {
                _output[i] = FastDotProduct.Vector8_1(_input, _weights.AsSpan(index, _input.Length));
            }
        }

        [Benchmark]
        public void SumNS_Vector8_GroupUnroll()
        {
            int index = 0;
            int i = 0;
            for (; i < _output.Length - 1; i += 2)
            {

                var res = FastDotProduct.Double_Vector81(_input, _weights.AsSpan(index, _input.Length), _weights.AsSpan(index + _input.Length, _input.Length));
                _output[i] = res.sumN1;
                _output[i + 1] = res.sumN2;

                index += _input.Length * 2;
            }

            if (i < _output.Length)
            {
                _output[i] = FastDotProduct.Vector8_1(_input, _weights.AsSpan(index, _input.Length));
            }
        }

        [Benchmark]
        public void SumNS_Vector8_Group4()
        {
            int index = 0;
            int i = 0;
            for (; i < _output.Length - 3; i += 4)
            {

                var res = FastDotProduct.Quad_Vector8(_input, _weights.AsSpan(index, _input.Length)
                    , _weights.AsSpan(index + _input.Length, _input.Length),
                    _weights.AsSpan(index + _input.Length*2, _input.Length),
                    _weights.AsSpan(index + _input.Length*3, _input.Length));
                _output[i] = res.sumN1;
                _output[i + 1] = res.sumN2;
                _output[i + 2] = res.sumN3;
                _output[i + 3] = res.sumN4;

                index += _input.Length * 4;
            }

            if (i < _output.Length)
            {
                _output[i] = FastDotProduct.Vector8_1(_input, _weights.AsSpan(index, _input.Length));
            }
        }

        [Benchmark]
        public void SumNS_Pure_Group()
        {
            int index = 0;
            int i = 0;
            for (; i < _output.Length - 1; i += 2)
            {

                var res = FastDotProduct.Double_PureUnroll(_input, _weights.AsSpan(index, _input.Length), _weights.AsSpan(index + _input.Length, _input.Length));
                _output[i] = res.sumN1;
                _output[i + 1] = res.sumN2;

                index += _input.Length * 2;
            }

            if (i < _output.Length)
            {
                _output[i] = FastDotProduct.PureUnroll(_input, _weights.AsSpan(index, _input.Length));
            }
        }
    }


    [HardwareCounters(
        HardwareCounter.BranchMispredictions,
        HardwareCounter.BranchInstructions,
        HardwareCounter.TotalCycles
        )]
    [DisassemblyDiagnoser]
    //[MemoryDiagnoser]
    public class BenchVector
    {
        
        float[] _input ;
        float[] _input2;
        float[] _output;

        const int inputSize = 180000;
        public BenchVector()
        {
            _input = new float[inputSize];
            _input2 = new float[inputSize];
            _output = new float[inputSize/4];
            for(int i = 0; i < inputSize; i++)
            {
                _input[i] = 1.0f*i;
                _input2[i] = 6.0f * i;
            }
        }


        [Benchmark]
        public void Vector8()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector8(_input, _input2);

            _output[0] = sum;

        }

        [Benchmark]
        public void Vector8Dor()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector8Dot(_input, _input2);

            _output[0] = sum;

        }

        [Benchmark]
        public void Vector8x()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector8x(_input, _input2);

            _output[0] = sum;

        }

        [Benchmark]
        public void Vector8r()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector8r(_input, _input2);

            _output[0] = sum;

        }

        [Benchmark]
        public void Vector8r1()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector8r1(_input, _input2);

            _output[0] = sum;

        }



        // [Benchmark]
        public void Vector41()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector41(_input, _input2);

            _output[0] = sum;

        }

       // [Benchmark]
        public void Vector41r()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector41r(_input, _input2);

            _output[0] = sum;

        }

        [Benchmark]
        public void Vector41unroll()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Vector41unroll(_input, _input2);

            _output[0] = sum;

        }

        

        //[Benchmark]
        public void Pure4()
        {
            float sum = 0.0f;

            sum = FastDotProduct.Pure(_input, _input2);

            _output[0] = sum;
        }

        [Benchmark]
        public void PureUnroll()
        {
            float sum = 0.0f;

            sum = FastDotProduct.PureUnroll(_input, _input2);

            _output[0] = sum;
        }

        [Benchmark]
        public void PureUnroll8()
        {
            float sum = 0.0f;

            sum = FastDotProduct.PureUnroll8(_input, _input2);

            _output[0] = sum;
        }



    }
}
