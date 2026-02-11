using CodeSupp.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSupp.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;

        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }

        public TransactionType Type { get; set; }

        public TransactionCategory Category { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ExpenseId { get; set; }
        [ForeignKey("ExpenseId")]
        public virtual Expense? Expense { get; set; }
        public int? PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public virtual Payment? Payment { get; set; }

        public Transaction()
        {
            CreatedAt = DateTime.UtcNow;
            Date = DateTime.UtcNow;
        }
    }
}