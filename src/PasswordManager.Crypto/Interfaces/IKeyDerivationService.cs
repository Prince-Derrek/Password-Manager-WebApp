namespace PasswordManager.Crypto.Interfaces;

public record Argon2Parameters(int MemoryKb = 65536, int Iterations = 2, int Parallelism = 2);
public interface IKeyDerivationService
{
    byte[] DeriveKey(string password, byte[] salt, Argon2Parameters parameters, int keyBytes = 32);
    byte[] CreateSalt(int size = 16);
}
