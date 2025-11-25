using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    public class PresupuestoController : Controller
    {
        private readonly ConexionDB db;

        // Constructor ajustado para DI + funcionamiento local y Render
        public PresupuestoController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

        // GET: Presupuesto
        public ActionResult Index()
        {
            var presupuestos = db.ObtenerPresupuestos();
            return View(presupuestos);
        }

        // GET: Presupuesto/Details/5
        public ActionResult Details(int id)
        {
            var presupuesto = db.ObtenerPresupuestoPorId(id);
            if (presupuesto == null)
            {
                return NotFound();
            }

            return View(presupuesto);
        }

        // GET: Presupuesto/Create
        public ActionResult Create()
        {
            var model = new Presupuesto
            {
                Fecha = DateTime.Now
            };

            return View(model);
        }

        // POST: Presupuesto/Create
        [HttpPost]
        public ActionResult Create(Presupuesto presupuesto)
        {
            try
            {
                presupuesto.Fecha = DateTime.Now;
                db.AgregarPresupuesto(presupuesto);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al guardar el presupuesto: " + ex.Message;
                return View(presupuesto);
            }
        }

        [HttpGet]
        public IActionResult ObtenerSiguienteNroPresupuesto()
        {
            int maxNro = db.ObtenerMaximoNroPresupuesto();
            return Json(maxNro + 1);
        }

        // GET: Presupuesto/Delete/5
        public ActionResult Delete(int id)
        {
            var presupuesto = db.ObtenerPresupuestoPorId(id);
            if (presupuesto == null)
            {
                return NotFound();
            }

            return View(presupuesto);
        }

        // POST: Presupuesto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                db.EliminarPresupuesto(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el presupuesto: " + ex.Message;
                return RedirectToAction("Delete", new { id });
            }
        }

        public JsonResult ObtenerProductos()
        {
            var productos = db.ObtenerProductos();

            var lista = productos.Select(p => new
            {
                id = p.IdProducto,
                nombre = p.Nombre,
                descripcion = p.Descripcion,
                precio = Math.Round(p.PrecioCosto * (1 + p.RecargoPorcentaje / 100), 2)
            }).ToList();

            return Json(lista);
        }
    }
}
