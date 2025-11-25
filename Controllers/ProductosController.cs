using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
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
            db = new ConexionDB(config);            // ← AHORA sí usa DI
            _webHostEnvironment = webHostEnvironment;
        }

        //--------------------------------------------------------------------
        // LISTAR CON BUSQUEDA Y PAGINACION
        //--------------------------------------------------------------------
        public ActionResult Index(string busqueda, int pagina = 1, int tamanioPagina = 10)
        {
            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");

            List<Productos> productos = new List<Productos>();

            using (MySqlConnection conn = new MySqlConnection(db.CadenaConexion))
            {
                conn.Open();

                string consulta =
                    "SELECT * FROM productos ";    // NO pongas alias si no los usás

                if (!string.IsNullOrEmpty(busqueda))
                {
                    consulta += " WHERE nombre LIKE @busqueda OR codigo LIKE @busqueda ";
                }

                consulta += " LIMIT @inicio, @tamanio ";

                using (MySqlCommand cmd = new MySqlCommand(consulta, conn))
                {
                    if (!string.IsNullOrEmpty(busqueda))
                        cmd.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");

                    int inicio = (pagina - 1) * tamanioPagina;
                    cmd.Parameters.AddWithValue("@inicio", inicio);
                    cmd.Parameters.AddWithValue("@tamanio", tamanioPagina);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(new Productos
                            {
                                IdProducto = Convert.ToInt32(reader["IdProducto"]),
                                Nombre = reader["Nombre"].ToString(),
                                Codigo = reader["Codigo"].ToString(),
                                PrecioCosto = Convert.ToDecimal(reader["PrecioCosto"]),
                                RecargoPorcentaje = Convert.ToDecimal(reader["RecargoPorcentaje"]),
                                NombreProveedor = reader["NombreProveedor"].ToString(),
                                StockActual = Convert.ToInt32(reader["StockActual"]),
                                Imagen = reader["Imagen"].ToString(),
                                Descripcion = reader["Descripcion"].ToString()
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

        //--------------------------------------------------------------------
        // BUSCAR (AJAX)
        //--------------------------------------------------------------------
        [HttpGet]
        public JsonResult Buscar(string term)
        {
            var productos = db.ObtenerProductos();

            if (!string.IsNullOrWhiteSpace(term))
            {
                productos = productos.Where(p =>
                    p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Json(productos);
        }

        //--------------------------------------------------------------------
        // CARGAR TABLA PARCIAL
        //--------------------------------------------------------------------
        public ActionResult ObtenerProductos()
        {
            var productos = db.ObtenerProductos();
            return PartialView("filasProductos", productos);
        }

        //--------------------------------------------------------------------
        // CREAR
        //--------------------------------------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            var modelo = new Productos();

            modelo.Proveedores = db.ObtenerProveedores()
                .Select(p => new SelectListItem
                {
                    Value = p.NombreProveedor,
                    Text = p.NombreProveedor
                }).ToList();

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] Productos prod, IFormFile? imagen)
        {
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

        //--------------------------------------------------------------------
        // ELIMINAR
        //--------------------------------------------------------------------
        public IActionResult Delete(int id)
        {
            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");

            var producto = db.ObtenerProductoPorId(id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var producto = db.ObtenerProductoPorId(id);
            if (producto == null)
                return NotFound();

            if (!string.IsNullOrEmpty(producto.Imagen) &&
                !producto.Imagen.Contains("no-disponible.png"))
            {
                string ruta = Path.Combine(_webHostEnvironment.WebRootPath,
                    producto.Imagen.TrimStart('/').Replace("/", "\\"));

                if (System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);
            }

            db.EliminarProducto(id);

            return RedirectToAction("Index");
        }

        //--------------------------------------------------------------------
        // EDITAR
        //--------------------------------------------------------------------
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var producto = db.ObtenerProductoPorId(id);

            if (producto == null)
                return RedirectToAction("Index");

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
            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");

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
            if (producto == null)
                return NotFound();

            producto.Codigo = model.Codigo;
            producto.Nombre = model.Nombre;
            producto.Categoria = model.Categoria;
            producto.Descripcion = model.Descripcion;
            producto.PrecioCosto = model.PrecioCosto;
            producto.RecargoPorcentaje = model.RecargoPorcentaje;
            producto.StockActual = model.StockActual;
            producto.StockMinimo = model.StockMinimo;
            producto.NombreProveedor = model.NombreProveedor;

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
