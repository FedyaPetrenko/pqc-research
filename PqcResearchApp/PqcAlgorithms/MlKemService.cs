using System.Security.Cryptography;

namespace PqcResearchApp.PqcAlgorithms;

public class MlKemService : IDisposable
{
    private readonly MLKem _kemKey;
    private readonly MLKemAlgorithm _algorithm = MLKemAlgorithm.MLKem768;

    public MlKemService()
    {
        if (!MLKem.IsSupported)
            throw new PlatformNotSupportedException("ML-KEM is not supported on this OS/Hardware.");

        _kemKey = MLKem.GenerateKey(_algorithm);
    }

    public void GenerateKey()
    {
        using var key = MLKem.GenerateKey(_algorithm);
    }

    public (byte[] ciphertext, byte[] sharedSecret) Encapsulate()
    { 
        _kemKey.Encapsulate(out var ciphertext, out var sharedSecret );
        return (ciphertext, sharedSecret);
    }

    public byte[] Decapsulate(byte[] ciphertext)
    {
        return _kemKey.Decapsulate(ciphertext);
    }

    public void PrintArtifactSizes()
    {
        var publicKeyInfo = _kemKey.ExportSubjectPublicKeyInfo();

        var privateKey = _kemKey.ExportPkcs8PrivateKey();

        var (ciphertext, _) = Encapsulate();

        Console.WriteLine($"[Native .NET 10] ML-KEM-768 | " +
                          $"Public Key: {publicKeyInfo.Length} B | " +
                          $"Private Key: {privateKey.Length} B | " +
                          $"Ciphertext: {ciphertext.Length} B");
    }

    public void Dispose()
    {
        _kemKey?.Dispose();
    }
}