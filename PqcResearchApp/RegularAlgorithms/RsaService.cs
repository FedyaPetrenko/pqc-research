using System.Security.Cryptography;

namespace PqcResearchApp.RegularAlgorithms
{
    /// <summary>
    /// Lightweight wrapper around the framework's <see cref="RSA"/> implementation that
    /// provides basic RSA operations used in benchmarks and comparisons:
    /// - Key initialization with a configurable key size
    /// - Encryption using OAEP-SHA256
    /// - Signing and verification using SHA256 and PKCS#1 v1.5
    /// - Convenience method to print key/signature sizes
    /// The service implements <see cref="IDisposable"/> and disposes the underlying RSA instance.
    /// </summary>
    public sealed class RsaService : IDisposable
    {
        private readonly RSA _rsa;
        private readonly int _keySize;

        /// <summary>
        /// Creates a new <see cref="RsaService"/> instance and initializes an RSA key pair.
        /// </summary>
        /// <param name="keySize">
        /// Size of the RSA key in bits. Defaults to 4096.
        /// </param>
        public RsaService(int keySize = 4096)
        {
            _keySize = keySize;
            _rsa = RSA.Create(_keySize);
        }

        /// <summary>
        /// Generates a temporary RSA key pair and exports its public parameters.
        /// Note: the exported parameters are not stored by this method — this is a
        /// utility method that demonstrates creation/export of parameters and can be
        /// extended to persist or return keys if needed.
        /// </summary>
        public void GenerateKey()
        {
            using var tempRsa = RSA.Create(_keySize);
            var parameters = tempRsa.ExportParameters(false);
        }

        /// <summary>
        /// Encrypts the provided plaintext using RSA with OAEP padding and SHA-256.
        /// </summary>
        /// <param name="data">Plaintext bytes to encrypt. Must not be null.</param>
        /// <returns>Ciphertext bytes produced by the encryption operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null.</exception>
        /// <exception cref="CryptographicException">If the encryption operation fails.</exception>
        public byte[] Encrypt(byte[] data)
        {
            return _rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        }

        /// <summary>
        /// Signs the specified data using RSA with SHA-256 and PKCS#1 v1.5 signature padding.
        /// </summary>
        /// <param name="data">Data to sign. Must not be null.</param>
        /// <returns>Signature bytes for the provided data.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null.</exception>
        /// <exception cref="CryptographicException">If the signing operation fails.</exception>
        public byte[] Sign(byte[] data)
        {
            return _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Verifies that a signature is valid for the provided data using SHA-256 and PKCS#1 v1.5 padding.
        /// </summary>
        /// <param name="data">The original data that was signed.</param>
        /// <param name="signature">The signature to verify.</param>
        /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="signature"/> is null.</exception>
        /// <exception cref="CryptographicException">If the verification operation fails unexpectedly.</exception>
        public bool Verify(byte[] data, byte[] signature)
        {
            return _rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Prints basic details about the currently loaded RSA keys and a sample signature to the console.
        /// Outputs:
        /// - RSA key size
        /// - Public key DER length (bytes)
        /// - Private key PKCS#8 length (bytes)
        /// - Length of a signature over a 32-byte zeroed buffer
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="Console.WriteLine"/> and is intended for debugging or benchmark output.
        /// </remarks>
        public void PrintDetails()
        {
            var publicKeyInfo = _rsa.ExportSubjectPublicKeyInfo();
            var privateKey = _rsa.ExportPkcs8PrivateKey();
            var sign = Sign(new byte[32]);

            Console.WriteLine($"[Native] RSA-{_keySize} |" +
                              $" Public Key: {publicKeyInfo.Length} B |" +
                              $" Private Key: {privateKey.Length} B |" +
                              $" Signature: {sign.Length} B");
        }

        /// <summary>
        /// Releases resources used by this instance by disposing the underlying <see cref="RSA"/> object.
        /// </summary>
        public void Dispose() => _rsa.Dispose();
    }
}
