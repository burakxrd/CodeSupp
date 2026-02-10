using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CodeSupp.Enums;

namespace CodeSupp.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? OrderCode { get; set; }

        [StringLength(100)]
        public string? ExternalReference { get; set; }

        [Display(Name = "Müşteri")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [Display(Name = "Vergi / KDV Gideri")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Display(Name = "Satış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; }

        [Display(Name = "Toplam Tutar (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Kargo Maliyeti (Gider)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Display(Name = "Platform Komisyonu (Gider)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PlatformCommission { get; set; } = 0;

        [Display(Name = "Manuel Sepet İndirimi")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ManualDiscount { get; set; } = 0;

        [Display(Name = "Kargo Durumu")]
        public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.SiparisAlindi;

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "Satışın bir sahibi (TenantId) olmalıdır.")]
        public string TenantId { get; set; } = null!;
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }

        public Sale()
        {
            CreatedAt = DateTime.UtcNow;
            SaleDate = DateTime.UtcNow;
            SaleItems = new HashSet<SaleItem>();
            Payments = new HashSet<Payment>();
        }

        [NotMapped]
        public decimal CollectedAmount => Payments?.Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal RemainingAmount => TotalAmount - CollectedAmount;

        [NotMapped]
        public int ItemCount => SaleItems?.Sum(i => i.Quantity) ?? 0;

        [NotMapped]
        public string PaymentStatusDescription
        {
            get
            {
                if (TotalAmount == 0) return "Ücretsiz";
                if (RemainingAmount <= 0.01m) return "Tamamlandı";
                if (CollectedAmount > 0) return "Kısmi";
                return "Bekliyor";
            }
        }
    }
}