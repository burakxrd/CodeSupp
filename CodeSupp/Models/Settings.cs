namespace CodeSupp.Models
{
    public class StoreSetting
    {
        public int Id { get; set; }
        public string? TenantId { get; set; }
        public bool ShowShippingColumn { get; set; }
        public bool EnableVAT { get; set; }
        public decimal DefaultVAT { get; set; } = 20;
    }
}
