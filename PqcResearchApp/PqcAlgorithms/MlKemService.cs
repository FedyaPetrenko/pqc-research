using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace PqcResearchApp.PqcAlgorithms;

/// <summary>
/// Service that wraps the native ML-KEM implementation (MLKem) to provide
/// key generation, encapsulation and decapsulation operations.
/// </summary>
/// <remarks>
/// This service constructs a persistent ML-KEM key pair using the
/// <see cref="MLKemAlgorithm.MLKem768"/> algorithm and exposes helpers to:
/// - encapsulate a shared secret (producing a ciphertext and shared secret),
/// - decapsulate a shared secret from a ciphertext,
/// - export artifact sizes for diagnostic purposes.
///
/// The constructor will throw <see cref="PlatformNotSupportedException"/> when
/// the current OS/hardware does not support ML-KEM.
/// </remarks>
public class MlKemService : IDisposable
{
    private readonly MLKem _kemKey;
    private readonly MLKemAlgorithm _algorithm = MLKemAlgorithm.MLKem768;

    /// <summary>
    /// Initializes a new instance of the <see cref="MlKemService"/> class and
    /// generates a persistent ML-KEM key pair using the configured algorithm.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when ML-KEM is not supported on the running platform.
    /// </exception>
    public MlKemService()
    {
        if (!MLKem.IsSupported)
            throw new PlatformNotSupportedException("ML-KEM is not supported on this OS/Hardware.");

        _kemKey = MLKem.GenerateKey(_algorithm);
    }

    /// <summary>
    /// Generates a new ephemeral ML-KEM key pair and immediately disposes it.
    /// </summary>
    /// <remarks>
    /// This method demonstrates key generation but does not replace the service's
    /// internal key pair. The generated key is scoped to the method and disposed
    /// when the method returns.
    /// </remarks>
    public void GenerateKey()
    {
        using var key = MLKem.GenerateKey(_algorithm);
    }

    /// <summary>
    /// Encapsulates a shared secret using the service's public key.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - <c>ciphertext</c>: the encapsulated ciphertext to send to the holder of the private key,
    /// - <c>sharedSecret</c>: the derived shared secret bytes.
    /// </returns>
    public (byte[] sharedSecret, byte[] ciphertext) Encapsulate()
    {
        _kemKey.Encapsulate(out var ciphertext, out var sharedSecret );
        return (sharedSecret, ciphertext);
    }

    /// <summary>
    /// Decapsulates the provided ciphertext using the service's private key and
    /// returns the recovered shared secret.
    /// </summary>
    /// <param name="ciphertext">The ciphertext produced by a corresponding encapsulation operation.</param>
    /// <returns>The recovered shared secret bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ciphertext"/> is null.</exception>
    /// <exception cref="CryptographicException">Thrown when decapsulation fails or ciphertext is invalid.</exception>
    public byte[] Decapsulate(byte[] ciphertext)
    {
        return _kemKey.Decapsulate(ciphertext);
    }

    /// <summary>
    /// Exports current key artifacts and prints their sizes to the console:
    /// public key, private key (PKCS#8) and a sample ciphertext produced by encapsulation.
    /// </summary>
    /// <remarks>
    /// Intended for simple diagnostic output to observe artifact sizes for the configured algorithm.
    /// </remarks>
    [Experimental("SYSLIB5006")]
    public void PrintArtifactSizes()
    {
        var publicKeyInfo = _kemKey.ExportSubjectPublicKeyInfo();

        var privateKey = _kemKey.ExportPkcs8PrivateKey();

        var (_, ciphertext) = Encapsulate();

        Console.WriteLine($"[Native .NET 10] ML-KEM-768 | " +
                          $"Public Key: {publicKeyInfo.Length} B | " +
                          $"Private Key: {privateKey.Length} B | " +
                          $"Ciphertext: {ciphertext.Length} B");
    }

    /// <summary>
    /// Disposes the underlying ML-KEM key pair and releases native resources.
    /// </summary>
    public void Dispose()
    {
        _kemKey?.Dispose();
    }
}