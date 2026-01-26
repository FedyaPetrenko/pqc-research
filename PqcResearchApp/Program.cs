using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using PqcResearchApp.Benchmarks;

namespace PqcResearchApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance));

            Console.WriteLine("PQC Research Benchmark (Native .NET 10)");

            BenchmarkRunner.Run<KemBenchmarks>(config);
            BenchmarkRunner.Run<SignatureBenchmarks>(config);

            Console.ReadKey();
        }
    }
}