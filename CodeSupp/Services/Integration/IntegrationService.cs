using CodeSupp.Services.Identity;
using System.Net.Http.Headers;

namespace CodeSupp.Services.Integration
{
    public class IntegrationService : IIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITenantService _tenantService;
        private readonly IConfiguration _configuration;

        public IntegrationService(IHttpClientFactory httpClientFactory, ITenantService tenantService, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tenantService = tenantService;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeDocumentAsync(IFormFile file, string docType)
        {
            // Tenant bilgisini al (Dış servise göndermek için)
            var tenantId = _tenantService.GetTenantId();

            // URL'i config'den al
            var webhookUrl = _configuration["Integration:N8nWebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl)) throw new Exception("Entegrasyon URL'i yapılandırılmamış.");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5); // OCR işlemi uzun sürebilir

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            // Payload Hazırlığı
            content.Add(streamContent, "file", file.FileName);
            content.Add(new StringContent(tenantId ?? "unknown"), "tenantId");
            content.Add(new StringContent(docType), "docType");

            var response = await client.PostAsync(webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OCR Servisi Hatası ({response.StatusCode}): {errorContent}");
            }

            var jsonResult = await response.Content.ReadAsStringAsync();

            // LLM bazen Markdown formatında JSON döner, temizleyelim
            return jsonResult.Replace("```json", "").Replace("```", "").Trim();
        }
    }
}