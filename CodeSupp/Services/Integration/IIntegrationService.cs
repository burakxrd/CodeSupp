namespace CodeSupp.Services.Integration
{
    public interface IIntegrationService
    {
        Task<string> AnalyzeDocumentAsync(IFormFile file, string docType);
    }
}