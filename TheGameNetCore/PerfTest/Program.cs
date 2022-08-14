// See https://aka.ms/new-console-template for more information


using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PerfTest;


//ValidationTest.TestDot();
//return;
//var summary = BenchmarkRunner.Run<Testing>();
var summary =
BenchmarkRunner.Run<BenchNeuronSum>(
//    BenchmarkRunner.Run<Testing>(
//    BenchmarkRunner.Run<BenchVector>(
ManualConfig.CreateMinimumViable() // A configuration for our benchmarks
    .AddJob(Job.Default // Adding first job
        .WithStrategy(BenchmarkDotNet.Engines.RunStrategy.Monitoring)
        
        .WithRuntime(CoreRuntime.Core60) // .NET Framework 4.7.2
        .WithPlatform(Platform.X64) // Run as x64 application
        .WithJit(Jit.RyuJit) // Use LegacyJIT instead of the default RyuJIT
        
        .WithGcServer(true) // Use Server GC
    ));

//var summary = BenchmarkRunner.Run<NNBench>();


/*
var summary = BenchmarkRunner.Run<BenchVector>(
    //DefaultConfig.Instance.AddJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)
    );
*/


Console.WriteLine("Hello, World!");
