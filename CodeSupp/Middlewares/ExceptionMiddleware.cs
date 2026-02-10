using CodeSupp.Exceptions;
using CodeSupp.Models;
using System.Net;

namespace CodeSupp.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Log Kirliliğini Önleme
                // İş mantığı hataları (stok yok vs.) sistem hatası değildir, uyarıdır.
                if (ex is BaseException)
                    _logger.LogWarning("İşlem Hatası: {Message} | IP: {Ip}", ex.Message, httpContext.Connection.RemoteIpAddress);
                else
                    _logger.LogError(ex, "Kritik Sunucu Hatası: {Message}", ex.Message);

                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Varsayılan yanıt (500 - Internal Server Error)
            var response = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Title = "Sunucu Hatası",
                Message = "Beklenmedik bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."
            };

            // Modern Pattern Matching
            switch (exception)
            {
                // 1. Bizim Özel Hatalarımız
                case BaseException baseEx:
                    response.StatusCode = baseEx.StatusCode;
                    response.Message = baseEx.Message;

                    // Alt türlere göre başlık güzelleştirme
                    if (baseEx is BusinessException busEx) response.Title = busEx.Title;
                    else if (baseEx is NotFoundException) response.Title = "Kayıt Bulunamadı";
                    else if (baseEx is UnauthorizedException) response.Title = "Yetkisiz İşlem";
                    break;

                // 2. .NET'in Standart Yetki Hatası (Fallback)
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Title = "Erişim Engellendi";
                    response.Message = "Bu işlemi yapmak için yetkiniz bulunmuyor.";
                    break;

                // 3. Diğer Hatalar (500)
                default:
                    if (_env.IsDevelopment())
                    {
                        response.Message = exception.Message;
                        response.Detail = exception.StackTrace;
                    }
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    // Bu dosyanın altına (veya Models klasörüne) eklenebilecek modern response modeli.
    // ErrorDetails sınıfını buna yükseltebiliriz.
    internal class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Detail { get; set; }
    }
}