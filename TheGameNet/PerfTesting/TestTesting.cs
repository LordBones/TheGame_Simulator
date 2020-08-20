using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet;

namespace PerfTesting
{
    public class TestTesting
    {
        public TestTesting()
        {

        }

        private void Pef()
        {
            Testings.Run_QLearning_Teach();
        }

        [Benchmark]
        public void GamePerf() => Pef();

    }
}
