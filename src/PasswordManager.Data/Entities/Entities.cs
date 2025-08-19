using PasswordManager.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManager.Data.Entities;

public class VaultEntity
{
    [Key]
    public int Id { get; set; }
    public byte[] EncryptedVaultKey { get; set; } = Array.Empty<byte>();
    public byte[] KdfSalt { get; set; } = Array.Empty<byte>();
    public int KdfIterations { get; set; }
    public int KdfParallelism { get; set; }
    public int KdfMemoryKb { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
}
public class VaultItemEntity
{
    [Key]
    public int Id { get; set; }
    public int VaultId { get; set; }
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string UsernameBlob { get; set; } = "";
    public string PasswordBlob { get; set; } = "";
    public string NotesBlob { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? UpdatedAt { get; set; } 

}
