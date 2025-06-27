var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// wwwroot klasöründeki dosyaların (index.html, app.js vb.)
// sunulmasını sağlar.
app.UseStaticFiles();

// Proje çalıştığında varsayılan olarak index.html'i açar.
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();