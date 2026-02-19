using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http; // Para usar Context.Session
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionVentas.Controllers
{
    public class CtaCteController : Controller
    {
        private readonly ConexionDB db;

        public CtaCteController(IConfiguration config)
        {
            this.db = new ConexionDB(config);
        }

        // 1. Detalle de la Libreta de Cuenta Corriente
    public IActionResult Index(int idCliente)
{
    // 1. Buscamos al cliente
    var cliente = db.ObtenerClienteCtaCtePorId(idCliente);
    if (cliente == null) return RedirectToAction("Index", "ClientesCtaCte");

    // 2. Traemos sus movimientos (aquellos que son DEUDAS, no pagos)
    List<MovimientoCtaCte> historial = db.ObtenerHistorialCtaCte(idCliente);

    // 3. RECALCULO DE DEUDA EN TIEMPO REAL
    decimal totalActualizado = 0;

    foreach (var mov in historial)
    {
        // Solo recalculamos si es un producto (Monto > 0) y tiene un IdProducto asociado
        if (mov.IdProducto > 0) 
        {
            var prodActual = db.ObtenerProductoPorId(mov.IdProducto);
            if (prodActual != null)
            {
                // Actualizamos el monto del movimiento en la lista para que la vista lo vea nuevo
                mov.Monto = prodActual.PrecioVenta * mov.Cantidad;
            }
        }
        totalActualizado += mov.Monto;
    }

    // 4. GUARDAR EL NUEVO SALDO EN LA BASE DE DATOS (Para que en el listado general salga bien)
    db.SobreescribirSaldoCliente(idCliente, totalActualizado);

    // 5. PASAR LOS DATOS A LA VISTA
    ViewBag.NombreCliente = cliente.NombreCliente;
    ViewBag.IdCliente = idCliente;
    ViewBag.TotalDeuda = totalActualizado; 
    ViewBag.ListaProductos = db.ObtenerProductos();

    return View(historial); 
}


[HttpGet]
public JsonResult BuscarProductoPorCodigo(string codigo)
{
    var producto = db.ObtenerProductoPorCodigo(codigo); // Asegurate de tener este método en ConexionDB
    if (producto != null)
    {
        return Json(new { 
            success = true, 
            id = producto.IdProducto, 
            nombre = producto.Nombre, 
            precio = producto.PrecioVenta 
        });
    }
    return Json(new { success = false });
}
        // 2. Acción para agregar un producto a la libreta
        [HttpPost]
        public IActionResult AgregarItemCuenta(int idCliente, int idProducto, int cantidad, string vendedor)
        {
            if (idCliente > 0 && idProducto > 0 && cantidad > 0)
            {
                var producto = db.ObtenerProductoPorId(idProducto);
                if (producto != null)
                {
                    decimal totalItem = producto.PrecioVenta * cantidad;
                    
                    // Creamos un detalle claro para la base de datos
                    // Si tu SQL da error en 'resumenProductos', es porque el SP o el INSERT
                    // espera un nombre de columna distinto. Aquí mandamos el string:
                    string detalleParaDB = $"{producto.Nombre} (x{cantidad})";

                    // REGISTRAMOS EL GASTO
                    // Asegurate que en ConexionDB, el método use @idCli, @monto, @vendedor, @detalle
                    db.InsertarGastoCtaCte(idCliente, totalItem, vendedor, detalleParaDB);
                    
                    // IMPORTANTE: También deberías insertar el FacturaItem correspondiente 
                    // si querés que aparezca en el desglose de la tabla.
                }
            }
            return RedirectToAction("Index", new { idCliente = idCliente });
        }

        // 3. Liquidación total de la cuenta
        [HttpPost]
        public JsonResult CobrarCuentaCompleta(int idCliente, decimal totalADebitar)
        {
            try 
            {
                int idCaja = db.ObtenerIdCajaAbierta();
                
                if (idCaja == 0)
                    return Json(new { success = false, message = "No hay una caja abierta." });

                var resultado = db.LiquidarCuentaCorriente(idCliente, totalADebitar, idCaja);

                return Json(new { success = resultado.success, message = resultado.error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


[HttpPost]
public IActionResult GuardarVentaCtaCte(int idCliente, List<int> ids, List<int> cants)
{
    if (ids == null || ids.Count == 0) return RedirectToAction("Index", new { idCliente });

    decimal totalVentaGeneral = 0;

    for (int i = 0; i < ids.Count; i++)
    {
        var p = db.ObtenerProductoPorId(ids[i]);
        if (p != null)
        {
            decimal subtotalItem = p.PrecioVenta * cants[i];
            totalVentaGeneral += subtotalItem;
            
            // 1. Registramos en el historial (lo que ya hicimos)
            string conceptoIndividual = $"{p.Nombre} (x{cants[i]})";
            db.RegistrarEnHistorialCtaCte(idCliente, conceptoIndividual, subtotalItem, p.IdProducto, cants[i]);

            // 2. NUEVO: Restamos del stock de la tabla Productos
            db.RestarStockProducto(p.IdProducto, cants[i]);
        }
    }

    // Actualizamos el saldo total del cliente
    db.ActualizarSaldoClienteCtaCte(idCliente, totalVentaGeneral);

    return RedirectToAction("Index", new { idCliente = idCliente });
}
    }
}