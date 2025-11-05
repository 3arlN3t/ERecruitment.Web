using System.Security.Cryptography;

namespace ERecruitment.Web.Utilities;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);

        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var hash = deriveBytes.GetBytes(32);

        return Convert.ToBase64String(salt.Concat(hash).ToArray());
    }

    public static bool VerifyPassword(string password, string stored)
    {
        var data = Convert.FromBase64String(stored);
        var salt = data.Take(16).ToArray();
        var storedHash = data.Skip(16).ToArray();

        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var computedHash = deriveBytes.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
