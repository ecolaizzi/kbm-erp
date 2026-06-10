using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace KBM.Application.Auth;

public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemoryKb = 65536;
    private const int Parallelism = 2;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = HashInternal(password, salt);
        return $"argon2id${Iterations}${MemoryKb}${Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string encoded)
    {
        var parts = encoded.Split('$');
        if (parts.Length != 6 || parts[0] != "argon2id") return false;
        var salt = Convert.FromBase64String(parts[4]);
        var expected = Convert.FromBase64String(parts[5]);
        var actual = HashInternal(password, salt);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] HashInternal(string password, byte[] salt)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemoryKb,
            Iterations = Iterations
        };
        return argon.GetBytes(HashSize);
    }
}
