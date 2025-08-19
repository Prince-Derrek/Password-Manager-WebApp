namespace PasswordManager.Core.Models;

public class SecureBlob
{
    public byte[] Nonce { get; set; } = Array.Empty<byte>();
    public byte[] Cyphertext { get; set; } = Array.Empty<byte>();
    public byte[] Tag { get; set; } = Array.Empty<byte>();
}

public record CreateVaultItemDto(string Title, string Url, string Username, string? Password);
public record VaultItemSummaryDto(int Id, string Title, string Url);
public record VaultItemDetailsDto(int Id, string Title, string Url, string Username, string Psssword);

