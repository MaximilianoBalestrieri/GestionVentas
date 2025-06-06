using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;


public class LoginController : Controller
{
    private ConexionDB conexionDB = new ConexionDB();

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
            // 1. Creamos la identidad con claims (datos del usuario)
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UsuarioNombre),
            new Claim(ClaimTypes.Role, user.Rol),
            new Claim("NombreyApellido", user.NombreyApellido),
            new Claim("FotoPerfil", user.FotoPerfil ?? "/imagenes/usuarios/default.png")
        };

            var identity = new ClaimsIdentity(claims, "MiCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // 2. Creamos la cookie de autenticación
            await HttpContext.SignInAsync("MiCookieAuth", principal);

            // 3. También guardamos en sesión (opcional si usás ViewBag/HttpContext.User)
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


    public async Task<IActionResult> Logout() // Borra la cookie de autenticación y limpia toda la sesión
    {
        // Limpiamos la cookie de autenticación
        await HttpContext.SignOutAsync("MiCookieAuth");

        // Limpiamos la sesión
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Login");
    }


    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

[HttpGet]
public IActionResult AccesoDenegado()  // empezamos la pagina del acceso denegado para que no salga la pagina fea
{
    return View();
}


}
