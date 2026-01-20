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
    var cajaAbierta = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);

    if (cajaAbierta == null)
    {
        return RedirectToAction("AbrirCaja");
    }

    var movimientos = conexion.MovimientosCaja
        .Where(m => m.CajaId == cajaAbierta.Id)
        .OrderByDescending(m => m.Fecha)
        .ToList();

    // --- CÁLCULOS FILTRADOS ---

    // 1. EFECTIVO (Verde)
    decimal ingresosEfectivo = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Efectivo)"))
        .Sum(m => m.Monto);

    decimal egresosEfectivo = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Efectivo)"))
        .Sum(m => m.Monto);

    // 2. TRANSFERENCIA (Azul)
    decimal ingresosTransferencia = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Transferencia)"))
        .Sum(m => m.Monto);

    decimal egresosTransferencia = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Transferencia)"))
        .Sum(m => m.Monto);

    // --- ASIGNACIÓN A VIEWBUG ---

    // Efectivo: Inicial + Entradas Efectivo - Salidas Efectivo
    ViewBag.TotalEfectivo = cajaAbierta.MontoInicial + ingresosEfectivo - egresosEfectivo;
    
    // Transferencia: Entradas Transf - Salidas Transf
    ViewBag.TotalTransferencia = ingresosTransferencia - egresosTransferencia;

    // Saldo Total (La suma de lo que hay en ambos "bolsillos")
    ViewBag.SaldoActual = ViewBag.TotalEfectivo + ViewBag.TotalTransferencia;
    
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

    // 5. GET: Formulario de Cierre (Arqueo)
  [HttpGet]
public IActionResult CerrarCaja()
{
    // Buscamos la caja que está abierta actualmente
    var caja = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);

    if (caja == null)
    {
        return RedirectToAction("Index");
    }

    // Traemos todos los movimientos de ESTA caja
    var movimientos = conexion.MovimientosCaja
        .Where(m => m.CajaId == caja.Id)
        .ToList();

    // Calculamos subtotales de EFECTIVO
    decimal ingresosEfectivo = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Efectivo)"))
        .Sum(m => m.Monto);

    decimal egresosEfectivo = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Efectivo)"))
        .Sum(m => m.Monto);

    // Calculamos subtotales de TRANSFERENCIA
    decimal ingresosTransf = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Transferencia)"))
        .Sum(m => m.Monto);

    decimal egresosTransf = movimientos
        .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Transferencia)"))
        .Sum(m => m.Monto);

    // Enviamos los datos a la vista mediante ViewBag
    ViewBag.TotalEsperadoEfectivo = caja.MontoInicial + ingresosEfectivo - egresosEfectivo;
    ViewBag.TotalEsperadoTransferencia = ingresosTransf - egresosTransf;

    return View(caja);
}
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CerrarCaja(decimal montoFisicoReal)
{
    var caja = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);

    if (caja != null)
    {
        var movimientos = conexion.MovimientosCaja.Where(m => m.CajaId == caja.Id).ToList();
        
        // 1. Calculamos el esperado de EFECTIVO (para comparar contra lo que el usuario contó)
        decimal ingresosEfectivo = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Efectivo)")).Sum(m => m.Monto);
        decimal egresosEfectivo = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Efectivo)")).Sum(m => m.Monto);
        decimal esperadoEfectivo = caja.MontoInicial + ingresosEfectivo - egresosEfectivo;

        // 2. Calculamos el esperado de TRANSFERENCIAS
        decimal ingresosTransf = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Transferencia)")).Sum(m => m.Monto);
        decimal egresosTransf = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Transferencia)")).Sum(m => m.Monto);
        decimal esperadoTransf = ingresosTransf - egresosTransf;

        // 3. Guardamos los valores en el modelo de Caja
        // MontoEsperado: Es el total bruto que DEBERÍA haber sumando ambos medios
        caja.MontoEsperado = esperadoEfectivo + esperadoTransf;
        
        // MontoFinalReal: Guardamos lo que el usuario contó físicamente + lo que ya sabemos que hay en el banco
        // Así, la diferencia final de la caja se basará solo en el error de conteo del efectivo.
        caja.MontoFinalReal = montoFisicoReal + esperadoTransf;
        
        caja.FechaCierre = DateTime.Now;
        caja.EstaAbierta = false;

        conexion.Cajas.Update(caja);
        conexion.SaveChanges();
    }

    return RedirectToAction("Index");
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult NuevoMovimiento(decimal monto, string concepto, TipoMovimiento tipo, string medioPago)
{
    var cajaActual = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);
    
    if (cajaActual != null)
    {
        // Concatenamos el medio de pago para que las tarjetas de la vista funcionen
        string conceptoFinal = $"{concepto} ({medioPago})";

        var mov = new MovimientoCaja
        {
            CajaId = cajaActual.Id,
            Fecha = DateTime.Now,
            Monto = monto,
            Concepto = conceptoFinal, 
            Tipo = tipo 
        };

        conexion.MovimientosCaja.Add(mov);

        // Actualizamos el MontoEsperado general de la caja
        if (tipo == TipoMovimiento.Ingreso)
            cajaActual.MontoEsperado += monto;
        else
            cajaActual.MontoEsperado -= monto;

        conexion.Cajas.Update(cajaActual);
        conexion.SaveChanges();
    }

    return RedirectToAction("Index");
}
}