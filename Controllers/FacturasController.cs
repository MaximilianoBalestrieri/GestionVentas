using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    public class FacturasController : Controller
    {
        public IActionResult Index()
        {
            var conexion = new ConexionDB();
            ViewBag.Rol = HttpContext.Session.GetString("Rol");
            var facturas = conexion.ObtenerFacturas(); // este método lo hacemos ahora
            return View(facturas);
        }




        public IActionResult Detalles(int id)
        {
            ConexionDB conexion = new ConexionDB();
            var factura = conexion.ObtenerFacturaConItems(id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

// GET: Facturas/Eliminar/5
public ActionResult Eliminar(int id)
{
     ConexionDB conexion = new ConexionDB();
    var factura = conexion.ObtenerFacturaConItems(id); 
    if (factura == null)
    {
        return RedirectToAction("Index");

    }
    return View(factura);
}

        // POST: Facturas/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]

        public ActionResult EliminarConfirmado(int id)
        {
             ConexionDB conexion = new ConexionDB();
            conexion.EliminarFactura(id);
            return RedirectToAction("Index");
        }


    }

    }

