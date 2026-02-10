using Microsoft.AspNetCore.Http;

namespace CodeSupp.Services.Infrastructure
{
    public interface IFileService
    {
        // Resmi alır, işler, kaydeder ve dosya yolunu (string) döner
        Task<string> UploadImageAsync(IFormFile file, string folderName);

        // Eski resmi silmek için (Update işlemlerinde lazım olur)
        void DeleteFile(string filePath);
    }
}