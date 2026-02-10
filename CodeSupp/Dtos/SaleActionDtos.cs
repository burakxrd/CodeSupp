using System.ComponentModel.DataAnnotations;
using CodeSupp.Enums;
using CodeSupp.ViewModels;

namespace CodeSupp.Dtos
{
    // 1. Kargo Durumu Güncelleme İçin Güvenli DTO (String yerine Enum)
    public class UpdateShippingStatusDto
    {
        [Required(ErrorMessage = "Durum alanı zorunludur.")]
        // Gelen değerin ShippingStatus enum'ına uygun olup olmadığını otomatik kontrol eder.
        [EnumDataType(typeof(ShippingStatus), ErrorMessage = "Geçersiz kargo durumu.")]
        public ShippingStatus Status { get; set; }
    }

    // 2. Toplu Satış İçin Kapsayıcı (Wrapper) DTO
    // Listeyi doğrudan almak yerine bu kutunun içinde alıyoruz.
    public class BulkSaleRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "En az bir satış kalemi olmalıdır.")]
        public List<BulkSaleItemViewModel> Items { get; set; } = new();
    }
}