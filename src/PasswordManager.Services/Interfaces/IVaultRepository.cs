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
    Task<List<VaultEntity>> GetAllVaultsAsync(); 
    Task<VaultEntity?> GetVaultByIdAsync(int id); 
    Task DeleteVaultAsync(int id);
}
