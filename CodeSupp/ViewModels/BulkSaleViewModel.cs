using System.ComponentModel.DataAnnotations;

namespace CodeSupp.ViewModels
{
    // [INPUT] Frontend'den gelen toplu sipariş satırı
    public class BulkSaleItemViewModel
    {
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public string? Email { get; set; }
        public string? InstagramHandle { get; set; }

        public List<BulkSaleProductDetail> Products { get; set; } = new List<BulkSaleProductDetail>();
    }

    // [INPUT DETAIL] Toplu siparişin içindeki ürün detayı
    public class BulkSaleProductDetail
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // ==========================================
    // [OUTPUT] YENİ EKLENEN: İŞLEM SONUCU RAPORU
    // ==========================================
    // Toplu işlem (Excel yükleme vs.) bittiğinde Controller bu sınıfı döndürür.

    public class SalesProcessResult
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<string> Errors { get; set; } = new();

        // Frontend'e özet bilgi vermek için calculated property
        public string Message => $"{SuccessCount} işlem başarılı, {FailCount} işlem başarısız.";
    }
}