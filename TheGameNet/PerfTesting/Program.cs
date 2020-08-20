using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfTesting
{
    class Program
    {
        static void Main(string[] args)
        {
             var summary = BenchmarkRunner.Run<TestTesting>();
            //summary.
        }
    }
}
