using System.ComponentModel.DataAnnotations;

namespace CodeSupp.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Display(Name = "Müşteri Adı")]
        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
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

        [Display(Name = "Oluşturulma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime CreatedAt { get; set; }
        [MaxLength(200)]
        public string? SearchText { get; set; }
        [Required]
        public string TenantId { get; set; } = string.Empty;
        public virtual ICollection<Sale> Sales { get; set; }
        public Customer()
        {
            CreatedAt = DateTime.UtcNow;
            Sales = new HashSet<Sale>(); 
        }
    }
}