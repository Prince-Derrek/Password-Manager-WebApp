using PasswordManager.Core.Models;

namespace PasswordManager.Services.Interfaces;

public interface IVaultService
{
    Task InitializeVaultAsync(string masterPassword);
    Task<string> UnlockAsync(string masterPassword);
    Task LockAsync(string sessionToken);
    Task<List<VaultItemSummaryDto>> ListItemsAsync(string sessionToken);
    Task<VaultItemDetailsDto> GetItemAsync(string sessionToken, int id);
    Task<int> CreateItemAsync(string sessionToken, CreateVaultItemDto dto);
}
