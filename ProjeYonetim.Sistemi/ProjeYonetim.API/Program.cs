using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjeYonetim.API.Data;
using ProjeYonetim.API.Middleware;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;

// Serilog'un ilk yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ProjeYonetimApiLog-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information(">>> Uygulama Başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u ASP.NET Core loglama sistemine entegre et
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // --- SERVİSLER ---

    // 1. CORS Politikasını Tanımla
    var frontendOrigin = "http://localhost:8000";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowFrontend",
                          policy =>
                          {
                              policy.WithOrigins(frontendOrigin)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          });
    });

    // 2. DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 3. Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value)),
            ValidateIssuer = false, // Geliştirme için basitleştirildi
            ValidateAudience = false, // Geliştirme için basitleştirildi
            RoleClaimType = ClaimTypes.Role
        };
    });

    // 4. Controller'lar
    builder.Services.AddControllers();

    // 5. API Versiyonlama
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();

    // 6. Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Token'ı 'Bearer {token}' formatında girin",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] {} }
        });
    });

    var app = builder.Build();

    // --- MIDDLEWARE PİPELİNE (Sıralama Çok Önemli!) ---

    // 1. Kendi özel Hata Yakalayıcımız (her şeyden önce olmalı)
    app.UseMiddleware<ErrorHandlingMiddleware>();

    // 2. Serilog ile istekleri loglama
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // app.UseHttpsRedirection(); // Frontend http olduğu için bu kapalı kalmalı

    // 3. Routing'i etkinleştir (CORS'tan önce!)
    app.UseRouting();

    // 4. CORS politikasını uygula
    app.UseCors("AllowFrontend");

    // 5. Kimlik doğrulama
    app.UseAuthentication();

    // 6. Yetkilendirme
    app.UseAuthorization();

    // 7. Endpoint'leri haritala
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, ">>> Uygulama başlatılırken ölümcül bir hata oluştu!");
}
finally
{
    Log.CloseAndFlush();
}