using CodeSupp.Data;
using CodeSupp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/settings
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            // Global Query Filter devrede olduğu için sadece Login olan Tenant'ın ayarı gelir.
            // FirstOrDefaultAsync, eğer tenant'ın hiç ayarı yoksa null döner.
            var settings = await _context.StoreSettings.FirstOrDefaultAsync();

            // Eğer ayar hiç yoksa (ilk kez giriliyorsa) varsayılan oluştur.
            if (settings == null)
            {
                settings = new StoreSetting
                {
                    // TenantId DbContext tarafından otomatik atanacak!
                    ShowShippingColumn = false,
                    EnableVAT = true,
                    DefaultVAT = 20
                };
                _context.StoreSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(settings);
        }

        // POST: api/settings
        [HttpPost]
        public async Task<IActionResult> UpdateSettings([FromBody] StoreSetting model)
        {
            // Global Filter devrede
            var settings = await _context.StoreSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                // Lazy Create (Nadir durum ama handle etmek iyidir)
                settings = new StoreSetting
                {
                    // TenantId otomatik
                    ShowShippingColumn = model.ShowShippingColumn,
                    EnableVAT = model.EnableVAT,
                    DefaultVAT = model.DefaultVAT
                };
                _context.StoreSettings.Add(settings);
            }
            else
            {
                // Mevcut ayarları güncelle
                settings.ShowShippingColumn = model.ShowShippingColumn;
                settings.EnableVAT = model.EnableVAT;
                settings.DefaultVAT = model.DefaultVAT;

                // EF Core tracking mekanizması sayesinde Update dememize gerek yok ama explicit olmak iyidir.
                // _context.Update(settings); 
            }

            await _context.SaveChangesAsync();
            return Ok(settings);
        }
    }
}