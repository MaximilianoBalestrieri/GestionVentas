using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;

namespace GestionVentas.Controllers
{
    [Route("Proveedores")]
    public class ProveedoresController : Controller
    {
        private readonly ConexionDB db;

        public ProveedoresController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

        public IActionResult Index() => View();

        [HttpGet("Obtener")]
        public JsonResult Obtener() => Json(db.ObtenerProveedores());

        [HttpPost("Crear")]
        public JsonResult Crear([FromBody] Proveedor prov)
        {
            db.AgregarProveedor(prov);
            return Json(new { ok = true });
        }

        [HttpPost("Editar")]
        public JsonResult Editar([FromBody] Proveedor prov)
        {
            db.ActualizarProveedor(prov);
            return Json(new { ok = true });
        }

        [HttpPost("Eliminar")]
        public JsonResult Eliminar([FromBody] int id)
        {
            db.EliminarProveedor(id);
            return Json(new { ok = true });
        }
    }
}
