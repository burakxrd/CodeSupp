using CodeSupp.Enums;

namespace CodeSupp.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public TransactionType Type { get; set; }

        public string TypeName => Type == TransactionType.Income ? "Gelir" : "Gider";

        public TransactionCategory Category { get; set; }
        public string CategoryName => Category.ToString(); 

        public string Description { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName => PaymentMethod.ToString();

        public decimal Amount { get; set; }
    }
}