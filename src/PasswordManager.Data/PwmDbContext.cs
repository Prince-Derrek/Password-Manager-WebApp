using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PasswordManager.Core.Models;
using PasswordManager.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Design;
using SQLitePCL; 

namespace PasswordManager.Data
{
    public class PwmDbContextFactory : IDesignTimeDbContextFactory<PwmDbContext>
    {
        public PwmDbContext CreateDbContext(string[] args)
        {
            Batteries.Init();

            var optionsBuilder = new DbContextOptionsBuilder<PwmDbContext>();

            optionsBuilder.UseSqlite("Data Source=app.db");

            return new PwmDbContext(optionsBuilder.Options);
        }
    }

    public class PwmDbContext : DbContext
    {
        public DbSet<VaultEntity> Vaults { get; set; }
        public DbSet<VaultItemEntity> Items { get; set; }

        public PwmDbContext(DbContextOptions<PwmDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var secureBlobConverter = new ValueConverter<SecureBlob, string>(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }),
                s => DeserializeSecureBlob(s)
            );

            modelBuilder.Entity<VaultItemEntity>(b =>
            {
                b.Property(p => p.UsernameBlob).HasColumnType("TEXT");
                b.Property(p => p.PasswordBlob).HasColumnType("TEXT");
                b.Property(p => p.NotesBlob).HasColumnType("TEXT");
            });

            base.OnModelCreating(modelBuilder);
        }

        private static SecureBlob DeserializeSecureBlob(string json)
        {
            return JsonSerializer.Deserialize<SecureBlob>(json, new JsonSerializerOptions()) ?? new SecureBlob();
        }
    }
}
