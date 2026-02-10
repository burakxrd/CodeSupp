namespace CodeSupp.Enums
{
    public enum ProductType
    {
        Physical = 0, // Fiziksel Ürün (Stok düşer, Kargo gerekir)
        Digital = 1,  // Dijital Ürün (Stok düşebilir/düşmeyebilir, Kargo yok)
        Service = 2   // Hizmet (Stok yok, Kargo yok - Örn: Danışmanlık)
    }
}