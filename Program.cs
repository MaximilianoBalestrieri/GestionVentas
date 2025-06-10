using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregamos servicios MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Autenticación combinada: Cookies + JWT
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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "GestionVentas",
        ValidAudience = "GestionVentas",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_super_secreta_para_token123!"))
    };
});

var app = builder.Build();

// Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware para sesión y autenticación
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
