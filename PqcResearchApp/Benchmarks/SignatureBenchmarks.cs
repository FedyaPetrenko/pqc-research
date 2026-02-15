using BenchmarkDotNet.Attributes;
using PqcResearchApp.ClassicalAlgorithms;
using PqcResearchApp.PqcAlgorithms;

namespace PqcResearchApp.Benchmarks
{
    [MemoryDiagnoser]
    [CsvMeasurementsExporter]
    public class SignatureBenchmarks
    {
        private RsaService? _rsa;
        private EccDsaService? _ecc;
        private MlDsaService? _pqc;

        // Payload to sign (Hash of a message)
        private byte[]? _dataToSign;

        // Pre-generated signatures for Verification benchmarks
        private byte[]? _rsaSignature;
        private byte[]? _eccSignature;
        private byte[]? _pqcSignature;

        [GlobalSetup]
        public void Setup()
        {
            _rsa = new RsaService(4096);

            // Uses P-384 for Signatures (Level 3)
            _ecc = new EccDsaService();

            // Uses ML-DSA-65 (Level 3)
            _pqc = new MlDsaService();

            // Simulating a SHA-256 Hash
            _dataToSign = new byte[32];
            new Random().NextBytes(_dataToSign);

            // Pre-calculate signatures for "Verify" benchmarks
            // We do this here so the benchmark only measures the Verification logic, not the Signing.
            _rsaSignature = _rsa.Sign(_dataToSign);
            _eccSignature = _ecc.Sign(_dataToSign);
            _pqcSignature = _pqc.Sign(_dataToSign);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _rsa?.Dispose();
            _ecc?.Dispose();
            _pqc?.Dispose();
        }

        [Benchmark(Description = "RSA-4096 KeyGen")]
        public void RsaKeyGen() => _rsa!.GenerateKey();

        [Benchmark(Description = "ECDSA-P384 KeyGen")]
        public void EccKeyGen() => _ecc!.GenerateKey();

        [Benchmark(Description = "ML-DSA-65 KeyGen")]
        public void MlDsaKeyGen() => _pqc!.GenerateKey();

        [Benchmark(Description = "RSA-4096 Sign")]
        public byte[] RsaSign() => _rsa!.Sign(_dataToSign!);

        [Benchmark(Description = "ECDSA-P384 Sign")]
        public byte[] EccSign() => _ecc!.Sign(_dataToSign!);

        [Benchmark(Description = "ML-DSA-65 Sign")]
        public byte[] MlDsaSign() => _pqc!.Sign(_dataToSign!);

        [Benchmark(Description = "RSA-4096 Verify")]
        public bool RsaVerify() => _rsa!.Verify(_dataToSign!, _rsaSignature!);

        [Benchmark(Description = "ECDSA-P384 Verify")]
        public bool EccVerify() => _ecc!.Verify(_dataToSign!, _eccSignature!);

        [Benchmark(Description = "ML-DSA-65 Verify")]
        public bool MlDsaVerify() => _pqc!.Verify(_dataToSign!, _pqcSignature!);
    }
}
