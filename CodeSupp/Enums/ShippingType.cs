using System.ComponentModel.DataAnnotations;

namespace CodeSupp.Enums
{
    public enum ShippingType
    {
        [Display(Name = "Ücretsiz Kargo")]
        Free = 0,

        [Display(Name = "Alıcı Öder")]
        BuyerPays = 1,

        [Display(Name = "Kapıda Ödeme")]
        PayAtDoor = 2
    }
}