using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Customers
{
    public interface ICustomerService
    {
        /// <summary>
        /// Müşterileri sayfalı ve filtreli şekilde listeler.
        /// </summary>
        Task<PaginatedResult<CustomerListViewModel>> GetCustomersAsync(int pageNumber = 1, int pageSize = 10, string? statusFilter = null, string? searchTerm = null);

        /// <summary>
        /// [YENİ] Verilen isime sahip müşteriyi arar. 
        /// Varsa onu döndürür, yoksa yeni bir kayıt oluşturup döndürür.
        /// (Bulk Import ve Hızlı Satış işlemleri için idealdir)
        /// </summary>
        Task<Customer> GetOrCreateCustomerAsync(string name, string? phone, string? address);
    }
}