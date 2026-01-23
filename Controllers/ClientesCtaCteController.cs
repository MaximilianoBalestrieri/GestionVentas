using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Linq;
using System;

public class ClientesCtaCteController : Controller
{
    private readonly ConexionDB db;

    public ClientesCtaCteController(ConexionDB conexion)
    {
        this.db = conexion;
    }

    // Listado de clientes que tienen saldo pendiente
    public IActionResult Index()
    {
        // Traemos solo los que deben dinero (Saldo > 0)
        var deudores = db.Clientes
            .Where(c => c.SaldoActual > 0)
            .OrderByDescending(c => c.SaldoActual)
            .ToList();

        return View(deudores);
    }

    // Proceso para cobrar una deuda
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CobrarDeuda(int idCliente, decimal montoPagado, string medioPago)
    {
        var cajaAbierta = db.Cajas.FirstOrDefault(c => c.EstaAbierta);
        
        if (cajaAbierta == null)
        {
            TempData["Error"] = "No se puede cobrar sin una caja abierta.";
            return RedirectToAction("Index");
        }

        if (montoPagado <= 0) return RedirectToAction("Index");

        // 1. Actualizar el saldo en la tabla Clientes
        var cliente = db.Clientes.Find(idCliente);
        if (cliente != null)
        {
            cliente.SaldoActual -= montoPagado;
            db.Clientes.Update(cliente);

            // 2. Crear el movimiento en la caja para que sume al efectivo/transf
            string etiqueta = medioPago == "Transferencia" ? "(Transferencia)" : "(Efectivo)";
            var mov = new MovimientoCaja
            {
                CajaId = cajaAbierta.Id,
                Fecha = DateTime.Now,
                Monto = montoPagado,
                Concepto = $"Cobro CtaCte - {cliente.NombreCliente} {etiqueta}",
                Tipo = TipoMovimiento.Ingreso // Es un ingreso de dinero
            };

            db.MovimientosCaja.Add(mov);

            // 3. Actualizar el esperado de la caja
            cajaAbierta.MontoEsperado += montoPagado;

            db.SaveChanges();
            TempData["Success"] = "Pago registrado correctamente.";
        }

        return RedirectToAction("Index");
    }
}