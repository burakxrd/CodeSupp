using System.Globalization;

namespace CodeSupp.Exceptions
{
    public class BusinessException : BaseException
    {
        // Opsiyonel: Frontend'de modal başlığı olarak göstermek istersen
        public string Title { get; } = "İşlem Hatası";

        // Senaryo 1: Düz mesaj
        // Örn: throw new BusinessException("Bu işlem için bakiyeniz yetersiz.");
        public BusinessException(string message)
            : base(message, 400) // 400: Bad Request
        {
        }

        // Senaryo 2: (Senior) Başlık + Mesaj
        // Örn: throw new BusinessException("Stok Hatası", "Ürün tükendi.");
        public BusinessException(string title, string message)
            : base(message, 400)
        {
            Title = title;
        }

        // Senaryo 3: (Senior++) Dinamik Formatlı Mesaj
        // Kod içinde string birleştirmekle uğraşma.
        // Örn: throw new BusinessException("Stok Yetersiz", "Talep edilen {0} adet, stokta {1} adet var.", 10, 5);
        public BusinessException(string title, string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args), 400)
        {
            Title = title;
        }
    }
}