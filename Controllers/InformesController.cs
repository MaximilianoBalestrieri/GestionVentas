using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{
    public class InformesController : Controller
    {
        private readonly ConexionDB conexion;

        public InformesController(ConexionDB conexion)
        {
            this.conexion = conexion;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ObtenerVentasPorFecha(DateTime desde, DateTime hasta, string modo)
        {
            List<object> lista = new List<object>();

            if (modo == "diario")
            {
                lista = conexion.ObtenerTotalVentasPorFecha(desde, hasta);
            }
            else if (modo == "mensual")
            {
                lista = conexion.ObtenerTotalVentasPorMes(desde, hasta);
            }
            else if (modo == "anual")
            {
                lista = conexion.ObtenerTotalVentasPorAnio(desde, hasta);
            }
            else
            {
                return BadRequest("Modo inválido");
            }

            return Json(lista);
        }

        public class FiltroFecha
        {
            public DateTime Desde { get; set; }
            public DateTime Hasta { get; set; }
        }
    }
}
