using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSupp.Models
{
    public class ProductPurchaseHistory
    {
        public int Id { get; set; }

        // --- İlişkili Ürün ---
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Display(Name = "Alış Tarihi")]
        public DateTime PurchaseDate { get; set; }

        [Display(Name = "Alınan Miktar (Adet)")]
        public int Quantity { get; set; }

        [Display(Name = "Ürün Birim Alış Fiyatı (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProductPricePerUnit { get; set; }

        [Display(Name = "Toplam Ağırlık (Kg)")]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TotalKg { get; set; }

        [Display(Name = "Kargo Ücreti (Kg Başına, TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingCostPerKg { get; set; }

        [Display(Name = "Toplam Ürün Maliyeti (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProductCost { get; set; }

        [Display(Name = "Toplam Kargo Maliyeti (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalShippingCost { get; set; }

        [Display(Name = "Toplam Maliyet (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;

        public ProductPurchaseHistory()
        {
            PurchaseDate = DateTime.UtcNow;
        }

        public void CalculateCosts()
        {
            TotalProductCost = Quantity * ProductPricePerUnit;
            TotalShippingCost = TotalKg * ShippingCostPerKg;
            TotalCost = TotalProductCost + TotalShippingCost;
        }
    }
}