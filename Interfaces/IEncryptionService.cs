namespace CTD_FINAL.Interfaces;

/// <summary>General-purpose AES encryption for secrets at rest — client database passwords, connection strings, license payloads. Never used for user login passwords (those stay on ASP.NET Core Identity's one-way PBKDF2 hashing via UserManager, which is already correct and is left untouched).</summary>
public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string cipherText);
}
