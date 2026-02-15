using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using PqcResearchApp.Benchmarks;
using PqcResearchApp.ClassicalAlgorithms;
using PqcResearchApp.PqcAlgorithms;

namespace PqcResearchApp;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("1. ARTIFACT SIZES ANALYSIS (Network Overhead)");

        try
        {
            // 1. Classical Algorithms (RSA, ECC)
            // RSA-4096
            using (var rsa = new RsaService(4096))
            {
                rsa.PrintDetails();
            }

            // ECC P-256 (Key Exchange)
            using (var eccKem = new EccKemService())
            {
                eccKem.PrintDetails();
            }

            // ECC P-384 (Signatures)
            using (var eccDsa = new EccDsaService())
            {
                eccDsa.PrintDetails();
            }

            // 2. Post-Quantum Algorithms (Native .NET 10)
            Console.WriteLine("-----------------------------------------------------");

            // ML-KEM-768 (Kyber)
            using (var mlKem = new MlKemService())
            {
                mlKem.PrintArtifactSizes();
            }

            // ML-DSA-65 (Dilithium)
            using (var mlDsa = new MlDsaService())
            {
                mlDsa.PrintDetails();
            }
        }
        catch (PlatformNotSupportedException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n [CRITICAL ERROR] PQC Hardware Support Missing: {ex.Message}");
            Console.WriteLine("Ensure you are running on Windows 11 24H2 or later.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[WARNING] Error measuring artifact sizes: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\n=====================================================\n");
        Console.WriteLine("Press any key to start Performance Benchmarks...");
        Console.ReadKey();

        Console.WriteLine("\n 2. PERFORMANCE BENCHMARKS (Speed & Memory)");

        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance));

        BenchmarkRunner.Run<KemBenchmarks>(config);
        BenchmarkRunner.Run<SignatureBenchmarks>(config);

        Console.ReadKey();
    }
}