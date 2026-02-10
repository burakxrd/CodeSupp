using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    public class ProductPurchaseViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Satın Alınacak Ürün")]
        [Required(ErrorMessage = "Lütfen bir ürün seçin.")]
        public int ProductId { get; set; }
        public ProductIndexViewModel? Product { get; set; }

        public IEnumerable<SelectListItem> Products { get; set; } = new List<SelectListItem>();

        [Display(Name = "Miktar (Adet)")]
        [Required(ErrorMessage = "Miktar girilmesi zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        public int QuantityInUnits { get; set; }

        [Display(Name = "Ürün Birim Alış Fiyatı (TL)")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat negatif olamaz.")]
        public decimal ProductPricePerUnit { get; set; }

        [Display(Name = "Toplam Kargo Ağırlığı (Kg)")]
        [Range(0, double.MaxValue, ErrorMessage = "Ağırlık negatif olamaz.")]
        public decimal TotalKg { get; set; }

        [Display(Name = "Kargo Ücreti (Kg Başına, TL)")]
        [Range(0, double.MaxValue, ErrorMessage = "Kargo ücreti negatif olamaz.")]
        public decimal ShippingCostPerKg { get; set; }
        [Display(Name = "Toplam Kargo Maliyeti")]
        public decimal TotalShippingCost => TotalKg * ShippingCostPerKg;
        [Display(Name = "Birim Başına Gerçek Maliyet")]
        public decimal EffectiveUnitCost
        {
            get
            {
                if (QuantityInUnits <= 0) return 0;
                return ProductPricePerUnit + (TotalShippingCost / QuantityInUnits);}}
        [Display(Name = "Satın Alım Tarihi")]
        public DateTime? PurchaseDate { get; set; }
        [Display(Name = "Toplam Maliyet")]
        public decimal TotalCost { get; set; }
        [Display(Name = "Açıklama / Fatura No")]
        public string? Description { get; set; }
    }
}