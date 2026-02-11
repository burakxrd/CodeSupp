namespace CodeSupp.Services.Infrastructure
{
    /// <summary>
    /// Türkçe karakter normalizasyonu ve metin işlemleri için yardımcı servis.
    /// </summary>
    public static class TextNormalizer
    {
        /// <summary>
        /// Türkçe karakterleri İngilizce eşdeğerlerine çevirir ve küçük harfe dönüştürür.
        /// Örnek: "İstanbul Öğrenci" → "istanbul ogrenci"
        /// </summary>
        public static string NormalizeTurkish(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input
                .Replace("İ", "i").Replace("I", "i").Replace("ı", "i")
                .Replace("Ö", "o").Replace("ö", "o")
                .Replace("Ü", "u").Replace("ü", "u")
                .Replace("Ş", "s").Replace("ş", "s")
                .Replace("Ğ", "g").Replace("ğ", "g")
                .Replace("Ç", "c").Replace("ç", "c")
                .ToLowerInvariant();
        }

        /// <summary>
        /// Telefon numarasını sadece rakamlardan oluşacak şekilde temizler.
        /// Örnek: "+90 (532) 123-4567" → "905321234567"
        /// </summary>
        public static string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return new string(phone.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Email adresini küçük harfe çevirir ve trim yapar.
        /// Örnek: "  User@EXAMPLE.com  " → "user@example.com"
        /// </summary>
        public static string NormalizeEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            return email.Trim().ToLowerInvariant();
        }
    }
}