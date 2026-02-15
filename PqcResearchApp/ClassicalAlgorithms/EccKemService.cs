using System.Security.Cryptography;

namespace PqcResearchApp.ClassicalAlgorithms;

/// <summary>
/// Lightweight helper that demonstrates an ECC-based KEM-style workflow using
/// <see cref="ECDiffieHellman"/> over the NIST P-256 curve.
/// </summary>
/// <remarks>
/// This class is intended for benchmarking and demonstration purposes only:
/// - It uses the platform <see cref="ECDiffieHellman"/> implementation (P-256).
/// </remarks>
public class EccKemService : IDisposable
{
    // NIST P-256 is the industry standard for ECDH Key Exchange (TLS 1.3 default)
    private readonly ECDiffieHellman _ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

    /// <summary>
    /// Simulates generating a fresh ephemeral key pair.
    /// </summary>
    /// <remarks>
    /// This method creates a temporary <see cref="ECDiffieHellman"/> instance and
    /// accesses its <see cref="ECDiffieHellmanPublicKey"/> to force key generation.
    /// It is useful for benchmarks that want to measure key generation overhead
    /// without replacing the service's primary key.
    /// </remarks>
    public void GenerateKey()
    {
        // Simulate generating a fresh ephemeral key
        using var temp = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
    }

    /// <summary>
    /// Performs the encapsulation phase of an ECC-based KEM workflow.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>SharedSecret: The raw ECDH-derived key material.</description></item>
    /// <item><description>Ciphertext: The ephemeral public key in SubjectPublicKeyInfo format.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method creates a fresh ephemeral key pair, derives a shared secret via ECDH
    /// with the receiver's public key, and encapsulates the ephemeral public key as the
    /// ciphertext. The shared secret should be passed through a KDF before use in production.
    /// </remarks>
    public (byte[] SharedSecret, byte[] Ciphertext) Encapsulate()
    {
        using var ephemeralEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

        var sharedSecret = ephemeralEcdh.DeriveKeyMaterial(_ecdh.PublicKey);
        var ciphertext = ephemeralEcdh.PublicKey.ExportSubjectPublicKeyInfo();

        return (sharedSecret, ciphertext);
    }

    /// <summary>
    /// Performs the decapsulation phase of an ECC-based KEM workflow.
    /// </summary>
    /// <param name="ciphertext">The ephemeral public key in SubjectPublicKeyInfo format.</param>
    /// <returns>The raw ECDH-derived key material shared with the encapsulation peer.</returns>
    /// <remarks>
    /// This method reconstructs the ephemeral public key from the ciphertext,
    /// derives a shared secret via ECDH using the service's static private key,
    /// and the ephemeral public key. The shared secret should be passed through a KDF
    /// before use in production.
    /// </remarks>
    public byte[] Decapsulate(byte[] ciphertext)
    {
        using var ephemeralEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        ephemeralEcdh.ImportSubjectPublicKeyInfo(ciphertext, out _);

        var sharedSecret = _ecdh.DeriveKeyMaterial(ephemeralEcdh.PublicKey);

        return sharedSecret;
    }

    /// <summary>
    /// Writes basic details about the underlying ECC public key to the console.
    /// </summary>
    /// <remarks>
    /// Exports the public key in SubjectPublicKeyInfo format and prints its length in bytes.
    /// This is a convenience for human-readable benchmarking output.
    /// </remarks>
    public void PrintDetails()
    {
        var pk = _ecdh.PublicKey.ExportSubjectPublicKeyInfo();
        var sk = _ecdh.ExportPkcs8PrivateKey();

        var (_, ciphertext) = Encapsulate();

        Console.WriteLine($"[Native] ECC-P256 (KEM) |" +
                          $" Public Key: {pk.Length} B |" +
                          $" Private Key: {sk.Length} B |" +
                          $" Ciphertext: {ciphertext.Length} B");
    }

    /// <summary>
    /// Releases resources used by the <see cref="EccKemService"/>.
    /// </summary>
    /// <remarks>
    /// Disposes the internal <see cref="ECDiffieHellman"/> instance. After calling
    /// <see cref="Dispose"/>, the instance should not be used.
    /// </remarks>
    public void Dispose()
    {
        _ecdh?.Dispose();
    }
}