using PasswordManager.Core.Models;

namespace PasswordManager.Crypto.Interfaces;

public interface ICryptoService
{
    SecureBlob Encrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> aad = default);
    byte[] Decrypt(ReadOnlySpan<byte> key, SecureBlob blob, ReadOnlySpan<byte> aad = default);
    SecureBlob EncryptString(ReadOnlySpan<byte> key, string plaintext, ReadOnlySpan<byte> aad = default);
    string DecryptToString(ReadOnlySpan<byte> key, SecureBlob blob, ReadOnlySpan<byte> aad = default);
}
