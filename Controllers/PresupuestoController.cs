using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{

    public class PresupuestoController : Controller
    {
        ConexionDB db = new ConexionDB();

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
        return View(new Presupuesto());
    }

    // POST: Presupuesto/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Presupuesto presupuesto)
    {
        if (!ModelState.IsValid)
        {
            return View(presupuesto);
        }

        try
        {
            db.AgregarPresupuesto(presupuesto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al guardar el presupuesto: " + ex.Message);
            return View(presupuesto);
        }
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

    }
}