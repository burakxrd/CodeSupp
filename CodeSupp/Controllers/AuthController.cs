using CodeSupp.Data;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseModel { IsSuccess = false, Message = "Giriş bilgileri geçersiz." });

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return Conflict(new AuthResponseModel { IsSuccess = false, Message = "Bu e-posta adresi zaten kullanılıyor." });

            var newUser = new AppUser
            {
                Email = model.Email,
                UserName = model.Email,
                TenantId = Guid.NewGuid().ToString(),
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new AuthResponseModel { IsSuccess = false, Message = $"Kullanıcı oluşturulamadı: {errors}" });
            }

            // Token Üret ve COOKIE OLARAK VER
            var (token, expires) = GenerateJwtToken(newUser);
            SetTokenCookie(token, expires);

            return Ok(new
            {
                IsSuccess = true,
                Message = "Kayıt başarılı!",
                User = new { newUser.Email, newUser.FullName, newUser.TenantId }
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseModel { IsSuccess = false, Message = "Giriş bilgileri geçersiz." });

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new AuthResponseModel { IsSuccess = false, Message = "E-posta veya şifre hatalı." });
            }

            // Token Üret ve COOKIE OLARAK VER
            var (token, expires) = GenerateJwtToken(user);
            SetTokenCookie(token, expires);

            return Ok(new
            {
                IsSuccess = true,
                Message = "Giriş başarılı!",
                User = new { user.Email, user.FullName, user.TenantId }
            });
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("X-Access-Token", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/" // Garanti olsun diye path eklendi
            });

            return Ok(new { Message = "Çıkış yapıldı." });
        }

        // GET: api/Auth/me
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // 1. Token içindeki veriyi al (Eski çerezde ID, yenisinde Email olabilir)
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(claimValue)) return Unauthorized();

            AppUser? user = null;

            // 2. AKILLI KONTROL: İçinde '@' varsa Email ile, yoksa ID ile ara
            if (claimValue.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(claimValue);
            }
            else
            {
                // Eski çerez (ID) gelirse burası çalışır ve kurtarır!
                user = await _userManager.FindByIdAsync(claimValue);
            }

            if (user == null) return Unauthorized();

            return Ok(new
            {
                user.Email,
                user.FullName,
                user.TenantId
            });
        }

        // --- YARDIMCI METOTLAR ---

        private void SetTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                Secure = false, 
                SameSite = SameSiteMode.Lax,
                Path = "/", 
                IsEssential = true
            };

            Response.Cookies.Append("X-Access-Token", token, cookieOptions);
        }

        private (string token, DateTime expires) GenerateJwtToken(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                new Claim(ClaimTypes.NameIdentifier, user.Email!),

                new Claim("tenantId", user.TenantId!),
                new Claim("fullName", user.FullName ?? "")
            };

            var expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpiresInHours"] ?? "1"));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return (tokenHandler.WriteToken(token), expires);
        }
    }
}