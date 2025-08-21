using PasswordManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Services.Interfaces;
using PasswordManager.Data;

namespace PasswordManager.Services.Implementations;

public class VaultRepository : IVaultRepository
{
    private readonly PwmDbContext _db;
    public VaultRepository(PwmDbContext db)
    {
        _db = db;
    }

    public async Task<VaultEntity?> GetVaultAsync(string vaultName)
    {
        return await _db.Vaults.FirstOrDefaultAsync(v => v.VaultName == vaultName);
    }

    public async Task CreateVaultAsync(VaultEntity vault)
    {
        _db.Vaults.Add(vault);
        await _db.SaveChangesAsync();
    }

    public async Task AddItemAsync(VaultItemEntity item)
    {
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task<List<VaultItemEntity>> GetItemsAsync(int vaultId)
    {
       return await _db.Items
            .Where(i => i.VaultId == vaultId)
            .ToListAsync();
    }
    public async Task<VaultItemEntity?> GetItemByIdAsync(int id, int vaultId)
    {
        return await _db.Items
            .FirstOrDefaultAsync(i => i.Id == id && i.VaultId == vaultId);
    }
    public async Task UpdateItemAsync(VaultItemEntity item)
    {
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
    }
    public async Task<List<VaultEntity>> GetAllVaultsAsync()
    => await _db.Vaults.ToListAsync();

    public async Task<VaultEntity?> GetVaultByIdAsync(int id)
        => await _db.Vaults.FindAsync(id);

    public async Task DeleteVaultAsync(int id)
    {
        var vault = await _db.Vaults.FindAsync(id);
        if (vault != null)
        {
            _db.Vaults.Remove(vault);
            await _db.SaveChangesAsync();
        }
    }
}
