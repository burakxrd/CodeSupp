namespace CodeSupp.Enums
{
    public enum TransactionType
    {
        Income = 1, // Gelir
        Expense = 2 // Gider
    }

    public enum PaymentMethod
    {
        CreditCard = 1, // Kredi Kartı
        BankTransfer = 2, // Havale/EFT
        Cash = 3, // Nakit
        Other = 4 // Diğer
    }

    public enum TransactionCategory
    {
        // GELİR KATEGORİLERİ
        Sale = 10,        // Satış Geliri (Otomatik)
        Capital = 11,     // Sermaye Ekleme
        OtherIncome = 12, // Diğer Gelir

        // GİDER KATEGORİLERİ
        StockPurchase = 50, // Mal Alımı (Otomatik)
        Rent = 51,          // Kira
        Salary = 52,        // Personel Maaşı
        Marketing = 53,     // Reklam/Pazarlama
        Tax = 54,           // Vergi
        Bills = 55,         // Faturalar (Elektrik/Su/İnternet)
        Refund = 56,        // İade
        OtherExpense = 99   // Diğer Gider
    }
}