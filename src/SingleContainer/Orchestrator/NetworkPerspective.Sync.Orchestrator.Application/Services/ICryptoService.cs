using System;
using System.Security.Cryptography;
using System.Text;

using Konscious.Security.Cryptography;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface ICryptoService
{
    byte[] GenerateSalt();
    byte[] HashPassword(string password, byte[] salt);
    bool VerifyPassword(string password, string base64Hash, string base64Salt);
}

internal class CryptoService : ICryptoService
{
    public byte[] GenerateSalt()
    {
        byte[] salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    public byte[] HashPassword(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = 4;
        argon2.MemorySize = 65536;
        argon2.Iterations = 4;

        return argon2.GetBytes(32);
    }

    public bool VerifyPassword(string password, string base64Hash, string base64Salt)
    {
        byte[] hash = Convert.FromBase64String(base64Hash);
        byte[] salt = Convert.FromBase64String(base64Salt);

        byte[] testHash = HashPassword(password, salt);
        return SlowEquals(hash, testHash);
    }

    private static bool SlowEquals(byte[] a, byte[] b)
    {
        uint diff = (uint)a.Length ^ (uint)b.Length;

        for (int i = 0; i < a.Length && i < b.Length; i++)
            diff |= (uint)(a[i] ^ b[i]);

        return diff == 0;
    }
}