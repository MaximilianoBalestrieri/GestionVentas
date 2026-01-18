using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;

namespace GestionVentas.Controllers
{
    // Quitamos el [Route] genérico de aquí para que use el ruteo convencional o lo definimos bien
    public class ProveedoresController : Controller
    {
        private readonly ConexionDB db;

        public ProveedoresController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

        // Acceso: /Proveedores o /Proveedores/Index
        public IActionResult Index() => View();

        [HttpGet]
        [Route("Proveedores/Obtener")]
        public JsonResult Obtener() => Json(db.ObtenerProveedores());

        [HttpPost]
        [Route("Proveedores/Crear")]
        public JsonResult Crear([FromBody] Proveedor prov)
        {
            db.AgregarProveedor(prov);
            return Json(new { ok = true });
        }

        [HttpPost]
        [Route("Proveedores/Editar")]
        public JsonResult Editar([FromBody] Proveedor prov)
        {
            db.ActualizarProveedor(prov);
            return Json(new { ok = true });
        }

        [HttpPost]
        [Route("Proveedores/Eliminar")]
        public JsonResult Eliminar([FromBody] int id)
        {
            db.EliminarProveedor(id);
            return Json(new { ok = true });
        }

        // --- SECCIÓN DE ACTUALIZACIÓN DE PRECIOS ---

        [HttpGet]
        [Route("Proveedores/ActualizarPrecios")] // Ruta explícita para que no choque con Index
        public IActionResult ActualizarPrecios()
        {
            ViewBag.Proveedores = db.ObtenerProveedores();
            return View();
        }

        [HttpPost]
        [Route("Proveedores/ActualizarPrecios")]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarPrecios(string nombreProveedor, string porcentajeAumento)
        {
            try
            {
                var culturaArg = new System.Globalization.CultureInfo("es-AR");
                if (decimal.TryParse(porcentajeAumento, System.Globalization.NumberStyles.Any, culturaArg, out decimal porcentajeLimpio))
                {
                    int cantidadAfectados = db.ActualizarPreciosPorProveedor(nombreProveedor, porcentajeLimpio);
                    TempData["Mensaje"] = $"Se actualizaron los precios de {cantidadAfectados} productos del proveedor {nombreProveedor}.";
                }
                else
                {
                    ViewBag.Error = "El formato del porcentaje no es válido.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar precios: " + ex.Message;
            }

            ViewBag.Proveedores = db.ObtenerProveedores();
            return View();
        }
    }
}