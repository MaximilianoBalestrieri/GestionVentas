using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.IO;

namespace GestionVentas.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ConexionDB conexionDB;

        public ActionResult Index(string busqueda, int pagina = 1, int tamanioPagina = 10)
        {
            var fotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            ViewBag.FotoPerfil = fotoPerfil;
            List<Productos> productos = new List<Productos>();
            ConexionDB db = new ConexionDB(); // Usa tu clase de conexión

            using (MySqlConnection conn = new MySqlConnection(db.CadenaConexion))
            {
                conn.Open();

                string consulta = "SELECT * FROM productos";

                if (!string.IsNullOrEmpty(busqueda))
                {
                    consulta += " WHERE nombre LIKE @busqueda OR codigo LIKE @busqueda";
                }

                consulta += " LIMIT @inicio, @tamanio";

                using (MySqlCommand cmd = new MySqlCommand(consulta, conn))
                {
                    if (!string.IsNullOrEmpty(busqueda))
                    {
                        cmd.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");
                    }

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
                                Proveedor = reader["Proveedor"].ToString(),
                                StockActual = Convert.ToInt32(reader["StockActual"]),
                                Imagen = reader["Imagen"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                // PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                                // Otros campos que tengas
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

        //----------------------------------- BUSCAR------------------------------------

        [HttpGet]
        public JsonResult Buscar(string term)
        {
            
            var db = new ConexionDB();
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

public ActionResult ObtenerProductos()
{
    var productos = new ConexionDB().ObtenerProductos();
    return PartialView("filasProductos", productos);
}



        //---------------------------------- CREAR ----------------------------------
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductosController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            conexionDB = new ConexionDB();  // Acá creamos la instancia para usarla luego
        }


        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] Productos prod, IFormFile? imagen)

        {
            var fotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            ViewBag.FotoPerfil = fotoPerfil;
            try
            {
                if (ModelState.IsValid)
                {
                    if (imagen != null && imagen.Length > 0)
                    {
                        string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                        string rutaCarpeta = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes", "productos");

                        if (!Directory.Exists(rutaCarpeta))
                            Directory.CreateDirectory(rutaCarpeta);

                        string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            imagen.CopyTo(stream);
                        }

                        prod.Imagen = "/imagenes/productos/" + nombreArchivo;
                        Console.WriteLine("✔ Imagen guardada en: " + prod.Imagen);
                    }
                    else
                    {
                        prod.Imagen = "/imagenes/productos/no-disponible.png";
                        Console.WriteLine("⚠ No se cargó ninguna imagen, se usará por defecto.");
                    }

                    // Guardar el producto en la DB
                    conexionDB.AgregarProducto(prod);
                    Console.WriteLine("✔ Producto guardado correctamente");

                    return RedirectToAction("Index");
                }
                else
                {
                    var errores = ModelState.Values.SelectMany(v => v.Errors);
                    ViewBag.Errores = errores;
                    Console.WriteLine("❌ Errores en el ModelState:");
                    foreach (var e in errores)
                        Console.WriteLine(e.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al guardar el producto: " + ex.Message;
                Console.WriteLine("❌ Excepción: " + ex.Message);
            }

            return View(prod);
        }


        //---------------------- ELIMINAR ---------------------------
        // Mostrar la página de confirmación antes de borrar
        public IActionResult Delete(int id)
        {
            var fotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            ViewBag.FotoPerfil = fotoPerfil;
            var producto = conexionDB.ObtenerProductoPorId(id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // Eliminar el producto confirmado
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)

        {
            var producto = conexionDB.ObtenerProductoPorId(id);
            if (producto == null)
                return NotFound();

            // Eliminar imagen si no es la predeterminada
            if (!string.IsNullOrEmpty(producto.Imagen) && !producto.Imagen.EndsWith("no-disponible.png"))
            {
                var rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, producto.Imagen.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(rutaImagen))
                {
                    System.IO.File.Delete(rutaImagen);
                }
            }

            // Eliminar producto de la base de datos
            conexionDB.EliminarProducto(id);

            return RedirectToAction("Index");
        }

        //--------------------------------EDIT --------------------------------------------
    
  

    [HttpGet]
public IActionResult Edit(int id)
{
    var producto = conexionDB.ObtenerProductoPorId(id);
    if (producto == null)
    {
        return NotFound();
    }

    return View(producto);
}


    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Edit(Productos model, IFormFile? imagen)
{
    var fotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            ViewBag.FotoPerfil = fotoPerfil;
    if (!ModelState.IsValid)
            {
                return View(model);
            }

    // Traemos el producto original de la base de datos
    var producto = conexionDB.ObtenerProductoPorId(model.IdProducto);
    if (producto == null)
    {
        return NotFound();
    }

    // Actualizamos los campos
    producto.Codigo = model.Codigo;
    producto.Nombre = model.Nombre;
    producto.Categoria = model.Categoria;
    producto.Descripcion = model.Descripcion;
    producto.PrecioCosto = model.PrecioCosto;
    producto.RecargoPorcentaje = model.RecargoPorcentaje;
    producto.StockActual = model.StockActual;
    producto.StockMinimo = model.StockMinimo;
    producto.Proveedor = model.Proveedor;

    // Manejo de imagen nueva (si se cargó)
    if (imagen != null && imagen.Length > 0)
    {
        string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
        string rutaCarpeta = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes", "productos");

        if (!Directory.Exists(rutaCarpeta))
            Directory.CreateDirectory(rutaCarpeta);

        string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            imagen.CopyTo(stream);
        }

        producto.Imagen = "/imagenes/productos/" + nombreArchivo;
    }

    // Actualizamos en la base de datos
    conexionDB.ActualizarProducto(producto);

    return RedirectToAction("Index");
}


        //---------------------------------------------------------------------------------
    }
}



