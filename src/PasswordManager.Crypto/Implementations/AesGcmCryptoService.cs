using System.Text.Json;
using System.Text;
using PasswordManager.Core.Models;
using System.Security.Cryptography;
using PasswordManager.Crypto.Interfaces;

namespace PasswordManager.Crypto.Implementations;

public class AesGcmCryptoService : ICryptoService
{
    public SecureBlob Encrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> aad = default)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key.ToArray());
        aes.Encrypt(nonce, plaintext, ciphertext, tag, aad.ToArray());

        return new SecureBlob { Nonce = nonce, Ciphertext = ciphertext, Tag = tag };
    }
    public byte[] Decrypt(ReadOnlySpan<byte> key, SecureBlob blob, ReadOnlySpan<byte> aad = default)
    {
        var plain = new byte[blob.Ciphertext.Length];
        using var aes = new AesGcm(key.ToArray());
        aes.Decrypt(blob.Nonce, blob.Ciphertext, blob.Tag, plain, aad.ToArray());
        return plain;
    }
    public SecureBlob EncryptString(ReadOnlySpan<byte> key, string plaintext, ReadOnlySpan<byte> aad = default)
        => Encrypt(key, Encoding.UTF8.GetBytes(plaintext), aad);
    public string DecryptToString(ReadOnlySpan<byte> key, SecureBlob blob, ReadOnlySpan<byte> aad = default)
        => Encoding.UTF8.GetString(Decrypt(key, blob, aad));
}
