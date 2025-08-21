using Microsoft.FeatureManagement;
using PasswordManager.Core.Models;
using PasswordManager.Crypto;
using PasswordManager.Crypto.Interfaces;
using PasswordManager.Data.Entities;
using PasswordManager.Services.Interfaces;
using System.Security.Cryptography;
using System.Text.Json;
using ISessionManager = PasswordManager.Services.Interfaces.ISessionManager;
using Microsoft.Extensions.Logging;

namespace PasswordManager.Services.Implementations;

public class VaultService : IVaultService
{
    private readonly IVaultRepository _repo;
    private readonly IKeyDerivationService _kdf;
    private readonly ICryptoService _crypto;
    private readonly ISessionManager _sessions;
    private readonly IPasswordGenerator _generator;
    private readonly ILogger<VaultService> _log;


    public VaultService(IVaultRepository repo, IKeyDerivationService kdf, ICryptoService crypto, ISessionManager sessions, IPasswordGenerator generator, ILogger<VaultService> log)
    {
        _repo = repo;
        _kdf = kdf;
        _crypto = crypto;
        _sessions = sessions;
        _generator = generator;
        _log = log;
    }

    public async Task InitializeVaultAsync(string masterPassword, string vaultName)
    {
        _log.LogDebug($"Checking if vault: {vaultName} exists");
        var existing = await _repo.GetVaultAsync(vaultName);
        if (existing != null)
        {
            _log.LogError($"Vault: {vaultName} already exists! Choose another name");
            return;
        }

        var salt = _kdf.CreateSalt(16);
        var kdfParams = new Argon2Parameters(MemoryKb: 65536, Iterations: 2, Parallelism: 2);
        var masterKey = _kdf.DeriveKey(masterPassword, salt, kdfParams, 32);

        var vaultKey = RandomNumberGenerator.GetBytes(32);

        var blob = _crypto.Encrypt(masterKey, vaultKey);

        _log.LogInformation($"Creating new vault: {vaultName}");
        var vaultEntity = new VaultEntity
        {
            EncryptedVaultKey = CombineBlob(blob),
            VaultName = vaultName,
            KdfSalt = salt,
            KdfIterations = kdfParams.Iterations,
            KdfMemoryKb = kdfParams.MemoryKb,
            KdfParallelism = kdfParams.Parallelism,
            CreatedAt = DateTime.UtcNow.AddHours(3)
        };

        await _repo.CreateVaultAsync(vaultEntity);

        _log.LogInformation("Vault created");
        Array.Clear(masterKey);
        Array.Clear(vaultKey);
    }
    private static byte[] CombineBlob(SecureBlob blob)
    {
        return JsonSerializer.SerializeToUtf8Bytes(blob);
    }

    public async Task<string> UnlockAsync(string masterPassword, string vaultName)
    {
        var v = await _repo.GetVaultAsync(vaultName);
        if(v == null)
        {
            _log.LogError($"Vault: {vaultName} doesn't exist");
            return null;
        }

        var salt = v.KdfSalt;
        var kdfParams = new Argon2Parameters(v.KdfMemoryKb, v.KdfIterations, v.KdfParallelism);
        var masterKey = _kdf.DeriveKey(masterPassword, salt, kdfParams, 32);

        var blob = JsonSerializer.Deserialize<SecureBlob>(v.EncryptedVaultKey);
        if (blob == null)
        {
            _log.LogError($"Vault Key: {v.EncryptedVaultKey} has been corrupted");
            return null;
        }
        var vaultKey = _crypto.Decrypt(masterKey, blob) ?? null;
        if (vaultKey == null)
        {
            _log.LogError("Invalid Master Password entered!");
            return null;
        }

        _log.LogDebug("Creating Session token");
        var token = _sessions.CreateSession(vaultKey, TimeSpan.FromMinutes(5), v.Id);
        _log.LogDebug("Session Token Created");

        _log.LogInformation($"Vault: {vaultName} unlocked successfully");
        Array.Clear(masterKey);
        return token;
    }
    public Task LockAsync(string sessionToken)
    {
        _sessions.RemoveSession(sessionToken);
        _log.LogDebug("Session destroyed and Vault Locked Successfully");
        return Task.CompletedTask;
    }
    public async Task<List<VaultItemSummaryDto>> ListItemsAsync(string sessionToken)
    {
        var vaultKey = _sessions.GetKey(sessionToken);
        if (vaultKey == null)
        {
            _log.LogError($"Vault Key invalid! Unauthorized for access");
            return new List<VaultItemSummaryDto>
            {
                new VaultItemSummaryDto(0, string.Empty, string.Empty)
            }; 
        }
        var vaultId = _sessions.GetVaultId(sessionToken) ?? throw new UnauthorizedAccessException();
        var items = await _repo.GetItemsAsync(vaultId);
        _log.LogInformation($"Items from vault with id {vaultId} retrieved successfully");
        return items.Select(i => new VaultItemSummaryDto(i.Id, i.Title, i.Url)).ToList();
    }

    public async Task<VaultItemDetailsDto> GetItemAsync(string sessionToken, int id)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? null;
        if (vaultKey == null)
        {
            _log.LogError($"Vault Key invalid! Unauthorized for access");
            return new VaultItemDetailsDto(0, string.Empty, string.Empty, string.Empty, string.Empty);
        }
        var vaultId = _sessions.GetVaultId(sessionToken) ?? -1;
        if(vaultId == -1)
        {
            _log.LogError($"Vault Id invalid");
            return new VaultItemDetailsDto(0, string.Empty, string.Empty, string.Empty, string.Empty);
        }
        var ent = await _repo.GetItemByIdAsync(id, vaultId) ?? null;
        if (ent == null)
        {
            _log.LogError("Trying to access items in another vault. Please unlock the correct vault");
            return null;
        }
        var usernameBlob = JsonSerializer.Deserialize<SecureBlob>(ent.UsernameBlob) ?? new SecureBlob();
        var passwordBlob = JsonSerializer.Deserialize<SecureBlob>(ent.PasswordBlob) ?? new SecureBlob();

        var username = _crypto.DecryptToString(vaultKey, usernameBlob);
        var password = _crypto.DecryptToString(vaultKey, passwordBlob);

        _log.LogInformation($"Item with id: {id} retrieved successfully from vault with id: {vaultId}");
        return new VaultItemDetailsDto(ent.Id, ent.Title, ent.Url, username, password);
    }

    public async Task<int> CreateItemAsync(string sessionToken, CreateVaultItemDto dto)
    {
        var vaultKey = _sessions.GetKey(sessionToken);
        if (vaultKey == null)
        {
            _log.LogError($"Vault Key invalid! Unauthorized for access");
            return -1;
        }
        var vaultId = _sessions.GetVaultId(sessionToken) ?? -1;
        if(vaultId == -1)
        {
            _log.LogError("Vault Id invalid. Unauthorised for access");
            return -1;
        }

        var passwordToUse = string.IsNullOrEmpty(dto.Password)
            ?_generator.GenerateSecurePassword(16) // default 16 chars
            : dto.Password;

        var usernameBlob = _crypto.EncryptString(vaultKey, dto.Username ?? "");
        var passwordBlob = _crypto.EncryptString(vaultKey, passwordToUse);

        _log.LogDebug($"Creating new Vault item username: {dto.Username} in Vault with id: {vaultId}");
        var ent = new VaultItemEntity
        {
            VaultId = vaultId,
            Title = dto.Title,
            Url = dto.Url ?? "",
            UsernameBlob = JsonSerializer.Serialize(usernameBlob),
            PasswordBlob = JsonSerializer.Serialize(passwordBlob),
            NotesBlob = ""
        };

        await _repo.AddItemAsync(ent);
        _log.LogInformation($"Item with username: {dto.Username} created successfully");
        return ent.Id;
    }
    public async Task<List<VaultSummaryDto>> ListVaultsAsync(string sessionToken)
    {
        var vaultKey = _sessions.GetKey(sessionToken);
        if (vaultKey == null)
        {
            _log.LogError($"Vault Key invalid! Unauthorized for access");
            return new List<VaultSummaryDto>
            {
                new VaultSummaryDto(0, string.Empty, DateTime.MinValue)
            };
        }
        var vaults = await _repo.GetAllVaultsAsync();
        _log.LogInformation("All vaults retrieved successfully");
        return vaults.Select(v => new VaultSummaryDto(v.Id, v.VaultName, v.CreatedAt)).ToList();
    }

    public async Task DeleteVaultAsync(string sessionToken, int vaultId)
    {
        var vaultKey = _sessions.GetKey(sessionToken) ?? null;
        if (vaultKey == null)
        {
            _log.LogError("Invalid Vault Key");
            return;
        }
        var vault = await _repo.GetVaultByIdAsync(vaultId);
        if (vault == null)
        {
            _log.LogError("Vault not found");
            return;
        }

        _log.LogInformation($"Vault with id: {vaultId} deleted successfully");
        await _repo.DeleteVaultAsync(vaultId);
    }
}
