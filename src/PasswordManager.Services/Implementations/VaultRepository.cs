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

    public async Task<VaultEntity?> GetVaultAsync()
        => await _db.Vaults.FirstOrDefaultAsync();

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

    public async Task<List<VaultItemEntity>> GetItemsAsync()
        => await _db.Items.ToListAsync();
    public async Task<VaultItemEntity?> GetItemByIdAsync(int id)
        => await _db.Items.FindAsync(id);
    public async Task UpdateItemAsync(VaultItemEntity item)
    {
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
    }
}
