using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore;
using PasswordManager.Crypto.Implementations;
using PasswordManager.Crypto.Interfaces;
using PasswordManager.Data;
using PasswordManager.Services.Implementations;
using PasswordManager.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// DB: SQLite file in app folder or configurable via appsettings
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";
builder.Services.AddDbContext<PwmDbContext>(opt => opt.UseSqlite(conn));

// register crypto + services
builder.Services.AddSingleton<IKeyDerivationService, Argon2KeyDerivationService>();
builder.Services.AddSingleton<ICryptoService, AesGcmCryptoService>();

// register data + business services
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
builder.Services.AddScoped<IVaultService, VaultService>();
builder.Services.AddSingleton<ISessionManager, SessionManager>();
builder.Services.AddScoped<IPasswordGenerator, PasswordGenerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Password Manager",
        Version = "v1"
    });

    c.AddSecurityDefinition("Session", new OpenApiSecurityScheme
    {
        Description = "Session token returned from Unlock. Example: abc123xyz...",
        Name = "Session-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Session"
            }
        },
        Array.Empty<string>()
    }
});

});

var app = builder.Build();
app.UseSwagger(); 
app.UseSwaggerUI( c=>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Password Manager v1");
});

app.MapControllers();

app.Run();
