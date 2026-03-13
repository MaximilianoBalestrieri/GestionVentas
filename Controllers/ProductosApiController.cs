using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

[Route("api/[controller]")]
[ApiController]
public class ProductosApiController : ControllerBase
{
    [HttpGet]
    public IActionResult GetOfertas()
    {
        // Buscamos el archivo en la carpeta Data
        var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Data", "productos.json");

        if (!System.IO.File.Exists(rutaArchivo))
        {
            return NotFound("No se encontró el archivo de productos.");
        }

        // Leemos el texto del JSON
        var jsonString = System.IO.File.ReadAllText(rutaArchivo);

        // Lo transformamos a una lista de nuestra nueva clase
        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var listaProductos = JsonSerializer.Deserialize<List<ProductoApi>>(jsonString, opciones);

        return Ok(listaProductos);
    }
}
