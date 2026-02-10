using CodeSupp.Models; // Customer için
using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    // 1. INPUT: Frontend'den POST isteği ile gelen asıl satış verisi
    public class CreateSaleViewModel
    {
        [Display(Name = "Müşteri")]
        [Required(ErrorMessage = "Lütfen bir müşteri seçin.")]
        public int CustomerId { get; set; }

        [Display(Name = "Satış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Kargo Maliyeti (Gider)")]
        public decimal ShippingCost { get; set; } = 0;

        [Display(Name = "Platform Komisyonu (Gider)")]
        public decimal PlatformCommission { get; set; } = 0;

        [Display(Name = "Sepet İndirimi (Manuel)")]
        public decimal ManualDiscount { get; set; } = 0;

        [Display(Name = "Referans / Fatura No")]
        public string? ExternalReference { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "En az 1 ürün eklemelisiniz.")]
        public List<SaleItemViewModel> Items { get; set; } = new List<SaleItemViewModel>();
    }

    // 2. INPUT ITEM: Satışın içindeki her bir satır
    public class SaleItemViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Display(Name = "Miktar (Adet)")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        public int Quantity { get; set; }

        [Display(Name = "Birim Fiyat")]
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }
    }

    public class CreateSaleDataViewModel
    {
        public List<Customer> Customers { get; set; } = new();
        public List<ProductInfoForSaleViewModel> Products { get; set; } = new();
    }

    public class ProductInfoForSaleViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public decimal AverageUnitCostInTRY { get; set; } 
    }
}