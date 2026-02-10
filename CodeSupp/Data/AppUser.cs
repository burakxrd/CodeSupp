using Microsoft.AspNetCore.Identity;
using CodeSupp.Models;
using System.Collections.Generic;

namespace CodeSupp.Data
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? TenantId { get; set; } = string.Empty;
        public SubscriptionPackage PackageType { get; set; } = SubscriptionPackage.Starter;
        public string? DashboardSettings { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
        public virtual ICollection<Customer> Customers { get; set; } = new HashSet<Customer>();
    }
}