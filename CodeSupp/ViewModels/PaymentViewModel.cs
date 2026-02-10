using CodeSupp.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    public class PaymentViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Müşteri")]
        public int? CustomerId { get; set; }

        public int? SaleId { get; set; }

        [Display(Name = "Tutar")]
        [Required(ErrorMessage = "Tutar girilmesi zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Tarih")]
        [Required(ErrorMessage = "Tarih seçimi zorunludur.")]
        public DateTime Date { get; set; }

        public TransactionCategory Category { get; set; } = TransactionCategory.Sale;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
    }
}