namespace CodeSupp.Enums
{
    public enum ShippingStatus
    {
        // 0 genelde 'Bilinmeyen' veya 'Hata' durumu için ayrılır, 1'den başlayalım.
        SiparisAlindi = 1,
        Hazirlaniyor = 2,
        KargoyaVerildi = 3,
        TeslimEdildi = 4,
        IptalEdildi = 5,
        IadeEdildi = 6
    }
}