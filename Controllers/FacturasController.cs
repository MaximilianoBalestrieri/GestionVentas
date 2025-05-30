using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    public class FacturasController : Controller
    {
        public IActionResult Index()
        {
            var conexion = new ConexionDB();
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


    }

    }

