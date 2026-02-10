using CodeSupp.Models;
using CodeSupp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Claims;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // --- YARDIMCI: Token'dan Kimlik Bilgisini Güvenli Çek ---
        private string? GetUserIdentifierFromToken()
        {
            // Token içindeki olası tüm kimlik alanlarını sırayla dener, ilk bulduğunu alır.
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?? User.FindFirstValue("id")
                ?? User.FindFirstValue(ClaimTypes.Email);
        }

        // --- CORE: Akıllı Kullanıcı Bulma (Smart Lookup) ---
        // Bu metod, token'da ID de yazsa, Email de yazsa kullanıcıyı doğru şekilde bulur.
        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var identifier = GetUserIdentifierFromToken();
            if (string.IsNullOrEmpty(identifier)) return null;

            // 1. Önce ID (GUID) olduğunu varsayarak arayalım (En hızlı yöntem)
            var user = await _userManager.FindByIdAsync(identifier);

            // 2. Eğer bulamadıysa ve değer bir Email formatındaysa, Email ile arayalım
            // (Senin token yapındaki sorunu çözen kritik blok burasıdır)
            if (user == null && identifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(identifier);
            }

            return user;
        }

        // --- DASHBOARD AYARLARINI GETİR ---
        [HttpGet("dashboard-settings")]
        public async Task<IActionResult> GetDashboardSettings()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var settings = string.IsNullOrEmpty(user.DashboardSettings)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(user.DashboardSettings);

            return Ok(settings);
        }

        // --- DASHBOARD AYARLARINI KAYDET ---
        [HttpPost("dashboard-settings")]
        public async Task<IActionResult> SaveDashboardSettings([FromBody] List<string> settings)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            user.DashboardSettings = JsonSerializer.Serialize(settings);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Dashboard düzeni kaydedildi." });
            }

            return BadRequest(new { Message = "Ayarlar kaydedilirken bir hata oluştu." });
        }
    }
}