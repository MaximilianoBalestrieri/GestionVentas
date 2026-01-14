using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GestionVentas.Controllers
{
    public class VentasController : Controller
    {
        private readonly ConexionDB db;

        public VentasController(IConfiguration config)
        {
            // Mantenemos tu forma de instanciar la base de datos
            db = new ConexionDB(config);
        }

        // --- FUNCIÓN PRIVADA PARA REGISTRAR EL DINERO EN LA CAJA ---
        private void RegistrarIngresoEnCaja(decimal monto, int idFactura)
        {
            try
            {
                // Buscamos si hay una caja abierta en este momento
                var cajaAbierta = db.Cajas.FirstOrDefault(c => c.EstaAbierta);

                if (cajaAbierta != null)
                {
                    var movimiento = new MovimientoCaja
                    {
                        CajaId = cajaAbierta.Id,
                        Fecha = DateTime.Now,
                        Concepto = "Venta Factura Nro: " + idFactura.ToString("D8"),
                        Monto = monto,
                        Tipo = TipoMovimiento.Ingreso // Enum definido en Caja.cs
                    };

                    db.MovimientosCaja.Add(movimiento);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                // Opcional: Loguear error, pero no detener la venta por un error en caja
            }
        }

        // GET: Crear venta
        public ActionResult Create()
        {
            // VERIFICACIÓN DE CAJA:
            // Buscamos si existe alguna caja abierta
            var cajaAbierta = db.Cajas.FirstOrDefault(c => c.EstaAbierta);

            if (cajaAbierta == null)
            {
                // Si no hay caja abierta, guardamos un mensaje para mostrar en la otra pantalla
                TempData["MensajeError"] = "DEBE ABRIR CAJA PRIMERO PARA PODER VENDER.";
                return RedirectToAction("AbrirCaja", "Caja");
            }

            var productos = db.ObtenerProductos() ?? new List<Productos>();
            ViewBag.Vendedor = User.Identity.Name ?? "Usuario Desconocido";
            return View(productos);
        }

        // POST: Abonar productos
        [HttpPost]
        public JsonResult Abonar([FromBody] List<ProductoVenta> productos)
        {
            try
            {
                foreach (var p in productos)
                    db.RestarStock(p.idProducto, p.cantidad);

                return Json(new { exito = true });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, error = ex.Message });
            }
        }

        // POST: Restar stock y registrar venta (Sincronizado con Caja)
        [HttpPost]
        public JsonResult RestarStockYRegistrar([FromBody] VentaConProductos datos)
        {
            try
            {
                if (datos == null || datos.Productos == null || datos.Productos.Count == 0)
                    throw new Exception("Datos incompletos");

                var stocksActualizados = new List<object>();

                foreach (var p in datos.Productos)
                {
                    if (p == null) throw new Exception("Uno de los productos es nulo");

                    db.RestarStock(p.IdProducto, p.Cantidad);
                    int stockActual = db.ObtenerStockActual(p.IdProducto);
                    stocksActualizados.Add(new { IdProducto = p.IdProducto, StockDisponible = stockActual });
                }

                Venta venta = new Venta { DiaVenta = DateTime.Today, MontoVenta = datos.MontoVenta };
                int nuevoIdFactura = db.GuardarVenta(venta);

                // IMPACTO EN CAJA
                RegistrarIngresoEnCaja(datos.MontoVenta, nuevoIdFactura);

                string nroFacturaFormateado = "0001-" + nuevoIdFactura.ToString("D8");
                return Json(new { success = true, idFactura = nuevoIdFactura, nroFactura = nroFacturaFormateado, stocksActualizados });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Guardar venta completa (Sincronizado con Caja)
        [HttpPost]
        public JsonResult GuardarVentaCompleta([FromBody] VentaCompleta venta)
        {
            if (venta == null || venta.Productos == null || venta.Productos.Count == 0)
                return Json(new { success = false, message = "Datos incompletos" });

            try
            {
                var resultado = db.RegistrarVenta(venta);

                if (resultado.success)
                {
                    // IMPACTO EN CAJA
                    RegistrarIngresoEnCaja(venta.MontoVenta, resultado.idFactura);

                    string nroFactura = "0001-" + resultado.idFactura.ToString("D8");
                    return Json(new { success = true, idFactura = resultado.idFactura, nroFactura, message = "Venta guardada correctamente" });
                }

                return Json(new { success = false, message = resultado.error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error inesperado: " + ex.Message });
            }
        }

        // POST: Registrar venta (Sincronizado con Caja)
        [HttpPost]
        public JsonResult RegistrarVenta([FromBody] VentaCompleta datos)
        {
            try
            {
                if (datos == null || datos.Productos == null || datos.Productos.Count == 0)
                    throw new Exception("Datos incompletos");

                var resultado = db.RegistrarVenta(datos);

                if (!resultado.success) return Json(new { success = false, message = resultado.error });

                // IMPACTO EN CAJA
                RegistrarIngresoEnCaja(datos.MontoVenta, resultado.idFactura);

                string nroFacturaFormateado = "0001-" + resultado.idFactura.ToString("D8");
                return Json(new { success = true, idFactura = resultado.idFactura, nroFactura = nroFacturaFormateado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Obtener próximo número de factura
        [HttpGet]
        public JsonResult ObtenerProximoNroFactura()
        {
            try
            {
                int proximoId = db.ObtenerProximoAutoIncremento("facturas", "gestionventas");
                string nroFormateado = "0001-" + proximoId.ToString("D8");
                return Json(new { success = true, nroFactura = nroFormateado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Clases auxiliares necesarias para el model binding
        public class ProductoVenta
        {
            public int idProducto { get; set; }
            public int cantidad { get; set; }
        }
    }
}