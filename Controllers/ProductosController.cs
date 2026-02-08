using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionVentas.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ConexionDB db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductosController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            db = new ConexionDB(config);
            _webHostEnvironment = webHostEnvironment;
        }

        public ActionResult Index(string busqueda, int pagina = 1, int tamanioPagina = 10)
        {
            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            List<Productos> productos = new List<Productos>();
            int inicio = (pagina - 1) * tamanioPagina;

            using (SqlConnection conn = db.ObtenerConexion())
            {
                conn.Open();

                // La lista explícita de columnas evita la inclusión de PrecioVenta (calculada)
                // Mantenemos StockMinimo aquí, ya que lo incluiste en la DB.
                string consulta = "SELECT IdProducto, Nombre, Codigo, PrecioCosto, RecargoPorcentaje, NombreProveedor, StockActual, StockMinimo, Imagen, Descripcion FROM productos";

                if (!string.IsNullOrEmpty(busqueda))
                    consulta += " WHERE nombre LIKE @busqueda OR codigo LIKE @busqueda";

                consulta += " ORDER BY IdProducto OFFSET @inicio ROWS FETCH NEXT @tamanio ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    if (!string.IsNullOrEmpty(busqueda))
                        cmd.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");

                    cmd.Parameters.AddWithValue("@inicio", inicio);
                    cmd.Parameters.AddWithValue("@tamanio", tamanioPagina);

                    using (var reader = cmd.ExecuteReader())
                    {
                        // Definición de Ordinales
                        int ordinalId = reader.GetOrdinal("IdProducto");
                        int ordinalNombre = reader.GetOrdinal("Nombre");
                        int ordinalCodigo = reader.GetOrdinal("Codigo");
                        int ordinalCosto = reader.GetOrdinal("PrecioCosto");
                        int ordinalRecargo = reader.GetOrdinal("RecargoPorcentaje");
                        int ordinalProveedor = reader.GetOrdinal("NombreProveedor");
                        int ordinalStockActual = reader.GetOrdinal("StockActual");
                        int ordinalStockMinimo = reader.GetOrdinal("StockMinimo");
                        int ordinalImagen = reader.GetOrdinal("Imagen");
                        int ordinalDescripcion = reader.GetOrdinal("Descripcion");

                        while (reader.Read())
                        {
                            productos.Add(new Productos
                            {
                                IdProducto = reader.GetInt32(ordinalId),
                                Nombre = reader.GetString(ordinalNombre),
                                Codigo = reader.GetString(ordinalCodigo),

                                // *** CORRECCIÓN CRÍTICA: Convertir INT a DECIMAL de forma segura ***
                                PrecioCosto = reader.IsDBNull(ordinalCosto) ? 0.00M : Convert.ToDecimal(reader.GetValue(ordinalCosto)),
                                RecargoPorcentaje = reader.IsDBNull(ordinalRecargo) ? 0.00M : Convert.ToDecimal(reader.GetValue(ordinalRecargo)),

                                NombreProveedor = reader.IsDBNull(ordinalProveedor) ? null : reader.GetString(ordinalProveedor),

                                // Lectura de Stock
                                StockActual = reader.IsDBNull(ordinalStockActual) ? 0 : reader.GetInt32(ordinalStockActual),
                                StockMinimo = reader.IsDBNull(ordinalStockMinimo) ? 0 : reader.GetInt32(ordinalStockMinimo),

                                // Lectura de strings con nulos
                                Imagen = reader.IsDBNull(ordinalImagen) ? null : reader.GetString(ordinalImagen),
                                Descripcion = reader.IsDBNull(ordinalDescripcion) ? null : reader.GetString(ordinalDescripcion)
                            });
                        }
                    }
                }
            }

            ViewBag.Busqueda = busqueda;
            ViewBag.Pagina = pagina;
            ViewBag.TamanioPagina = tamanioPagina;

            return View(productos);

        }
       [HttpGet]
public JsonResult Buscar(string term, string proveedor)
{
    // Traemos todos los productos inicialmente
    var productos = db.ObtenerProductos();

    // Filtro por Nombre o Código (el primer input)
    if (!string.IsNullOrWhiteSpace(term))
    {
        productos = productos.Where(p =>
            p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (p.Codigo != null && p.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    // Filtro por Proveedor (el segundo input que agregamos)
    if (!string.IsNullOrWhiteSpace(proveedor))
    {
        productos = productos.Where(p =>
            p.NombreProveedor != null && 
            p.NombreProveedor.Contains(proveedor, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }

    return Json(productos);
}
        

        public ActionResult ObtenerProductos()
        {
            var productos = db.ObtenerProductos();
            return PartialView("filasProductos", productos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var modelo = new Productos
            {
                Proveedores = db.ObtenerProveedores()
                    .Select(p => new SelectListItem
                    {
                        Value = p.NombreProveedor,
                        Text = p.NombreProveedor
                    }).ToList()
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] Productos prod, IFormFile? imagen)
        {
            var culturaArg = new System.Globalization.CultureInfo("es-AR");
            System.Threading.Thread.CurrentThread.CurrentCulture = culturaArg;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culturaArg;

            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");

            try
            {
                if (ModelState.IsValid)
                {
                    string? nombreArchivo = null;

                    if (imagen != null && imagen.Length > 0)
                    {
                        nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                        string rutaCarpeta = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes", "productos");
                        Directory.CreateDirectory(rutaCarpeta);

                        using (var stream = new FileStream(Path.Combine(rutaCarpeta, nombreArchivo), FileMode.Create))
                        {
                            imagen.CopyTo(stream);
                        }

                        prod.Imagen = "/imagenes/productos/" + nombreArchivo;
                    }
                    else
                    {
                        prod.Imagen = "/imagenes/productos/no-disponible.png";
                    }

                    db.AgregarProducto(prod);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al guardar el producto: " + ex.Message;
            }

            prod.Proveedores = db.ObtenerProveedores()
                .Select(p => new SelectListItem
                {
                    Value = p.NombreProveedor,
                    Text = p.NombreProveedor
                }).ToList();

            return View(prod);
        }

        public IActionResult Delete(int id)
        {
            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            var producto = db.ObtenerProductoPorId(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var producto = db.ObtenerProductoPorId(id);
            if (producto == null) return NotFound();

            if (!string.IsNullOrEmpty(producto.Imagen) && !producto.Imagen.Contains("no-disponible.png"))
            {
                string ruta = Path.Combine(_webHostEnvironment.WebRootPath, producto.Imagen.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }

            db.EliminarProducto(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var producto = db.ObtenerProductoPorId(id);
            if (producto == null) return RedirectToAction("Index");

            producto.Proveedores = db.ObtenerProveedores()
                .Select(p => new SelectListItem
                {
                    Value = p.NombreProveedor,
                    Text = p.NombreProveedor
                }).ToList();

            return View(producto);
        }

       [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Edit(Productos model, IFormFile? imagen)
{
    // 1. Configurar la cultura para este hilo
    var culturaArg = new System.Globalization.CultureInfo("es-AR");
    System.Threading.Thread.CurrentThread.CurrentCulture = culturaArg;
    System.Threading.Thread.CurrentThread.CurrentUICulture = culturaArg;

    // 2. CORRECCIÓN MANUAL: Leer directamente del formulario para evitar los dos ceros extra
    // Esto rescata el valor exacto que escribió el usuario (con coma)
    string precioCostoRaw = Request.Form["PrecioCosto"].ToString();
    string recargoRaw = Request.Form["RecargoPorcentaje"].ToString();

    if (decimal.TryParse(precioCostoRaw, System.Globalization.NumberStyles.Any, culturaArg, out decimal precioLimpio))
    {
        model.PrecioCosto = precioLimpio;
    }
    if (decimal.TryParse(recargoRaw, System.Globalization.NumberStyles.Any, culturaArg, out decimal recargoLimpio))
    {
        model.RecargoPorcentaje = recargoLimpio;
    }

    ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");

    // Revalidar el modelo después de la corrección manual
    ModelState.Clear(); 
    TryValidateModel(model);

    if (!ModelState.IsValid)
    {
        model.Proveedores = db.ObtenerProveedores()
            .Select(p => new SelectListItem
            {
                Value = p.NombreProveedor,
                Text = p.NombreProveedor
            }).ToList();
        return View(model);
    }

    var producto = db.ObtenerProductoPorId(model.IdProducto);
    if (producto == null) return NotFound();

    // Asignar los valores corregidos
    producto.Codigo = model.Codigo;
    producto.Nombre = model.Nombre;
    producto.Categoria = model.Categoria;
    producto.Descripcion = model.Descripcion;
    producto.PrecioCosto = model.PrecioCosto; // Valor ya limpio
    producto.RecargoPorcentaje = model.RecargoPorcentaje; // Valor ya limpio
    producto.StockActual = model.StockActual;
    producto.StockMinimo = model.StockMinimo;
    producto.NombreProveedor = model.NombreProveedor;

    // Lógica de imagen (sin cambios para no romperla)
    if (imagen != null && imagen.Length > 0)
    {
        string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
        string rutaCarpeta = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes", "productos");
        Directory.CreateDirectory(rutaCarpeta);

        using (var stream = new FileStream(Path.Combine(rutaCarpeta, nombreArchivo), FileMode.Create))
        {
            imagen.CopyTo(stream);
        }

        producto.Imagen = "/imagenes/productos/" + nombreArchivo;
    }

    db.ActualizarProducto(producto);
    return RedirectToAction("Index");
}
}
}