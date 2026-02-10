using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    // Kayıt olma (Register) işlemi için tarayıcıdan gelecek JSON model
    public class RegisterModel
    {
        // --- EKLENMESİ GEREKEN KISIM ---
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur")]
        public string FullName { get; set; } = string.Empty;
        // -------------------------------

        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Password { get; set; } = string.Empty;
    }

    // Giriş yapma (Login) işlemi için tarayıcıdan gelecek JSON model
    public class LoginModel
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        public string Password { get; set; } = string.Empty;
    }

    // Giriş veya kayıt başarılı olduğunda tarayıcıya geri göndereceğimiz cevap
    public class AuthResponseModel
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpires { get; set; }
    }
}