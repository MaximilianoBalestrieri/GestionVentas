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
            // Buscamos los datos en la tabla de clientes general
            var clienteGeneral = db.ObtenerClienteGeneralPorId(idClienteOriginal);
            
            if (clienteGeneral != null)
            {
                // Insertamos en la tabla ClientesCtaCte con saldo 0
                db.InsertarClienteCtaCte(clienteGeneral.NombreCliente, clienteGeneral.TelefonoCliente);
                TempData["Success"] = "Cliente habilitado para cuenta corriente.";
            }

            return RedirectToAction("Index");
        }

        // Proceso para cobrar una deuda (Corregido para usar ConexionDB)
        [HttpPost]
        public IActionResult CobrarDeuda(int idCliente, decimal montoPagado, string medioPago)
        {
            try 
            {
                int idCaja = db.ObtenerIdCajaAbierta();
                
                if (idCaja == 0)
                {
                    TempData["Error"] = "No hay una caja abierta.";
                    return RedirectToAction("Index");
                }

                if (montoPagado <= 0) return RedirectToAction("Index");

                // Registramos el pago usando los métodos de tu ConexionDB
                // 1. Registramos el ingreso en la tabla HistorialCtaCte (como un Pago)
                db.RegistrarPagoEnHistorial(idCliente, montoPagado, medioPago);

                // 2. Restamos el saldo al cliente
                db.ActualizarSaldoPorPago(idCliente, montoPagado);

                // 3. Registramos el movimiento en la caja abierta
                string concepto = $"Cobro CtaCte - ID:{idCliente} ({medioPago})";
                
                db.RegistrarMovimientoCaja(idCaja, montoPagado, concepto, "Ingreso");

                TempData["Success"] = "Pago registrado y caja actualizada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}