using System.ComponentModel.DataAnnotations;
using CodeSupp.Enums; // Enum dosyamızı tanıttık

namespace CodeSupp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; } = null!;
        public CategoryType Type { get; set; } = CategoryType.Standard;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string TenantId { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}