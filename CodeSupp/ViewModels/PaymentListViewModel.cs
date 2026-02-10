using CodeSupp.Enums;
using System;
using System.Collections.Generic;

namespace CodeSupp.ViewModels
{
    public class PaymentListViewModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; } 
        public string CustomerName { get; set; } = string.Empty;
        public List<string> ProductNames { get; set; } = new List<string>();
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public TransactionCategory Category { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}