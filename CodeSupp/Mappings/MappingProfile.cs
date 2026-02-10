using AutoMapper;
using CodeSupp.Dtos;
using CodeSupp.Enums;
using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==========================================================
            // 1. BASİT TİPLER VE YARDIMCILAR (Önce bunları tanımla)
            // ==========================================================

            // SaleItem -> SaleProductDto (Satış listesindeki ürün detayı için)
            // Bunu buraya ayırdığımız için aşağıda "Select(new...)" yazmaktan kurtuluyoruz.
            CreateMap<SaleItem, SaleProductDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Silinmiş Ürün"))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImagePath : null));

            // ==========================================================
            // 2. KATEGORİLER
            // ==========================================================
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // ==========================================================
            // 3. ÖDEMELER (PAYMENTS)
            // ==========================================================
            CreateMap<Payment, PaymentListViewModel>()
                .ForMember(dest => dest.CustomerName, opt =>
                    opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.ProductNames, opt =>
                    opt.MapFrom(src => src.Sale != null
                        ? src.Sale.SaleItems.Select(si => si.Product != null ? si.Product.Name : "Silinmiş Ürün").OrderBy(n => n).ToList()
                        : new List<string>()));

            CreateMap<PaymentViewModel, Payment>();

            // ==========================================================
            // 4. GİDERLER (EXPENSES)
            // ==========================================================
            CreateMap<Expense, ExpenseViewModel>();
            CreateMap<ExpenseViewModel, Expense>();

            // ==========================================================
            // 5. SATIŞLAR (SALES) - "DUMB" & CLEAN
            // ==========================================================
            CreateMap<Sale, SaleListViewModel>()
                // İsimleri aynı olanları SİLDİM (CollectedAmount, RemainingAmount, ItemCount vb.)
                // AutoMapper bunları otomatik eşleştirir.

                // Müşteri Adı (Flattening)
                .ForMember(dest => dest.CustomerName, opt =>
                    opt.MapFrom(src => src.Customer != null ? src.Customer.Name : "Silinmiş Müşteri"))

                // Enum -> String Dönüşümü
                .ForMember(dest => dest.ShippingStatus, opt => opt.MapFrom(src => src.ShippingStatus.ToString()))

                // Eğer Sale entity'sinde "PaymentStatusDescription" diye bir property yoksa,
                // ve bunu Frontend'e "Bekliyor/Tamamlandı" diye göndermek istiyorsan bu mantık burada kalabilir.
                // Ama ideali ViewModel içinde computed property yapmaktır. Şimdilik bozmadım.
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatusDescription))

                .ForMember(dest => dest.PaymentCount, opt => opt.MapFrom(src => src.Payments.Count))

                // Ürünler Listesi (Yukarıdaki 'SaleItem -> SaleProductDto' haritasını kullanır)
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.SaleItems));

            // ==========================================================
            // 6. ÜRÜNLER (PRODUCTS)
            // ==========================================================
            CreateMap<Product, ProductIndexViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Genel"))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
                .ReverseMap();

            CreateMap<StockAdjustmentDto, ProductPurchaseHistory>();
        }
    }
}