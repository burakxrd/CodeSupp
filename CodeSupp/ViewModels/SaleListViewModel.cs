using System;
using System.Collections.Generic;

namespace CodeSupp.ViewModels
{
    // [YENİ] Ürün Tooltip'i ve detayları için gerekli ufak paketçik
    public class SaleProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class SaleListViewModel
    {
        public int Id { get; set; }

        public string? OrderCode { get; set; }
        public string? ExternalReference { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerAddress { get; set; }

        public DateTime SaleDate { get; set; }
        public List<SaleProductDto> Products { get; set; } = new List<SaleProductDto>();

        public int PaymentCount { get; set; }
        public int ItemCount { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public decimal ShippingCost { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal ManualDiscount { get; set; }

        public decimal TaxAmount { get; set; }

        public string PaymentStatus { get; set; } = "Bekliyor";
        public string? ShippingStatus { get; set; }
    }
}