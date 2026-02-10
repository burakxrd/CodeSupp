using CodeSupp.Enums;
using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    public class ExpenseViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public TransactionCategory Category { get; set; } = TransactionCategory.OtherExpense;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}