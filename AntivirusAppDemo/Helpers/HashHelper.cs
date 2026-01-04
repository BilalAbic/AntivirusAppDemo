using System.Security.Cryptography;

namespace AntivirusAppDemo.Helpers;

public static class HashHelper
{
    public static string CalculateSHA256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
