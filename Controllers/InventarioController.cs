using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ConexionDB conexion;

        public InventarioController(ConexionDB conexion)
        {
            this.conexion = conexion;
        }

        public IActionResult Index()
        {
            var productos = conexion.ObtenerProductos();
            return View(productos);
        }
    }
}
