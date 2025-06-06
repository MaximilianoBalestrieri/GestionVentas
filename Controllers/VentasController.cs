using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{

    public class VentasController : Controller
    {
        public ActionResult Create()
        {
            ConexionDB db = new ConexionDB();
            var productos = db.ObtenerProductos(); 

            // Asegurate de que no sea null
            if (productos == null)
            {
                productos = new List<Productos>();
            }
            ViewBag.Vendedor = User.Identity.Name ?? "Usuario Desconocido";
            return View(productos);
        }

[HttpPost]
public JsonResult Abonar([FromBody] List<ProductoVenta> productos)
{
    try
    {
        ConexionDB db = new ConexionDB();

        foreach (var p in productos)
        {
            db.RestarStock(p.idProducto, p.cantidad);
        }

        return Json(new { exito = true });
    }
    catch (Exception ex)
    {
        return Json(new { exito = false, error = ex.Message });
    }
}

        public class ProductoVenta
        {
            public int idProducto { get; set; }
            public int cantidad { get; set; }
        }

[HttpPost]
public JsonResult RestarStock([FromBody] VentaConProductos datos)
{
    try
    {
        if (datos == null || datos.Productos == null || datos.Productos.Count == 0)
            throw new Exception("Datos incompletos");

        ConexionDB conexion = new ConexionDB();

        // Lista para devolver stocks actualizados
        var stocksActualizados = new List<object>();

        foreach (var p in datos.Productos)
        {
            if (p == null)
                throw new Exception("Uno de los productos es nulo");

            conexion.RestarStock(p.IdProducto, p.Cantidad);

            // Obtener stock actualizado después de restar
            int stockActual = conexion.ObtenerStockActual(p.IdProducto);
            stocksActualizados.Add(new {
                IdProducto = p.IdProducto,
                StockDisponible = stockActual
            });
        }

        // Guardar la venta
        Venta venta = new Venta
        {
            DiaVenta = DateTime.Today,
            MontoVenta = datos.MontoVenta
        };

        int nuevoIdFactura = conexion.GuardarVenta(venta);

        // Generar el número de factura con formato: 0001-00000001
        string nroFacturaFormateado = "0001-" + nuevoIdFactura.ToString("D8");

        // Devolver éxito, nro de factura y stocks actualizados
        return Json(new
        {
            success = true,
            idFactura = nuevoIdFactura,
            nroFactura = nroFacturaFormateado,
            stocksActualizados = stocksActualizados
        });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}

[HttpPost]
public JsonResult GuardarVentaCompleta([FromBody] VentaCompleta venta)
{
    if (venta == null || venta.Productos == null || venta.Productos.Count == 0)
        return Json(new { success = false, message = "Datos incompletos" });

    try
    {
        ConexionDB conexion = new ConexionDB();
        var resultado = conexion.RegistrarVenta(venta);

        if (resultado.success)
        {
            string nroFactura = "0001-" + resultado.idFactura.ToString("D8");
            return Json(new
            {
                success = true,
                idFactura = resultado.idFactura,
                nroFactura = nroFactura,
                message = "Venta guardada correctamente"
            });
        }
        else
        {
            return Json(new { success = false, message = resultado.error });
        }
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Error inesperado: " + ex.Message });
    }
}


       [HttpPost]
public JsonResult RegistrarVenta([FromBody] VentaCompleta datos)
{
    try
    {
        if (datos == null || datos.Productos == null || datos.Productos.Count == 0)
            throw new Exception("Datos incompletos");

        Console.WriteLine("Llamando al método RegistrarVenta de ConexionDB...");
        Console.WriteLine("Vendedor: " + datos.Vendedor + " - Cliente: " + datos.IdCliente);
        Console.WriteLine("Productos recibidos: " + datos.Productos.Count);

        ConexionDB conexion = new ConexionDB();

        // Llamamos al método que hace todo (factura + ítems + stock) dentro de una transacción
        var resultado = conexion.RegistrarVenta(datos);

        if (!resultado.success)
        {
            Console.WriteLine("❌ Error en la base de datos: " + resultado.error);
            return Json(new { success = false, message = resultado.error });
        }

        string nroFacturaFormateado = "0001-" + resultado.idFactura.ToString("D8");

        return Json(new
        {
            success = true,
            idFactura = resultado.idFactura,
            nroFactura = nroFacturaFormateado
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine("💥 Excepción en el controlador: " + ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
}


[HttpGet]
public JsonResult ObtenerProximoNroFactura()
{
    try
    {
        ConexionDB conexion = new ConexionDB();
        int proximoId = conexion.ObtenerProximoAutoIncremento("facturas", "gestionventas");

        string nroFormateado = "0001-" + proximoId.ToString("D8");

        return Json(new { success = true, nroFactura = nroFormateado });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}






    }
}