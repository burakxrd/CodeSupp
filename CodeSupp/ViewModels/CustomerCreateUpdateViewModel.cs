using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    public class CustomerCreateUpdateViewModel
    {
        [Display(Name = "Müşteri Adı")]
        [MaxLength(100)]
        public string? Name { get; set; } // Zorunluluk kalktı

        [Display(Name = "E-posta")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Instagram Adı")]
        [MaxLength(100)]
        public string? InstagramHandle { get; set; }

        [Display(Name = "Adres")]
        [MaxLength(1000)]
        public string? Address { get; set; }
    }
}