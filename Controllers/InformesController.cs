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

    // 2. Traemos los movimientos, pero FILTRAMOS las aperturas de caja 
    // para que no ensucien la suma ni la lista si así lo deseas.
    var movimientos = conexion.MovimientosCaja
        .Where(m => m.Fecha.Date == diaFiltro.Date && !m.Concepto.Contains("Apertura"))
        .OrderBy(m => m.Fecha)
        .ToList();

    // 3. Calculamos totales netos de lo que pasó en las manos del cajero hoy
    decimal totalIngresos = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Ingreso)
        .Sum(m => m.Monto);

    decimal totalEgresos = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Egreso)
        .Sum(m => m.Monto);

    // 4. EL CAMBIO CLAVE: El total ahora es solo la diferencia de movimientos.
    // Ya no sumamos 'totalIniciales'.
    ViewBag.TotalFinalDia = totalIngresos - totalEgresos;

    // Cambiamos el título para la vista
    ViewBag.TituloInforme = "RESUMEN DE INGRESOS Y EGRESOS DEL DÍA";

    return View(movimientos);
}

        // --- TUS MÉTODOS ANTERIORES (SIN TOCAR) ---
       [HttpPost]
public IActionResult ObtenerVentasPorFecha(DateTime desde, DateTime hasta, string modo)
{
    List<object> lista = new List<object>();

    // Cambiamos el nombre de los métodos de la conexión para que busquen en MovimientosCaja
    if (modo == "diario")
    {
        // Este método debe sumar (Ingresos - Egresos) agrupado por día
        lista = conexion.ObtenerBalanceMovimientosPorFecha(desde, hasta);
    }
    else if (modo == "mensual")
    {
        lista = conexion.ObtenerBalanceMovimientosPorMes(desde, hasta);
    }
    else if (modo == "anual")
    {
        lista = conexion.ObtenerBalanceMovimientosPorAnio(desde, hasta);
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