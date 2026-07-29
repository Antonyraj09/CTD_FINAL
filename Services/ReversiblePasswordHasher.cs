using System.Security.Cryptography;
using System.Text;
using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CTD_FINAL.Services;

/// <summary>
/// Stores every user's login password as reversible AES-256-GCM ciphertext (via
/// IEncryptionService, the same key already used for ADMIN_CTD's stored credentials)
/// instead of ASP.NET Core Identity's default one-way PBKDF2 hash.
///
/// Explicitly requested, against the default recommendation: this is a real security
/// downgrade for every end-user account in every tenant, not just infrastructure
/// credentials — anyone with read access to a tenant database (or a copy of
/// Encryption:AesKeyBase64) can recover every user's actual password, and unlike a
/// one-way hash, a stolen copy of this table is immediately useful to an attacker
/// with no cracking effort at all.
///
/// Accounts created before this change carry a PBKDF2 hash on file, not AES ciphertext.
/// Decrypting that as if it were ciphertext throws, so VerifyHashedPassword catches the
/// failure and reports Failed rather than crashing login — those accounts need a
/// password reset (Users &amp; Roles → Reset Password) to get a decryptable value stored.
/// </summary>
public class ReversiblePasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly IEncryptionService _encryptionService;

    public ReversiblePasswordHasher(IEncryptionService encryptionService) => _encryptionService = encryptionService;

    public string HashPassword(ApplicationUser user, string password) => _encryptionService.Encrypt(password);

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        string decrypted;
        try
        {
            decrypted = _encryptionService.Decrypt(hashedPassword);
        }
        catch (Exception)
        {
            // Not AES ciphertext produced by this hasher — almost always a pre-existing
            // PBKDF2 hash from before this change took effect.
            return PasswordVerificationResult.Failed;
        }

        var match = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(decrypted),
            Encoding.UTF8.GetBytes(providedPassword));
        return match ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}
