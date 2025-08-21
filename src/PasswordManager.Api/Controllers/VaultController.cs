using Microsoft.AspNetCore.Mvc;
using PasswordManager.Core.Models;
using PasswordManager.Services.Interfaces;

namespace PasswordManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaultController : ControllerBase
    {
        private readonly IVaultService _vault;
        private readonly ILogger<VaultController> _log;

        public VaultController(IVaultService vault, ILogger<VaultController> log)
        {
            _vault = vault;
            _log = log;
        } 
        private string GetSessionToken() =>
            Request.Headers["Session-Token"].FirstOrDefault() ?? "";

        [HttpGet("items")]
        public async Task<IActionResult> ListItems()
        {
            _log.LogInformation("Retrieving Items in vault");
            var token = GetSessionToken();
            var list = await _vault.ListItemsAsync(token);
            return Ok(list);
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            _log.LogInformation($"Getting Item with id:{id} from vault");
            var token = GetSessionToken();
            var item = await _vault.GetItemAsync(token, id);
            return Ok(item);
        }

        [HttpPost("items")]
        public async Task<IActionResult> Create([FromBody] CreateVaultItemDto dto)
        {
            _log.LogInformation("Creating new item in the vault");
            var token = GetSessionToken();
            var id = await _vault.CreateItemAsync(token, dto);
            return CreatedAtAction(nameof(GetItem), new { id }, new { id });
        }
        [HttpGet]
        public async Task<IActionResult> ListVaults()
        {
            _log.LogInformation("Listing all available vaults");
            var token = GetSessionToken();
            var vaults = await _vault.ListVaultsAsync(token);
            return Ok(vaults);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVault(int id)
        {
            _log.LogWarning($"Deleting Vault with id: {id}");
            var token = GetSessionToken();
            await _vault.DeleteVaultAsync(token, id);
            return NoContent();
        }
    }
}
