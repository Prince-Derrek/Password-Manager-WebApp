using PasswordManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Services.Interfaces;
using PasswordManager.Data;
using Microsoft.Extensions.Logging;

namespace PasswordManager.Services.Implementations;

public class VaultRepository : IVaultRepository
{
    private readonly PwmDbContext _db;
    private readonly ILogger<VaultRepository> _log;
    public VaultRepository(PwmDbContext db, ILogger<VaultRepository> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<VaultEntity?> GetVaultAsync(string vaultName)
    {
        _log.LogDebug($"Fetching vault: {vaultName} from Db");
        return await _db.Vaults.FirstOrDefaultAsync(v => v.VaultName == vaultName);
    }

    public async Task CreateVaultAsync(VaultEntity vault)
    {
        _log.LogDebug($"Creating new vault: {vault}");
        _db.Vaults.Add(vault);
        await _db.SaveChangesAsync();
    }

    public async Task AddItemAsync(VaultItemEntity item)
    {
        _log.LogDebug($"Creating new vault item: {item}");
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task<List<VaultItemEntity>> GetItemsAsync(int vaultId)
    {
        _log.LogDebug($"Getting vault items from Vault with Id: {vaultId}");
       return await _db.Items
            .Where(i => i.VaultId == vaultId)
            .ToListAsync();
    }
    public async Task<VaultItemEntity?> GetItemByIdAsync(int id, int vaultId)
    {
        _log.LogDebug($"Getting item with Id: {id} from vault: {vaultId}");
        return await _db.Items
            .FirstOrDefaultAsync(i => i.Id == id && i.VaultId == vaultId);
    }
    public async Task UpdateItemAsync(VaultItemEntity item)
    {
        _log.LogDebug($"Updating Item: {item}");
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
    }
    public async Task<List<VaultEntity>> GetAllVaultsAsync()
    {
        _log.LogDebug("Fetching all vaults from the db");
        return await _db.Vaults.ToListAsync();
    }

    public async Task<VaultEntity?> GetVaultByIdAsync(int id)
    {
        _log.LogDebug($"Fetching Vault with Id: {id}");
        return await _db.Vaults.FindAsync(id);
    }

    public async Task DeleteVaultAsync(int id)
    {
        _log.LogDebug($"Deleting Vault with Id: {id}");
        var vault = await _db.Vaults.FindAsync(id);
        if (vault != null)
        { 
            _db.Vaults.Remove(vault);
            await _db.SaveChangesAsync();
        }
        else
        {
            _log.LogError($"Vault Id: {id} doesn't exist in the db");
            return;
        }
    }
}
