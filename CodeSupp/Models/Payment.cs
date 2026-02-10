using CodeSupp.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSupp.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Display(Name = "Müşteri")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [Display(Name = "Tutar (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }
        public TransactionCategory Category { get; set; } = TransactionCategory.Sale;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
        public int? SaleId { get; set; } 
        [ForeignKey("SaleId")]
        public virtual Sale? Sale { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;

        public Payment()
        {
            CreatedAt = DateTime.UtcNow;
            Date = DateTime.UtcNow;
        }
    }
}