using GestionVentas.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Servicios MVC y HTTP Context
// --------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

// ConexionDB local
builder.Services.AddSingleton<ConexionDB>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConexionDB(config); // Usa appsettings.json para localhost
});

// --------------------------------------------------
// Sesión
// --------------------------------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --------------------------------------------------
// Autenticación: Cookies + JWT
// --------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MiCookieAuth";
    options.DefaultAuthenticateScheme = "MiCookieAuth";
    options.DefaultChallengeScheme = "MiCookieAuth";
})
.AddCookie("MiCookieAuth", options =>
{
    options.LoginPath = "/Login/Index";
    options.AccessDeniedPath = "/Login/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
})
.AddJwtBearer("Bearer", options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "GestionVentas",
        ValidAudience = "GestionVentas",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("clave_super_secreta_para_token123!")
        )
    };
});

// --------------------------------------------------
// 🔹 Configuración de puerto
// --------------------------------------------------
// LOCALHOST: puerto 5000
//var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//builder.WebHost.UseUrls($"http://localhost:{port}");

// --------------------------------------------------
// Build de la app
// --------------------------------------------------
var app = builder.Build();

// --------------------------------------------------
// Middlewares
// --------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// Rutas MVC
// --------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Inicio}/{id?}"
);

// --------------------------------------------------
// Ejecutar
// --------------------------------------------------
app.Run();
