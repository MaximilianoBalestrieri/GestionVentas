using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para .Include()
using GestionVentas.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CajaController : Controller
{
    private readonly ConexionDB conexion;

    public CajaController(ConexionDB conexion)
    {
        this.conexion = conexion;
    }

    // 1. Pantalla principal: Muestra el estado actual, el resumen y movimientos
    public IActionResult Index()
    {
        var cajaAbierta = conexion.Cajas
            .FirstOrDefault(c => c.EstaAbierta);

        if (cajaAbierta == null)
        {
            return RedirectToAction("AbrirCaja");
        }

        // Cargamos los movimientos de la caja actual para mostrar en una tabla
        var movimientos = conexion.MovimientosCaja
            .Where(m => m.CajaId == cajaAbierta.Id)
            .OrderByDescending(m => m.Fecha)
            .ToList();

        // Calculamos el saldo acumulado en memoria para la vista
       // Usamos el Enum para filtrar correctamente
decimal ingresos = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso).Sum(m => m.Monto);
decimal egresos = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);
        
        ViewBag.SaldoActual = cajaAbierta.MontoInicial + ingresos - egresos;
        ViewBag.Movimientos = movimientos;

        return View(cajaAbierta);
    }

    // 2. GET: Formulario de Apertura
    [HttpGet]
    public IActionResult AbrirCaja()
    {
        // Si ya hay una caja abierta, no permitir abrir otra
        var existeCaja = conexion.Cajas.Any(c => c.EstaAbierta);
        if (existeCaja) return RedirectToAction("Index");

        return View();
    }

    // 3. POST: Procesar Apertura
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AbrirCaja(decimal montoInicial)
    {
        var nuevaCaja = new Caja
        {
            FechaApertura = DateTime.Now,
            MontoInicial = montoInicial,
            EstaAbierta = true,
            UsuarioId = "Admin", // Aquí podrías usar el ID del usuario logueado
            MontoEsperado = montoInicial
        };

        conexion.Cajas.Add(nuevaCaja);
        conexion.SaveChanges();

        return RedirectToAction("Index");
    }

    // 4. POST: Registrar un Ingreso o Egreso manual (ej: pago a proveedor)
   [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult NuevoMovimiento(decimal monto, string concepto, TipoMovimiento tipo) // Cambiado a TipoMovimiento
{
    var cajaActual = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);
    
    if (cajaActual != null)
    {
        var mov = new MovimientoCaja
        {
            CajaId = cajaActual.Id,
            Fecha = DateTime.Now,
            Monto = monto,
            Concepto = concepto,
            Tipo = tipo 
        };

        conexion.MovimientosCaja.Add(mov);
        conexion.SaveChanges();
    }

    return RedirectToAction("Index");
}

    // 5. GET: Formulario de Cierre (Arqueo)
   [HttpGet]
public IActionResult CerrarCaja()
{
    var caja = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);
    if (caja == null) return RedirectToAction("Index");

    // Obtenemos todos los movimientos de esta caja
    var movimientos = conexion.MovimientosCaja.Where(m => m.CajaId == caja.Id).ToList();
    
    // Calculamos el neto: Sumamos si es Ingreso, restamos si es Egreso
    decimal neto = movimientos.Sum(m => m.Tipo == TipoMovimiento.Ingreso ? m.Monto : -m.Monto);
    
    ViewBag.TotalEsperado = caja.MontoInicial + neto;

    return View(caja);
}

    // 6. POST: Procesar Cierre
    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CerrarCaja(decimal montoFisicoReal)
{
    var caja = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);

    if (caja != null)
    {
        var movimientos = conexion.MovimientosCaja.Where(m => m.CajaId == caja.Id).ToList();
        
        // Calculamos el neto correctamente antes de usarlo
        decimal neto = movimientos.Sum(m => m.Tipo == TipoMovimiento.Ingreso ? m.Monto : -m.Monto);

        caja.MontoEsperado = caja.MontoInicial + neto;
        caja.MontoFinalReal = montoFisicoReal;
        caja.FechaCierre = DateTime.Now;
        caja.EstaAbierta = false;

        conexion.Cajas.Update(caja);
        conexion.SaveChanges();
    }

    return RedirectToAction("Index");
}
}