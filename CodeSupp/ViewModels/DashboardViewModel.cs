using System;
using System.Collections.Generic;

namespace CodeSupp.ViewModels
{
    public class DashboardViewModel
    {
        // --- ÜST KARTLAR (Özet Sayılar + Trendler) ---
        public decimal TotalRevenue { get; set; }       // Toplam Ciro
        public int RevenueRate { get; set; }            // Ciro Artış Oranı (%)

        public int TotalOrders { get; set; }            // Toplam Sipariş
        public int OrderRate { get; set; }              // Sipariş Artış Oranı (%)

        public int TotalCustomers { get; set; }         // Müşteri Sayısı
        public int CustomerRate { get; set; }           // Müşteri Artış Oranı (%)

        public int TotalProducts { get; set; }          // Toplam Ürün Sayısı (SKU Çeşidi)

        // --- FİNANSAL KARTLAR ---
        public decimal NetProfit { get; set; }          // Net Kâr (Ciro - Maliyet - Giderler)
        public decimal TotalShippingCost { get; set; }  // Toplam Kargo Gideri
        public decimal TotalCommissions { get; set; }   // Toplam Platform Kesintisi
        public decimal TotalDiscounts { get; set; }     // Toplam İndirimler

        // [EKSİK OLAN ALAN BU] - Eklendi
        public decimal TotalOperationalExpenses { get; set; } // Toplam Operasyonel Gider (Kargo+Komisyon)
        // ------------------------------------------

        // --- STOK & ENVANTER KARTLARI ---
        public int TotalStockCount { get; set; }        // Toplam Stok Adedi
        public int CriticalStockCount { get; set; }     // Kritik Stoktaki Ürün Sayısı (<10)
        public int OutOfStockCount { get; set; }        // Tükenen Ürün Sayısı (0 olanlar)

        public decimal TotalInventoryValue { get; set; } // Envanter Değeri (Stok * Maliyet)
        public decimal PotentialRevenue { get; set; }    // Potansiyel Ciro (Stok * Satış Fiyatı)
        // -----------------------------------------------------------

        // --- GRAFİK VERİLERİ (Son 6 Ay) ---
        public List<decimal> Last6MonthsRevenue { get; set; } = new List<decimal>();
        public List<string> Last6MonthsLabels { get; set; } = new List<string>();

        // --- LİSTELER ---
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
        public List<TopProductViewModel> TopSellingProducts { get; set; } = new List<TopProductViewModel>();
    }

    // Son Siparişler Tablosu İçin Model
    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public string? OrderCode { get; set; }
        public string? CustomerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }
        public int ItemCount { get; set; }
        public List<string> ProductNames { get; set; } = new List<string>();
        public decimal RemainingAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ManualDiscount { get; set; }
    }

    // En Çok Satanlar Listesi İçin Model
    public class TopProductViewModel
    {
        public string? Name { get; set; }
        public int SalesCount { get; set; }
        public int Percentage { get; set; }
    }

    public class FinanceSummaryViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetBalance { get; set; }
    }
}