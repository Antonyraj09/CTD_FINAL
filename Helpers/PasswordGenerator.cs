using System.Security.Cryptography;

namespace CTD_FINAL.Helpers;

/// <summary>Generates a random password satisfying the app's Identity password policy (used for new-user provisioning and admin resets, since no email provider is configured to send reset links).</summary>
public static class PasswordGenerator
{
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Special = "!@#$%&*";

    public static string Generate(int length = 12)
    {
        var all = Lower + Upper + Digits + Special;
        var chars = new char[length];
        chars[0] = Pick(Upper);
        chars[1] = Pick(Lower);
        chars[2] = Pick(Digits);
        chars[3] = Pick(Special);
        for (int i = 4; i < length; i++) chars[i] = Pick(all);

        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];
}
