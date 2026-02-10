using CodeSupp.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSupp.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Tutar (TL)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public TransactionCategory Category { get; set; } = TransactionCategory.OtherExpense;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Gider Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;
        public int? ProductPurchaseHistoryId { get; set; }

        [ForeignKey("ProductPurchaseHistoryId")]
        public virtual ProductPurchaseHistory? ProductPurchaseHistory { get; set; }
        public Expense()
        {
            CreatedAt = DateTime.UtcNow;
            Date = DateTime.UtcNow;
        }
    }
}