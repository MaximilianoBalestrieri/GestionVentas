using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;


public class LoginController : Controller
{
     private ConexionDB conexionDB = new ConexionDB();

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string usuario, string contraseña)
    {
        var user = conexionDB.BuscarUsuario(usuario, contraseña);

        if (user != null)
        {
            HttpContext.Session.SetString("Usuario", user.UsuarioNombre);
            HttpContext.Session.SetString("NombreyApellido", user.NombreyApellido);
            HttpContext.Session.SetString("Rol", user.Rol);
            HttpContext.Session.SetString("FotoPerfil", user.FotoPerfil ?? "/imagenes/usuarios/default.png");


           return RedirectToAction("Index", "Home");

        }
        else
        {
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            ViewBag.Usuario = usuario; // <-- guarda lo que escribió para volver a mostrarlo
            return View("index","Login");
        }
    }

    public IActionResult Logout()
{
    HttpContext.Session.Clear();  // Limpiar la sesión
    
    return RedirectToAction("Index", "Login");  // Redirigir al login
}

[HttpGet]
public IActionResult Index()
{
    return View();
}

}
