using FluentValidation;
using CodeSupp.ViewModels;

namespace CodeSupp.Validators
{
    // Ana Satış Modeli için Doğrulama Kuralları
    public class CreateSaleValidator : AbstractValidator<CreateSaleViewModel>
    {
        public CreateSaleValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("Lütfen geçerli bir müşteri seçiniz.");

            RuleFor(x => x.SaleDate)
                .NotEmpty()
                .WithMessage("Satış tarihi boş bırakılamaz.")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)) // Geleceğe işlem yapılmasın (ufak esneklikle)
                .WithMessage("İleri tarihli satış girilemez.");

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Sepet boş olamaz.")
                .Must(items => items != null && items.Any())
                .WithMessage("Satış oluşturmak için en az bir ürün eklemelisiniz.");

            // Listenin içindeki her bir elemanı (satırı) tek tek kontrol et
            RuleForEach(x => x.Items).SetValidator(new SaleItemValidator());
        }
    }

    // Satışın içindeki her bir satır (Ürün) için Doğrulama Kuralları
    public class SaleItemValidator : AbstractValidator<SaleItemViewModel>
    {
        public SaleItemValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Ürün seçimi hatalı.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Miktar en az 1 olmalıdır.");

            // Fiyat 0 olabilir (Promosyon), ama negatif olamaz
            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Birim fiyat negatif olamaz.");
        }
    }
}