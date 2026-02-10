// Dosya: Models/CustomerProductPrice.cs (Güncellenmiş Hali)

using CodeSupp.Data; // AppUser için
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSupp.Models
{
    // "Bu müşteri bu ürünü bu fiyattan alır" tablosu
    public class CustomerProductPrice
    {
        public int Id { get; set; } // Birincil Anahtar

        [Required]
        public int ProductId { get; set; } // Foreign Key
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int CustomerId { get; set; } // Foreign Key
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        // --- OMURGA: Multi-Tenant İlişkisi ---
        [Required]
        public string TenantId { get; set; } = null!;
        // ------------------------------------

        public CustomerProductPrice()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}