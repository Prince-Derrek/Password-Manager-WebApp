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

        public VaultController(IVaultService vault) => _vault = vault;

        private string GetSessionToken() => Request.Headers["X-Session-Token"].FirstOrDefault() ?? "";

        [HttpGet("items")]
        public async Task<IActionResult> ListItems()
        {
            var token = GetSessionToken();
            var list = await _vault.ListItemsAsync(token);
            return Ok(list);
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var token = GetSessionToken();
            var item = await _vault.GetItemAsync(token, id);
            return Ok(item);
        }

        [HttpPost("items")]
        public async Task<IActionResult> Create([FromBody] CreateVaultItemDto dto)
        {
            var token = GetSessionToken();
            var id = await _vault.CreateItemAsync(token, dto);
            return CreatedAtAction(nameof(GetItem), new { id }, new { id });
        }
    }
}
