using System.Security.Cryptography;
using CTD_FINAL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CTD_FINAL.Services;

/// <summary>
/// AES-256-GCM (authenticated encryption — confidentiality and tamper-detection in one
/// primitive, no separate HMAC pass needed) for secrets stored in ADMIN_CTD: client
/// database passwords, full connection strings, and the encrypted license payload.
///
/// Key: Encryption:AesKeyBase64, a base64-encoded 32-byte (256-bit) key, sourced the
/// same placeholder+environment-variable-override way as every other secret in this
/// app's appsettings*.json files (see DefaultConnection/AdminConnection) — validated
/// eagerly here so a misconfigured deploy fails loudly at startup instead of silently
/// encrypting with a wrong-length key the first time a request needs it.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:AesKeyBase64"]
            ?? throw new InvalidOperationException("Configuration 'Encryption:AesKeyBase64' not found.");

        try
        {
            _key = Convert.FromBase64String(keyBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Configuration 'Encryption:AesKeyBase64' is not valid base64.", ex);
        }

        if (_key.Length != 32)
            throw new InvalidOperationException($"Configuration 'Encryption:AesKeyBase64' must decode to exactly 32 bytes (256-bit AES key); got {_key.Length}.");
    }

    public string Encrypt(string plaintext)
    {
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: nonce (12) + tag (16) + ciphertext (variable)
        var output = new byte[NonceSizeBytes + TagSizeBytes + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, output, NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, output, NonceSizeBytes + TagSizeBytes, ciphertext.Length);
        return Convert.ToBase64String(output);
    }

    public string Decrypt(string cipherText)
    {
        var input = Convert.FromBase64String(cipherText);
        if (input.Length < NonceSizeBytes + TagSizeBytes)
            throw new CryptographicException("Encrypted value is too short to contain a valid nonce and authentication tag.");

        var nonce = new byte[NonceSizeBytes];
        var tag = new byte[TagSizeBytes];
        var ciphertext = new byte[input.Length - NonceSizeBytes - TagSizeBytes];
        Buffer.BlockCopy(input, 0, nonce, 0, NonceSizeBytes);
        Buffer.BlockCopy(input, NonceSizeBytes, tag, 0, TagSizeBytes);
        Buffer.BlockCopy(input, NonceSizeBytes + TagSizeBytes, ciphertext, 0, ciphertext.Length);

        var plaintextBytes = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);
        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }
}
