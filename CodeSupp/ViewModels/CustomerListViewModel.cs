using System;

namespace CodeSupp.ViewModels
{
    public class CustomerListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? InstagramHandle { get; set; }

        // Listeleme ve filtreleme için gerekli metrikler
        public int OrderCount { get; set; }        // Toplam Sipariş Sayısı
        public decimal TotalSpent { get; set; }    // Toplam Harcama
        public DateTime? LastOrderDate { get; set; } // Son Sipariş Tarihi (Hesaplamalar için kritik)

        // Statü Flagleri (Servis tarafında hesaplanıp doldurulacak)
        // Frontend bu flag'lere bakarak ilgili badge'leri/etiketleri gösterecek.
        public bool IsVip { get; set; }       // 5'ten fazla sipariş
        public bool IsLoyal { get; set; }     // 3 veya 4 sipariş
        public bool IsNew { get; set; }       // Son 30 günde 1-2 sipariş
        public bool IsRisky { get; set; }     // 45-75 gündür sipariş yok
        public bool IsPlus75 { get; set; }    // 75+ gündür sipariş yok
    }
}