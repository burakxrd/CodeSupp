// Dosya: Models/SaleItem.cs (TenantId Eklenmiş Hali)

using CodeSupp.Data; // AppUser (Owner) için
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSupp.Models
{
    public class SaleItem
    {
        public int Id { get; set; }

        // Hangi Satışa ait? (Ana ilişki)
        public int SaleId { get; set; }
        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; } = null!;

        // Hangi Ürün satıldı? (İlişki)
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Display(Name = "Miktar (Adet)")]
        public int Quantity { get; set; }

        [Display(Name = "Birim Satış Fiyatı (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } // Satış anındaki fiyatı kaydederiz

        [Display(Name = "Toplam Satır Tutarı (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        // --- OMURGA: Multi-Tenant İlişkisi ---
        [Required(ErrorMessage = "Satış kaleminin bir sahibi (TenantId) olmalıdır.")]
        public string TenantId { get; set; } = null!;
        // ------------------------------------

        public SaleItem()
        {
            TotalPrice = Quantity * UnitPrice;
        }
    }
}