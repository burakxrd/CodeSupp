using CodeSupp.Data;
using CodeSupp.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CodeSupp.Models
{
    public class Product
    {
        public int Id { get; set; }
        [JsonIgnore] 
        public string? SearchText { get; set; }

        [Display(Name = "Ürün Adı")]
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Ürün Görseli")]
        public string? ImagePath { get; set; }

        [Display(Name = "Alış Fiyatı (Maliyet)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostPrice { get; set; }

        [Display(Name = "Satış Fiyatı (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Display(Name = "İndirimli Fiyat (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? DiscountedPrice { get; set; }

        [Display(Name = "Stok Adedi")]
        public int Stock { get; set; }

        [Display(Name = "Ürün Kodu")]
        [StringLength(50)]
        public string? Code { get; set; }

        [Display(Name = "Beden")]
        [StringLength(10)] 
        public string? Size { get; set; }

        [Display(Name = "Renk")]
        [StringLength(30)]
        public string? Color { get; set; }

        public ProductType Type { get; set; } = ProductType.Physical;

        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Display(Name = "Kargo Tipi")]
        public ShippingType ShippingType { get; set; } = ShippingType.Free;

        [Display(Name = "Kargo Ücreti")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingPrice { get; set; } = 0;
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string TenantId { get; set; } = null!;

        public virtual ICollection<ProductPurchaseHistory> PurchaseHistories { get; set; }

        public Product()
        {
            PurchaseHistories = new HashSet<ProductPurchaseHistory>();
        }
    }
}