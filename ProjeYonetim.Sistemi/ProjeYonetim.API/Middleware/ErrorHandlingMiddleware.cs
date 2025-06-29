using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
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
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmedik bir hata oluştu: {Message}", ex.Message);

                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Geliştirme ortamındaysak detaylı hata mesajı, değilsek genel mesaj gösterelim.
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = response.StatusCode,
                    message = _env.IsDevelopment()
                        ? $"Internal Server Error: {ex.Message}"
                        : "Sunucuda beklenmedik bir hata oluştu.",
                    details = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null
                });

                await response.WriteAsync(result);
            }
        }
    }
}