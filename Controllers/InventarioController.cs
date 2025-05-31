using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ConexionDB conexion = new ConexionDB();

        public IActionResult Index()
        {
        
            var productos = conexion.ObtenerProductos();
            return View(productos);
        }

    }
}