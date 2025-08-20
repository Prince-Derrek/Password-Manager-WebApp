using PasswordManager.Data.Entities;

namespace PasswordManager.Services.Interfaces;

public interface IVaultRepository
{
    Task<VaultEntity?> GetVaultAsync();
    Task CreateVaultAsync(VaultEntity vault);
    Task AddItemAsync(VaultItemEntity item);
    Task<List<VaultItemEntity>> GetItemsAsync();
    Task<VaultItemEntity?> GetItemByIdAsync(int id);
    Task UpdateItemAsync(VaultItemEntity item);
}
