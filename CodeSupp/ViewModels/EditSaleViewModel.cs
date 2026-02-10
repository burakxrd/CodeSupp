using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CodeSupp.Models;

namespace CodeSupp.ViewModels
{
    public class EditSaleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Müşteri")]
        [Required(ErrorMessage = "Lütfen bir müşteri seçin.")]
        public int CustomerId { get; set; }

        [Display(Name = "Satış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; }

        [Display(Name = "Kargo Maliyeti (Gider)")]
        public decimal ShippingCost { get; set; } = 0;

        [Display(Name = "Platform Komisyonu (Gider)")]
        public decimal PlatformCommission { get; set; } = 0;

        [Display(Name = "Sepet İndirimi (Manuel)")]
        public decimal ManualDiscount { get; set; } = 0;

        [Display(Name = "Referans / Fatura No")]
        public string? ExternalReference { get; set; }
        public byte[]? RowVersion { get; set; }
        public List<SaleItemViewModel> Items { get; set; } = new List<SaleItemViewModel>();
        public IEnumerable<ProductInfoForSaleViewModel> AvailableProducts { get; set; } = new List<ProductInfoForSaleViewModel>();
        public IEnumerable<Customer> AvailableCustomers { get; set; } = new List<Customer>();
    }
}