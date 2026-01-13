using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;

namespace GestionVentas.Controllers
{
    public class VentasController : Controller
    {
        private readonly ConexionDB db;

        public VentasController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

        // GET: Crear venta
        public ActionResult Create()
        {
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

        // POST: Restar stock y registrar venta
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
                string nroFacturaFormateado = "0001-" + nuevoIdFactura.ToString("D8");

                return Json(new { success = true, idFactura = nuevoIdFactura, nroFactura = nroFacturaFormateado, stocksActualizados });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Guardar venta completa
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

        // POST: Registrar venta (alternativa)
        [HttpPost]
        public JsonResult RegistrarVenta([FromBody] VentaCompleta datos)
        {
            try
            {
                if (datos == null || datos.Productos == null || datos.Productos.Count == 0)
                    throw new Exception("Datos incompletos");

                var resultado = db.RegistrarVenta(datos);

                if (!resultado.success) return Json(new { success = false, message = resultado.error });

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

        // Clases auxiliares
        public class ProductoVenta
        {
            public int idProducto { get; set; }
            public int cantidad { get; set; }
        }


        
    }
}
