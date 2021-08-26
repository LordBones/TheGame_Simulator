using BonesLib.FlexibleForwardNN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TheGameNet
{
    public static class TestingPerf
    {
        public static void FlexibleFNN_Speed()
        {
            FlexibleForwardNN fnn = new FlexibleForwardNN(100, 300);
            fnn.Layers.Init(100, 400, 400, 30000);
            //fnn.SetTopology(new short[] { 100, 100, 100 });

            FlexibleFNN_LayerManipulator manipulator = new FlexibleFNN_LayerManipulator(0);
            manipulator.InitRandomTopology(fnn.Layers);

            int countTest = 1000;
            int countEqual = Test_Network(fnn, countTest);
            
            Console.WriteLine("Are Equal? {0} from {1}", countEqual , countTest);


            int count = 100000;
           // FlexibleFNN_SpeedTest(count,fnn);
            FlexibleFNN_SpeedTest2(count, fnn);
        }

        private static void FlexibleFNN_SpeedTest(int count, FlexibleForwardNN fnn)
        {
            var startTS = Process.GetCurrentProcess().UserProcessorTime.TotalSeconds;
            int countRun = count;
            float sumOutput = 0.0f;
            for (int i = 0; i < countRun; i++)
            {
                Helper_FillInput(fnn.Inputs);
                fnn.Evaluate();
                sumOutput += Helper_SumOutput(fnn.Outputs);
            }

            var endTS = Process.GetCurrentProcess().UserProcessorTime.TotalSeconds;

            float speed = countRun / (float)(endTS - startTS);

            float ticksPerLink = (((float)(endTS - startTS)) * 1_500_000_000) / (countRun * fnn.Layers.NeuronLinks.Length);

            Console.WriteLine("Count links {0}", fnn.Layers.NeuronLinks.Length);
            Console.WriteLine("Count Eval per s {0:F3}       count ticks per link {1:f3}", speed,ticksPerLink);
        }

        private static void FlexibleFNN_SpeedTest2(int count, FlexibleForwardNN fnn)
        {
            var startTS = Process.GetCurrentProcess().UserProcessorTime.TotalSeconds;
            int countRun = count;
            float sumOutput = 0.0f;

          

            for (int i = 0; i < countRun; i++)
            {
                Helper_FillInput(fnn.Inputs);
                fnn.Layers.EvaluateFast();
                sumOutput += Helper_SumOutput(fnn.Outputs);
            }

            var endTS = Process.GetCurrentProcess().UserProcessorTime.TotalSeconds;

            float speed = countRun / (float)(endTS - startTS);
            float ticksPerLink = (((float)(endTS - startTS)) * 1_500_000_000) / (countRun * fnn.Layers.NeuronLinks.Length);

            Console.WriteLine("Count links {0}", fnn.Layers.NeuronLinks.Length);
            Console.WriteLine("Count Eval per s {0:F3}       count ticks per link {1:f3}", speed, ticksPerLink);
        }

        private static int Test_Network(FlexibleForwardNN fnn, int countRun)
        {
            int totalEqual = 0;
           

            for (int i =0; i < countRun; i++)
            {
                Helper_FillInput(fnn.Inputs,i);
                fnn.Evaluate();
                var output1 = fnn.Outputs.ToArray();
                Helper_FillInput(fnn.Inputs,i);
                fnn.Layers.EvaluateFast();
                var output2 = fnn.Outputs.ToArray();

                var areEqual = Helper_Compare(output1, output2);
                if (areEqual)
                    totalEqual++;
            }

            return totalEqual;
        }


        private static float Helper_SumOutput(Span<float> p)
        {
            float sum = 0.0f;
           
            for (int i = 0; i < p.Length; i++)
            {
                if(p[i]> 0.0f)
                sum += p[i];
                 
            }

            return sum;
        }



        private static bool Helper_Compare(float[] p, float [] p2)
        {
            if (p.Length != p2.Length)
                return false;

            for(int i =0;i < p.Length; i++)
            {
                if (MathF.Abs(p[i] - p2[i]) > float.Epsilon)
                    return false;
            }

            return true;
        }

        private static void Helper_FillInput(Span<float> input)
        {
            for(int i =0;i < input.Length; i += 2)
            {
                input[i] = 1.0f;
            }
        }

        private static void Helper_FillInput(Span<float> input, int randSeed)
        {
            Random rnd = new Random(randSeed);
            for (int i = 0; i < input.Length; i += 2)
            {
                input[i] = (float)rnd.NextDouble();
            }
        }
    }
}
