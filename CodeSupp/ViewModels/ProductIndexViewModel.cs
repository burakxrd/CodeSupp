using CodeSupp.Enums;
using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    public class ProductIndexViewModel
    {
        public int Id { get; set; }
        public string? Code { get; set; }

        [Display(Name = "Ürün Adı")]
        public string? Name { get; set; }

        public string? ImagePath { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Beden")]
        public string? Size { get; set; }

        [Display(Name = "Renk")]
        public string? Color { get; set; }

        [Display(Name = "Birim Maliyet (₺)")]
        public decimal AverageUnitCostInTRY { get; set; }

        public int TotalSales { get; set; }
        public decimal CostPrice { get; set; }

        [Display(Name = "Stok Adedi")]
        public int Stock { get; set; }
        public decimal Price { get; set; }

        public decimal? DiscountedPrice { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string TypeName { get; set; } = "Fiziksel";
        public ProductType Type { get; set; }

        public ShippingType ShippingType { get; set; }
        public decimal ShippingPrice { get; set; }
    }
}