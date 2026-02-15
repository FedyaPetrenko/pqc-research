using System.Security.Cryptography;

namespace PqcResearchApp.ClassicalAlgorithms;

/// <summary>
/// Provides a small wrapper around the platform <see cref="ECDsa"/> implementation using the NIST P-384 curve.
/// </summary>
/// <remarks>
/// - Uses <see cref="ECCurve.NamedCurves.nistP384"/> (approx. 192 bits of security).
/// - Signing and verification use <see cref="HashAlgorithmName.SHA256"/>.
/// - The underlying <see cref="ECDsa"/> instance is created once and disposed when this service is disposed.
/// - This class is a lightweight helper for measuring/experimenting with native ECC signatures in the codebase.
/// </remarks>
public class EccDsaService : IDisposable
{
    // NIST P-384 provides ~192 bits of security, comparable to ML-DSA-65 (NIST Level 3)
    private readonly ECDsa _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);

    /// <summary>
    /// Forces generation of a fresh ephemeral P-384 key pair by creating a temporary <see cref="ECDsa"/> instance
    /// and exporting its public parameters.
    /// </summary>
    /// <remarks>
    /// The produced key is not stored in this service. This method can be used to exercise platform key
    /// generation code paths (for benchmarking or warming up cryptographic providers).
    /// </remarks>
    public void GenerateKey()
    {
        using var temp = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        _ = temp.ExportParameters(false);
    }

    /// <summary>
    /// Signs the provided data using ECDSA (P-384) and SHA-256.
    /// </summary>
    /// <param name="data">The input bytes to sign. The method does not modify this array.</param>
    /// <returns>The signature bytes produced by the underlying <see cref="ECDsa"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">If the service has been disposed.</exception>
    public byte[] Sign(byte[] data)
    {
        return _ecdsa.SignData(data, HashAlgorithmName.SHA256);
    }

    /// <summary>
    /// Verifies an ECDSA signature over the provided data using SHA-256.
    /// </summary>
    /// <param name="data">The original data that was signed.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns><c>true</c> if the signature is valid for the data and the current public key; otherwise <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">If the service has been disposed.</exception>
    public bool Verify(byte[] data, byte[] signature)
    {
        return _ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
    }

    /// <summary>
    /// Writes a short summary to the console showing the exported public key size and a sample signature size.
    /// </summary>
    /// <remarks>
    /// This method exports the subject public key info and signs a zeroed 32-byte buffer to show typical sizes.
    /// It is intended for informational or diagnostic output.
    /// </remarks>
    public void PrintDetails()
    {
        var publicKey = _ecdsa.ExportSubjectPublicKeyInfo();
        var privateKey = _ecdsa.ExportPkcs8PrivateKey();

        var sign = Sign(new byte[32]);
        Console.WriteLine($"[Native] ECC-P384 (Sig) |" +
                          $" Public Key: {publicKey.Length} B |" +
                          $" Private Key: {privateKey.Length} B |" +
                          $" Signature: {sign.Length} B");
    }

    /// <summary>
    /// Disposes the underlying <see cref="ECDsa"/> instance and frees associated resources.
    /// </summary>
    public void Dispose()
    {
        _ecdsa?.Dispose();
    }
}