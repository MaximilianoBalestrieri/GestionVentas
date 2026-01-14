using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // --- NUEVA VISTA DE MOVIMIENTOS DIARIOS ---
        public IActionResult MovimientosDiarios(DateTime? fecha)
{
    // 1. Filtro de fecha (hoy si viene nulo)
    DateTime diaFiltro = fecha ?? DateTime.Today;
    ViewBag.FechaActual = diaFiltro.ToString("yyyy-MM-dd");

    // 2. Traemos TODOS los movimientos que ocurrieron en esa fecha
    // Eliminamos la dependencia de 'cajasIds' para que si hay una factura
    // de una caja de ayer, también aparezca.
    var movimientos = conexion.MovimientosCaja
        .Where(m => m.Fecha.Date == diaFiltro.Date)
        .OrderBy(m => m.Fecha)
        .ToList();

    // 3. Obtenemos los montos iniciales de las cajas que estuvieron activas hoy
    decimal totalIniciales = conexion.Cajas
        .Where(c => c.FechaApertura.Date == diaFiltro.Date)
        .Sum(c => (decimal?)c.MontoInicial) ?? 0;

    // 4. Totales basados en los movimientos encontrados
    decimal totalIngresos = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Ingreso)
        .Sum(m => m.Monto);

    decimal totalEgresos = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Egreso)
        .Sum(m => m.Monto);

    // Saldo final = Inicial + Ventas/Ingresos - Gastos
    ViewBag.TotalFinalDia = totalIniciales + totalIngresos - totalEgresos;

    return View(movimientos);
}

        // --- TUS MÉTODOS ANTERIORES (SIN TOCAR) ---
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