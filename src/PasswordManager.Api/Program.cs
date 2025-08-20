using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PasswordManager.Crypto.Implementations;
using PasswordManager.Crypto.Interfaces;
using PasswordManager.Data;
using PasswordManager.Services.Implementations;
using PasswordManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// DB: SQLite file in app folder or configurable via appsettings
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=pwm.db";
builder.Services.AddDbContext<PwmDbContext>(opt => opt.UseSqlite(conn));

// register crypto + services
builder.Services.AddSingleton<IKeyDerivationService, Argon2KeyDerivationService>();
builder.Services.AddSingleton<ICryptoService, AesGcmCryptoService>();

// register data + business services
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
builder.Services.AddScoped<IVaultService, VaultService>();
builder.Services.AddSingleton<ISessionManager, SessionManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PasswordManager",
        Version = "v1"
    });
});

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();

app.MapControllers();

app.Run();
