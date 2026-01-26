using BenchmarkDotNet.Attributes;
using PqcResearchApp.PqcAlgorithms;
using PqcResearchApp.RegularAlgorithms;

namespace PqcResearchApp.Benchmarks;

[MemoryDiagnoser]
[CsvMeasurementsExporter]
//[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class KemBenchmarks
{
    private RsaService? _rsa;
    private EccKemService? _ecc;
    private MlKemService? _pqc;

    [GlobalSetup]
    public void Setup()
    {
        _rsa = new RsaService(4096);
        _ecc = new EccKemService();
        _pqc = new MlKemService();
    }

    [Benchmark(Description = "RSA-4096 KeyGen")]
    public void RsaKeyGen() => _rsa!.GenerateKey();

    [Benchmark(Description = "ECC-P256 KeyGen")]
    public void EccKeyGen() => _ecc!.GenerateKey();

    [Benchmark(Description = "ML-KEM-768 KeyGen")]
    public void MlKemKeyGen() => _pqc!.GenerateKey();

    [Benchmark(Description = "RSA-4096 Encap")]
    public byte[] RsaEncap() => _rsa!.Encrypt(new byte[32]);

    [Benchmark(Description = "ECC-P256 Encap")]
    public byte[] EccEncap() => _ecc!.SharedSecret();

    [Benchmark(Description = "ML-KEM-768 Encap")]
    public object MlKemEncap() => _pqc!.Encapsulate();

    [GlobalCleanup]
    public void Cleanup()
    {
        _rsa?.Dispose();
        _ecc?.Dispose();
        _pqc?.Dispose();
    }
}