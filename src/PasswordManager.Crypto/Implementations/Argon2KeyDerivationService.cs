using Konscious.Security.Cryptography;
using PasswordManager.Crypto.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Crypto.Implementations;

public class Argon2KeyDerivationService : IKeyDerivationService
{
    public byte[] CreateSalt(int size = 16) => RandomNumberGenerator.GetBytes(size);
    public byte[] DeriveKey(string password, byte[] salt, Argon2Parameters parameters, int keyBytes = 32)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parameters.Parallelism,
            Iterations = parameters.Iterations,
            MemorySize = parameters.MemoryKb
        };
        return argon2.GetBytes(keyBytes);
    }
}
