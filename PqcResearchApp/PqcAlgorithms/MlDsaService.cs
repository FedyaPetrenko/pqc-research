using System.Security.Cryptography;

namespace PqcResearchApp.PqcAlgorithms;

/// <summary>
/// Service wrapper around the native .NET ML-DSA implementation (MLDsa).
/// </summary>
/// <remarks>
/// This class encapsulates key generation, signing and verification using the
/// <see cref="MLDsa"/> API introduced in .NET 10. The implementation picks the
/// <see cref="MLDsaAlgorithm.MLDsa65"/> algorithm by default.
/// </remarks>
public class MlDsaService : IDisposable
{
    private readonly MLDsa _dsaKey;
    private readonly MLDsaAlgorithm _algorithm = MLDsaAlgorithm.MLDsa65;

    /// <summary>
    /// Initializes a new instance of <see cref="MlDsaService"/>.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when ML-DSA is not supported on the running platform (<see cref="MLDsa.IsSupported"/> is false).
    /// </exception>
    public MlDsaService()
    {
        if (!MLDsa.IsSupported)
            throw new PlatformNotSupportedException("ML-DSA is not supported on this OS.");

        _dsaKey = MLDsa.GenerateKey(_algorithm);
    }

        public void GenerateKey()
    {
        using var key = MLDsa.GenerateKey(_algorithm);
    }

    /// <summary>
    /// Signs the provided data using the service's private ML-DSA key.
    /// </summary>
    /// <param name="data">The input data to sign. Expected to be a non-null byte array.</param>
    /// <returns>The raw signature bytes produced by ML-DSA.</returns>
    public byte[] Sign(byte[] data)
    {
        return _dsaKey.SignData(data);
    }

    /// <summary>
    /// Verifies a signature for the provided data using the service's public ML-DSA key.
    /// </summary>
    /// <param name="data">The original data that was signed.</param>
    /// <param name="signature">The signature bytes to verify.</param>
    /// <returns><c>true</c> when the signature is valid for the provided data; otherwise <c>false</c>.</returns>
    public bool Verify(byte[] data, byte[] signature)
    {
        return _dsaKey.VerifyData(data, signature);
    }

    /// <summary>
    /// Writes basic information about the keys and a sample signature to the console.
    /// </summary>
    /// <remarks>
    /// The method exports the subject public key info and the private key in PKCS#8 format,
    /// then prints their sizes (in bytes) along with the size of a signature produced for a
    /// 32-byte zeroed buffer. Intended for informational and benchmarking output.
    /// </remarks>
    public void PrintDetails()
    {
        var publicKeyInfo = _dsaKey.ExportSubjectPublicKeyInfo();
        var privateKey = _dsaKey.ExportPkcs8PrivateKey();
        var sign = Sign(new byte[32]);

        Console.WriteLine(
            $"[Native .NET 10] ML-DSA-65  |" +
            $" PubKey: {publicKeyInfo.Length} B |" +
            $" PrivKey: {privateKey.Length} B |" +
            $" Signature: {sign.Length} B");
    }

    /// <summary>
    /// Releases resources held by this instance.
    /// </summary>
    /// <remarks>
    /// Disposes the underlying <see cref="MLDsa"/> key. After calling this method,
    /// the instance should not be used for signing or verification.
    /// </remarks>
    public void Dispose()
    {
        _dsaKey?.Dispose();
    }
}