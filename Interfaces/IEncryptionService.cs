namespace CTD_FINAL.Interfaces;

/// <summary>General-purpose AES encryption for secrets at rest — client database passwords, connection strings, license payloads, and (via ReversiblePasswordHasher, an explicit security trade-off) end-user Identity login passwords.</summary>
public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string cipherText);
}
