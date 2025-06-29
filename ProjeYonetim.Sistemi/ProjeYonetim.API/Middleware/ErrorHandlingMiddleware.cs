using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjeYonetim.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İstek boru hattındaki bir sonraki middleware'i çağır.
                // Eğer hata olmazsa, bu metot normal şekilde tamamlanır.
                await _next(context);
            }
            catch (Exception ex)
            {
                // Eğer boru hattının herhangi bir yerinde bir hata oluşursa,
                // catch bloğu çalışır.

                // 1. Hatayı logla.
                _logger.LogError(ex, "Beklenmedik bir hata oluştu: {Message}", ex.Message);

                // 2. Kullanıcıya döneceğimiz cevabı hazırla.
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 kodu

                // 3. Kullanıcıya gösterilecek temiz hata mesajı.
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = response.StatusCode,
                    message = "Sunucuda beklenmedik bir hata oluştu. Lütfen daha sonra tekrar deneyin."
                });

                // 4. Cevabı kullanıcıya gönder.
                await response.WriteAsync(result);
            }
        }
    }
}