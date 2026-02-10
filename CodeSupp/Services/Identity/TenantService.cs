namespace CodeSupp.Services.Identity
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public const string TenantIdClaimType = "tenantId";

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;


            if (context?.User?.Identity?.IsAuthenticated is not true)
            {
                return null;
            }

            var tenantClaim = context.User.FindFirst(TenantIdClaimType);

            return string.IsNullOrEmpty(tenantClaim?.Value) ? null : tenantClaim.Value;
        }
    }
}