using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GestionVentas.Controllers
{
    public class ClientesCtaCteController : Controller
    {
        private readonly ConexionDB db;

        // Cambiamos el constructor para asegurarnos de que use tu clase de conexión
        public ClientesCtaCteController(ConexionDB conexion)
        {
            this.db = conexion;
        }

        // Listado principal
        public IActionResult Index()
        {
            // 1. Traemos los clientes que ya están en Cuenta Corriente (Deudores)
            // Usamos el método que creamos en ConexionDB
            List<Cliente> deudores = db.ObtenerClientesCtaCte();

            // 2. Traemos TODOS los clientes de la base general para el Modal
            // Esto permite seleccionar uno existente y pasarlo a CtaCte
            var todosLosClientes = db.ObtenerClientesGenerales();

            // Filtramos para que en el modal no aparezcan los que ya tienen cuenta corriente
            ViewBag.TodosLosClientes = todosLosClientes
                .Where(c => !deudores.Any(d => d.NombreCliente == c.NombreCliente))
                .ToList();

            return View(deudores);
        }

        // NUEVA ACCIÓN: Habilita un cliente existente para usar Cuenta Corriente
       [HttpPost]
public IActionResult HabilitarCtaCte(int idClienteOriginal)
{
    var clienteGeneral = db.ObtenerClienteGeneralPorId(idClienteOriginal);
    
    if (clienteGeneral != null)
    {
        // Pasamos el ID original para que queden vinculados desde el segundo cero
        db.InsertarClienteCtaCte(clienteGeneral.NombreCliente, clienteGeneral.TelefonoCliente, idClienteOriginal);
        TempData["Success"] = "Cliente habilitado para cuenta corriente.";
    }

    return RedirectToAction("Index");
}

[HttpPost]
public IActionResult CobrarDeuda(int idCliente, decimal montoPagado, string medioPago)
{
    try 
    {
        int idCaja = db.ObtenerIdCajaAbierta();
        if (idCaja == 0) {
            TempData["Error"] = "Caja cerrada.";
            return RedirectToAction("Index", new { idCliente });
        }

        var cliente = db.ObtenerClienteCtaCtePorId(idCliente);
        if (cliente == null || cliente.IdClienteOriginal == 0) {
            TempData["Error"] = "Cliente no vinculado.";
            return RedirectToAction("Index", new { idCliente });
        }

        int idMaestro = cliente.IdClienteOriginal;

        // 2. REGISTRO EN HISTORIAL (Libreta)
        // Ya incluimos el medio de pago aquí para la tabla del cliente
        db.RegistrarEnHistorialCtaCte(idMaestro, $"PAGO - {medioPago.ToUpper()}", -montoPagado, 0, 0);

        // 3. ACTUALIZAR SALDO
        db.ActualizarSaldoClienteCtaCte(idCliente, -montoPagado);

        // 4. REGISTRO EN CAJA (MovimientosCaja)
        // MODIFICADO: Agregamos el medio de pago al concepto que verás en la tabla de Caja
        string conceptoCaja = $"COBRO CTA CTE - {cliente.NombreCliente} ({medioPago.ToUpper()})";
        
        db.RegistrarMovimientoEnCajaReal(idCaja, conceptoCaja, montoPagado, 0, User.Identity.Name ?? "Cajero", medioPago);

        TempData["Success"] = "¡Cobro registrado!";
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Error: " + ex.Message;
    }
    return RedirectToAction("Index", new { idCliente });
}

    }
}