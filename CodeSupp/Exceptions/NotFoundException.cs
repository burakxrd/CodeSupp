namespace CodeSupp.Exceptions
{
    public class NotFoundException : BaseException
    {
        // Senaryo 1: Düz mesaj vermek istersen
        // Örn: throw new NotFoundException("Kullanıcı bulunamadı.");
        public NotFoundException(string message)
            : base(message, 404)
        {
        }

        // Senaryo 2: (Senior) Standart format
        // Örn: throw new NotFoundException("Product", 15);
        // Çıktı: "Product with id (15) was not found."
        public NotFoundException(string name, object key)
            : base($"{name} with id ({key}) was not found.", 404)
        {
        }
    }
}