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
        public AuthController(IVaultService vault) { _vault = vault; }

        [HttpPost("init")]
        public async Task<IActionResult> Init([FromBody] InitRequest req)
        {
            await _vault.InitializeVaultAsync(req.MasterPassword, req.vaultName);
            return Ok();
        }

        [HttpPost("unlock")]
        public async Task<IActionResult> Unlock([FromBody] UnlockRequest req)
        {
            var token = await _vault.UnlockAsync(req.MasterPassword, req.vaultName);
            return Ok(new { SessionToken = token });
        }

        [HttpPost("lock")]
        public async Task<IActionResult> Lock([FromBody] LockRequest req)
        {
            await _vault.LockAsync(req.SessionToken);
            return Ok();
        }
    }
}
