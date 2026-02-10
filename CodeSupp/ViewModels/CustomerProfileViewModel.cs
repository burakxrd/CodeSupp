// Dosya: ViewModels/CustomerProfileViewModel.cs (YENİ DOSYA)

using CodeSupp.Models; // 'using CRMYonetimSistemi.Models' yerine bunu kullan
using System.Collections.Generic;

namespace CodeSupp.ViewModels // 'namespace CRMYonetimSistemi.ViewModels' yerine bunu kullan
{
    public class CustomerProfileViewModel
    {
        // Müşterinin kendi bilgilerini tutar
        public Customer Customer { get; set; } = new Customer();

        // Müşteriye yapılan satışların listesini tutar
        public List<Sale> Sales { get; set; } = new List<Sale>();
    }
}