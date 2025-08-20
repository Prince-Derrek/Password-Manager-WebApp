using Microsoft.FeatureManagement;
using PasswordManager.Core.Models;
using PasswordManager.Crypto;
using PasswordManager.Crypto.Interfaces;
using PasswordManager.Data.Entities;
using PasswordManager.Services.Interfaces;
using System.Security.Cryptography;
using System.Text.Json;
using ISessionManager = PasswordManager.Services.Interfaces.ISessionManager;

namespace PasswordManager.Services.Implementations;

public class VaultService : IVaultService
{
    private readonly IVaultRepository _repo;
    private readonly IKeyDerivationService _kdf;
    private readonly ICryptoService _crypto;
    private readonly ISessionManager _sessions;

    public VaultService(IVaultRepository repo, IKeyDerivationService kdf, ICryptoService crypto, ISessionManager sessions)
    {
        _repo = repo;
        _kdf = kdf;
        _crypto = crypto;
        _sessions = sessions;
    }

    public async Task InitializeVaultAsync(string masterPassword)
    {
        var existing = await _repo.GetVaultAsync();
        if (existing != null)
            throw new InvalidOperationException("Vault already exists!");

        var salt = _kdf.CreateSalt(16);
        var kdfParams = new Argon2Parameters(MemoryKb: 65536, Iterations: 2, Parallelism: 2);
        var masterKey = _kdf.DeriveKey(masterPassword, salt, kdfParams, 32);

        var vaultKey = RandomNumberGenerator.GetBytes(32);

        var blob = _crypto.Encrypt(masterKey, vaultKey);

        var vaultEntity = new VaultEntity
        {
            EncryptedVaultKey = CombineBlob(blob),
            KdfSalt = salt,
            KdfIterations = kdfParams.Iterations,
            KdfMemoryKb = kdfParams.MemoryKb,
            KdfParallelism = kdfParams.Parallelism,
            CreatedAt = DateTime.UtcNow.AddHours(3)
        };

        await _repo.CreateVaultAsync(vaultEntity);

        Array.Clear(masterKey);
        Array.Clear(vaultKey);
    }
    private static byte[] CombineBlob(SecureBlob blob)
    {
        return JsonSerializer.SerializeToUtf8Bytes(blob);
    }

    public async Task<string> UnlockAsync(string masterPassword)
    {
        var v = await _repo.GetVaultAsync() ??
            throw new InvalidOperationException("No Vault");

        var salt = v.KdfSalt;
        var kdfParams = new Argon2Parameters(v.KdfMemoryKb, v.KdfIterations, v.KdfParallelism);
        var masterKey = _kdf.DeriveKey(masterPassword, salt, kdfParams, 32);

        var blob = JsonSerializer.Deserialize<SecureBlob>(v.EncryptedVaultKey) ??
            throw new InvalidOperationException("Corrupt Vault Key");
        var vaultKey = _crypto.Decrypt(masterKey, blob);

        var token = _sessions.CreateSession(vaultKey, TimeSpan.FromMinutes(5));

        Array.Clear(masterKey);
        return token;
    }
    public Task LockAsync(string sessionToken)
    {
        _sessions.RemoveSession(sessionToken);
        return Task.CompletedTask;
    }
    public async Task<List<VaultItemSummaryDto>> ListItemsAsync(string sessionToken)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? throw new UnauthorizedAccessException();
        var items = await _repo.GetItemsAsync();
        return items.Select(i => new VaultItemSummaryDto(i.Id, i.Title, i.Url)).ToList();
    }

    public async Task<VaultItemDetailsDto> GetItemAsync(string sessionToken, int id)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? throw new UnauthorizedAccessException();
        var ent = await _repo.GetItemByIdAsync(id) ?? throw new KeyNotFoundException();
        var usernameBlob = JsonSerializer.Deserialize<SecureBlob>(ent.UsernameBlob) ?? new SecureBlob();
        var passwordBlob = JsonSerializer.Deserialize<SecureBlob>(ent.PasswordBlob) ?? new SecureBlob();

        var username = _crypto.DecryptToString(vaultKey, usernameBlob);
        var password = _crypto.DecryptToString(vaultKey, passwordBlob);

        return new VaultItemDetailsDto(ent.Id, ent.Title, ent.Url, username, password);
    }

    public async Task<int> CreateItemAsync(string sessionToken, CreateVaultItemDto dto)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? throw new UnauthorizedAccessException();

        var usernameBlob = _crypto.EncryptString(vaultKey, dto.Username ?? "");
        var passwordBlob = _crypto.EncryptString(vaultKey, dto.Password ?? "");

        var ent = new VaultItemEntity
        {
            Title = dto.Title,
            Url = dto.Url ?? "",
            UsernameBlob = JsonSerializer.Serialize(usernameBlob),
            PasswordBlob = JsonSerializer.Serialize(passwordBlob),
            NotesBlob = ""
        };

        await _repo.AddItemAsync(ent);
        return ent.Id;
    }
    public async Task<List<VaultSummaryDto>> ListVaultsAsync(string sessionToken)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? throw new UnauthorizedAccessException();
        var vaults = await _repo.GetAllVaultsAsync();
        return vaults.Select(v => new VaultSummaryDto(v.Id, v.CreatedAt)).ToList();
    }

    public async Task DeleteVaultAsync(string sessionToken, int vaultId)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? throw new UnauthorizedAccessException();
        var vault = await _repo.GetVaultByIdAsync(vaultId);
        if (vault == null)
            throw new KeyNotFoundException("Vault not found");

        await _repo.DeleteVaultAsync(vaultId);
    }
}
