using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionVentas.Controllers
{

    public class VentasController : Controller
    {
        public ActionResult Index()
        {
            ConexionDB db = new ConexionDB();
            var productos = db.ObtenerProductos(); // este método debe devolver List<Productos>

            // Asegurate de que no sea null
            if (productos == null)
            {
                productos = new List<Productos>();
            }

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

            conexion.RestarStock(p.IdProducto, p.CantidadVendida);

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


[HttpGet]
public JsonResult ObtenerProximoNroFactura()
{
    try
    {
        ConexionDB conexion = new ConexionDB();
        int ultimoId = conexion.ObtenerUltimaFactura();
        string nroFormateado = "0001-" + (ultimoId + 1).ToString("D8");
        return Json(new { success = true, nroFactura = nroFormateado });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}






    }
}