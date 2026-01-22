using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionVentas.Models;
using System;
using System.Linq;
using System.Collections.Generic;

public class CajaController : Controller
{
    private readonly ConexionDB conexion;

    public CajaController(ConexionDB conexion)
    {
        this.conexion = conexion;
    }

    // 1. Pantalla principal
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
        decimal ingresosEfectivo = movimientos
            .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Efectivo)"))
            .Sum(m => m.Monto);

        decimal egresosEfectivo = movimientos
            .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Efectivo)"))
            .Sum(m => m.Monto);

        decimal ingresosTransferencia = movimientos
            .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Transferencia)"))
            .Sum(m => m.Monto);

        decimal egresosTransferencia = movimientos
            .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Transferencia)"))
            .Sum(m => m.Monto);

        // --- ASIGNACIÓN A VIEWBUG ---
        ViewBag.TotalEfectivo = cajaAbierta.MontoInicial + ingresosEfectivo - egresosEfectivo;
        ViewBag.TotalTransferencia = ingresosTransferencia - egresosTransferencia;
        ViewBag.SaldoActual = ViewBag.TotalEfectivo + ViewBag.TotalTransferencia;
        ViewBag.Movimientos = movimientos;

        return View(cajaAbierta);
    }

    // 2. GET: Formulario de Apertura
    [HttpGet]
    public IActionResult AbrirCaja()
    {
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
            UsuarioId = "Admin",
            MontoEsperado = montoInicial
        };

        conexion.Cajas.Add(nuevaCaja);
        conexion.SaveChanges();

        return RedirectToAction("Index");
    }

    // 4. POST: Registrar un Ingreso o Egreso manual
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoMovimiento(decimal monto, string concepto, TipoMovimiento tipo, string medioPago)
    {
        var cajaActual = conexion.Cajas.FirstOrDefault(c => c.EstaAbierta);
        
        if (cajaActual != null)
        {
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

            if (tipo == TipoMovimiento.Ingreso)
                cajaActual.MontoEsperado += monto;
            else
                cajaActual.MontoEsperado -= monto;

            conexion.Cajas.Update(cajaActual);
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

        var movimientos = conexion.MovimientosCaja.Where(m => m.CajaId == caja.Id).ToList();

        decimal ingEf = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Efectivo)")).Sum(m => m.Monto);
        decimal egEf = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Efectivo)")).Sum(m => m.Monto);
        decimal ingTr = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Transferencia)")).Sum(m => m.Monto);
        decimal egTr = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Transferencia)")).Sum(m => m.Monto);

        ViewBag.TotalEsperadoEfectivo = caja.MontoInicial + ingEf - egEf;
        ViewBag.TotalEsperadoTransferencia = ingTr - egTr;

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
            
            decimal ingEf = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Efectivo)")).Sum(m => m.Monto);
            decimal egEf = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Efectivo)")).Sum(m => m.Monto);
            decimal espEf = caja.MontoInicial + ingEf - egEf;

            decimal ingTr = movimientos.Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Concepto.Contains("(Transferencia)")).Sum(m => m.Monto);
            decimal egTr = movimientos.Where(m => m.Tipo == TipoMovimiento.Egreso && m.Concepto.Contains("(Transferencia)")).Sum(m => m.Monto);
            decimal espTr = ingTr - egTr;

            caja.MontoEsperado = espEf + espTr;
            caja.MontoFinalReal = montoFisicoReal + espTr; 
            
            caja.FechaCierre = DateTime.Now;
            caja.EstaAbierta = false;

            conexion.Cajas.Update(caja);
            conexion.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}