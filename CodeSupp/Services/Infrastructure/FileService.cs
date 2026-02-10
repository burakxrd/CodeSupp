using CodeSupp.Services.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace CodeSupp.Services.Infrastructure
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantService _tenantService;

        // Buraya TenantService'i mecbur ekliyoruz çünkü DbContext kullanmıyoruz.
        // Dosya yollarını ayırmak için TenantId'ye ihtiyacımız var.
        public FileService(IWebHostEnvironment environment, ITenantService tenantService)
        {
            _environment = environment;
            _tenantService = tenantService;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            // 1. Dosya Kontrolü
            if (file == null || file.Length == 0) return null!;

            var tenantId = _tenantService.GetTenantId();
            if (string.IsNullOrEmpty(tenantId)) throw new UnauthorizedAccessException("Tenant ID bulunamadı. Dosya yüklenemez.");

            // 2. Klasör Yolu: wwwroot/uploads/{tenantId}/{folderName}
            // SaaS mimarisinde dosyaları müşteri bazlı ayırmak en temiz yöntemdir.
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", tenantId, folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 3. Güvenli Dosya Adı
            var uniqueFileName = Guid.NewGuid().ToString() + ".jpg";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. ImageSharp İşlemleri
            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                // Resize
                if (image.Width > 800)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(800, 0),
                        Mode = ResizeMode.Max
                    }));
                }

                // Compress & Save
                var encoder = new JpegEncoder { Quality = 75 };
                await image.SaveAsync(filePath, encoder);
            }

            // 5. Dönüş Yolu: /uploads/{tenantId}/{folderName}/abc.jpg
            return $"/uploads/{tenantId}/{folderName}/{uniqueFileName}";
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                // Güvenlik Kontrolü:
                // Silinmek istenen dosya yolu, şu anki Tenant'ın klasöründe mi?
                // Örn: Kullanıcı TenantA iken, /uploads/TenantB/.. silmeye çalışırsa engellemeliyiz.

                var tenantId = _tenantService.GetTenantId();
                if (!string.IsNullOrEmpty(tenantId) && !filePath.Contains($"/{tenantId}/"))
                {
                    // Log: "TenantA, TenantB'nin dosyasını silmeye çalıştı."
                    return; // İşlemi sessizce reddet
                }

                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Log mekanizması eklenebilir.
            }
        }
    }
}