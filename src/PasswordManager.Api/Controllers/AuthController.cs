using Microsoft.AspNetCore.Mvc;
using PasswordManager.Services.Interfaces;
using static PasswordManager.Api.DTOs;

namespace PasswordManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IVaultService _vault;
        private readonly ILogger<AuthController> _log;
        public AuthController(IVaultService vault, ILogger<AuthController> log)
        {
            _vault = vault;
            _log = log;
        }

        [HttpPost("init")]
        public async Task<IActionResult> Init([FromBody] InitRequest req)
        {
            _log.LogInformation("Initializing a new vault");
            await _vault.InitializeVaultAsync(req.MasterPassword, req.vaultName);
            return Ok();
        }

        [HttpPost("unlock")]
        public async Task<IActionResult> Unlock([FromBody] UnlockRequest req)
        {
            _log.LogInformation($"Unlocking Vault: {req.vaultName}");
            var token = await _vault.UnlockAsync(req.MasterPassword, req.vaultName);
            return Ok(new { SessionToken = token });
        }

        [HttpPost("lock")]
        public async Task<IActionResult> Lock([FromBody] LockRequest req)
        {
            _log.LogInformation($"Locking vault");
            await _vault.LockAsync(req.SessionToken);
            return Ok();
        }
    }
}
