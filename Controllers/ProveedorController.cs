using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    [Route("Proveedores")]
    public class ProveedoresController : Controller
    {
        private ConexionDB conexion = new ConexionDB();

        public IActionResult Index()
{
    return View();
}

        [HttpGet("Obtener")]
        public JsonResult Obtener()
        {
            var lista = conexion.ObtenerProveedores();
            return Json(lista);
        }

        [HttpPost("Crear")]
        public JsonResult Crear([FromBody] Proveedor prov)
        {
            Console.WriteLine($"Creando proveedor: {prov.Nombre}, {prov.Telefono}");
            conexion.AgregarProveedor(prov);
            return Json(new { ok = true });
        }

        [HttpPost("Editar")] // 
        public JsonResult Editar([FromBody] Proveedor prov)
        {
            conexion.ActualizarProveedor(prov);
            return Json(new { ok = true });
        }

        [HttpPost("Eliminar")]  
        public JsonResult Eliminar([FromBody] int id)
        {
            conexion.EliminarProveedor(id);
            return Json(new { ok = true });
        }
    }
}