using System.Security.Cryptography;

namespace PqcResearchApp.RegularAlgorithms;

/// <summary>
/// Lightweight helper that demonstrates an ECC-based KEM-style workflow using
/// <see cref="ECDiffieHellman"/> over the NIST P-256 curve.
/// </summary>
/// <remarks>
/// This class is intended for benchmarking and demonstration purposes only:
/// - It uses the platform <see cref="ECDiffieHellman"/> implementation (P-256).
/// - The <see cref="SharedSecret"/> method derives raw key material via ECDH; in real
///   applications you must run the derived secret through a suitable KDF and associated
///   context binding before use.
/// - This is not a fully specified KEM (no standardized encapsulation/decapsulation
///   format, no integrity or key confirmation). Use a vetted KEM library for production.
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
        _ = temp.PublicKey;
    }

    /// <summary>
    /// Derives a shared secret using the service's ECDH key pair.
    /// </summary>
    /// <returns>
    /// Raw shared secret bytes as returned by <see cref="ECDiffieHellman.DeriveKeyMaterial(ECDiffieHellmanPublicKey)"/>.
    /// </returns>
    /// <remarks>
    /// For benchmarking this method derives a secret against the instance's own public key,
    /// measuring the mathematical cost of key agreement. In real protocols the peer's
    /// public key would be supplied instead.
    /// </remarks>
    /// <exception cref="CryptographicException">
    /// Thrown if key agreement fails or the underlying provider reports an error.
    /// </exception>
    public byte[] SharedSecret()
    {
        // Simulate "Encapsulate": Derive a shared secret from the peer's public key.
        // In a benchmark, we can derive against our own public key to measure the math overhead.
        return _ecdh.DeriveKeyMaterial(_ecdh.PublicKey);
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
        Console.WriteLine($"[Native] ECC-P256 (KEM) | Public Key: {pk.Length} B");
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