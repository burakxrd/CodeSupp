using System;

namespace CodeSupp.Dtos
{
    public class SaleFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? CustomerId { get; set; } 
        public string? ShippingStatus { get; set; }
        public string? PaymentStatus { get; set; } 

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SortBy { get; set; }
        public string SortDir { get; set; } = "desc"; 
    }
}