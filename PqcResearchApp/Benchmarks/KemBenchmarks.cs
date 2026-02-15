using BenchmarkDotNet.Attributes;
using PqcResearchApp.ClassicalAlgorithms;
using PqcResearchApp.PqcAlgorithms;

namespace PqcResearchApp.Benchmarks;

[MemoryDiagnoser]
[CsvMeasurementsExporter]
//[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class KemBenchmarks
{
    private RsaService? _rsa;
    private EccKemService? _ecc;
    private MlKemService? _pqc;

    private byte[]? _rsaCiphertext;
    private byte[]? _eccCiphertext;
    private byte[]? _mlkemCiphertext;

    [GlobalSetup]
    public void Setup()
    {
        _rsa = new RsaService(4096);
        _ecc = new EccKemService();
        _pqc = new MlKemService();

        _rsaCiphertext = _rsa.Encrypt(new byte[32]);
        var (_, eccCiphertext) = _ecc.Encapsulate();
        _eccCiphertext = eccCiphertext;
        var (_, mlkemCiphertext) = _pqc.Encapsulate();
        _mlkemCiphertext = mlkemCiphertext;

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
    public (byte[], byte[]) EccEncap() => _ecc!.Encapsulate();

    [Benchmark(Description = "ML-KEM-768 Encap")]
    public object MlKemEncap() => _pqc!.Encapsulate();

    [Benchmark(Description = "RSA-4096 Decap")]
    public byte[] RsaDecap() => _rsa!.Decrypt(_rsaCiphertext!);

    [Benchmark(Description = "ECC-P256 Decap")]
    public byte[] EccDecap() => _ecc!.Decapsulate(_eccCiphertext!);

    [Benchmark(Description = "ML-KEM-768 Decap")]
    public object MlKemDecap() => _pqc!.Decapsulate(_mlkemCiphertext!);

    [GlobalCleanup]
    public void Cleanup()
    {
        _rsa?.Dispose();
        _ecc?.Dispose();
        _pqc?.Dispose();
    }
}