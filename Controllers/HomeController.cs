using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using Microsoft.AspNetCore.Http;

namespace GestionVentas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConexionDB db = new ConexionDB();

        public IActionResult Index()
        {
            var usuario = HttpContext.Session.GetString("Usuario"); // ojo, debe ser igual a como guardás en LoginController
            var nombreyApellido = HttpContext.Session.GetString("NombreyApellido");
            var rol = HttpContext.Session.GetString("Rol");
            var fotoPerfil = HttpContext.Session.GetString("FotoPerfil");

            if (string.IsNullOrEmpty(usuario))
            {
                // Si no hay sesión iniciada, redirige al login
                return RedirectToAction("Index", "Login");
            }

            // Pasamos datos a la vista
            ViewBag.Usuario = usuario;
            ViewBag.NombreyApellido = nombreyApellido;
            ViewBag.Rol = rol;
            ViewBag.FotoPerfil = fotoPerfil;

            return View();
        }

        public IActionResult Perfil()
        {
            var usuarioNombre = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(usuarioNombre))
            {
                return RedirectToAction("Index", "Login");
            }

            var usuario = db.ObtenerUsuarioPorNombre(usuarioNombre);
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Usuario = usuario.UsuarioNombre;
            ViewBag.NombreyApellido = usuario.NombreyApellido;
            ViewBag.Rol = usuario.Rol;
            ViewBag.FotoPerfil = usuario.FotoPerfil ?? "/imagenes/usuarios/default.png";

            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Descripción del Software.";
            return View();
        }
    }
}
