using CodeSupp.Models;
using CodeSupp.Services.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CodeSupp.Services.Infrastructure;

namespace CodeSupp.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        private readonly ITenantService _tenantService;
        private readonly string? _currentTenantId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
            _currentTenantId = _tenantService.GetTenantId();
        }

        // 1. Ürün/Stok ve Kategori Grubu
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPurchaseHistory> ProductPurchaseHistories { get; set; }
        public DbSet<Category> Categories { get; set; }

        // 2. Müşteri Grubu
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerProductPrice> CustomerProductPrices { get; set; }

        // 3. Satış Grubu
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        // 4. Finans Grubu
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<StoreSetting> StoreSettings { get; set; }

        // [YENİ] Genel Hesap Hareketleri
        public DbSet<Transaction> Transactions { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(_currentTenantId))
            {
                var entries = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

                foreach (var entry in entries)
                {
                    var tenantProp = entry.Entity.GetType().GetProperty("TenantId");

                    if (tenantProp != null && tenantProp.PropertyType == typeof(string))
                    {
                        if (entry.State == EntityState.Added)
                        {
                            tenantProp.SetValue(entry.Entity, _currentTenantId);
                        }
                        else if (entry.State == EntityState.Modified)
                        {
                            var originalTenantId = entry.OriginalValues["TenantId"] as string;
                            var currentPropValue = tenantProp.GetValue(entry.Entity) as string;

                            if (originalTenantId != currentPropValue)
                            {
                                throw new InvalidOperationException(
                                    $"CRITICAL SECURITY ERROR: '{entry.Entity.GetType().Name}' kaydının TenantId değeri değiştirilemez!"
                                );
                            }
                        }
                    }

                    if (entry.Entity is Customer customer)
                    {
                        customer.SearchText = TextNormalizer.NormalizeTurkish(customer.Name);
                    }

                    if (entry.Entity is Product product)
                    {
                        product.SearchText = TextNormalizer.NormalizeTurkish($"{product.Name} {product.Code}");
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Bu eklenti veritabanına "Büyük/Küçük harf ayrımı yapmayan metin tipi" özelliğini ekler.
            builder.HasPostgresExtension("citext");

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(string)))
                {

                    if (!property.Name.EndsWith("Id") &&
                        !property.Name.Equals("Discriminator") &&
                        !property.Name.Equals("UserId") && 
                        !property.Name.StartsWith("Concurrency")) 
                    {
                        property.SetColumnType("citext");
                    }
                }
            }
            // ==============================================================================
            // GLOBAL QUERY FILTERS (TENANT GÜVENLİĞİ)
            // ==============================================================================

            builder.Entity<Product>().HasQueryFilter(x => x.TenantId == _currentTenantId);
            builder.Entity<ProductPurchaseHistory>().HasQueryFilter(x => x.TenantId == _currentTenantId);
            builder.Entity<Category>().HasQueryFilter(x => x.TenantId == _currentTenantId);

            builder.Entity<Customer>().HasQueryFilter(x => x.TenantId == _currentTenantId);
            builder.Entity<CustomerProductPrice>().HasQueryFilter(x => x.TenantId == _currentTenantId);

            builder.Entity<Sale>().HasQueryFilter(x => x.TenantId == _currentTenantId);
            builder.Entity<SaleItem>().HasQueryFilter(x => x.TenantId == _currentTenantId);

            builder.Entity<Expense>().HasQueryFilter(x => x.TenantId == _currentTenantId);
            builder.Entity<Payment>().HasQueryFilter(x => x.TenantId == _currentTenantId);
            builder.Entity<StoreSetting>().HasQueryFilter(x => x.TenantId == _currentTenantId);

            // [YENİ] Transaction Güvenliği
            builder.Entity<Transaction>().HasQueryFilter(x => x.TenantId == _currentTenantId);

            // ==============================================================================
            // İLİŞKİ TANIMLAMALARI (FLUENT API)
            // ==============================================================================

            // --- 1. Sale -> Customer ---
            builder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 2. SaleItem -> Sale ---
            builder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 3. SaleItem -> Product ---
            builder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 4. ProductPurchaseHistory -> Product ---
            builder.Entity<ProductPurchaseHistory>()
                .HasOne(h => h.Product)
                .WithMany(p => p.PurchaseHistories)
                .HasForeignKey(h => h.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 5. CustomerProductPrice İlişkileri ---
            builder.Entity<CustomerProductPrice>()
                .HasOne(cpp => cpp.Product)
                .WithMany()
                .HasForeignKey(cpp => cpp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerProductPrice>()
                .HasOne(cpp => cpp.Customer)
                .WithMany()
                .HasForeignKey(cpp => cpp.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 6. Payment İlişkileri ---
            builder.Entity<Payment>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- 7. Product -> Category İlişkisi ---
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- [YENİ] Transaction İlişkileri ---

            // Expense silinirse Transaction da silinsin (Cascade)
            builder.Entity<Transaction>()
                .HasOne(t => t.Expense)
                .WithMany() // Expense tarafında koleksiyon tutmuyoruz
                .HasForeignKey(t => t.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment silinirse Transaction da silinsin (Cascade)
            builder.Entity<Transaction>()
                .HasOne(t => t.Payment)
                .WithMany()
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}