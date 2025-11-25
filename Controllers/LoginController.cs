using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

public class LoginController : Controller
{
    private readonly ConexionDB conexionDB;

    // Ajustado para funcionar local y en Render
    public LoginController(IConfiguration config)
    {
        conexionDB = new ConexionDB(config);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string usuario, string contraseña)
    {
        var user = conexionDB.BuscarUsuario(usuario, contraseña);

        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UsuarioNombre),
                new Claim(ClaimTypes.Role, user.Rol),
                new Claim("NombreyApellido", user.NombreyApellido),
                new Claim("FotoPerfil", user.FotoPerfil ?? "/imagenes/usuarios/default.png")
            };

            var identity = new ClaimsIdentity(claims, "MiCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MiCookieAuth", principal);

            HttpContext.Session.SetString("Usuario", user.UsuarioNombre);
            HttpContext.Session.SetString("NombreyApellido", user.NombreyApellido);
            HttpContext.Session.SetString("Rol", user.Rol);
            HttpContext.Session.SetString("FotoPerfil", user.FotoPerfil ?? "/imagenes/usuarios/default.png");

            return RedirectToAction("Index", "Home");
        }
        else
        {
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            ViewBag.Usuario = usuario;
            return View("Index");
        }
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MiCookieAuth");
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Login");
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
