using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System;
using System.Linq;

public class ClientesController : Controller
{
    private readonly ConexionDB conexion = new ConexionDB();

    // Página principal de clientes 
 
    public IActionResult Index()
{
    var clientes = conexion.ObtenerClientes();

    int registrosPorPagina = 5;
    var clientesPagina = clientes.Take(registrosPorPagina).ToList();

    ViewBag.PaginaActual = 1;
    ViewBag.TotalPaginas = (int)Math.Ceiling((double)clientes.Count / registrosPorPagina);
    ViewBag.Filtro = "";

    return View(clientesPagina);
}


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Cliente cliente)
    {
        conexion.AgregarCliente(cliente);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var cliente = conexion.ObtenerClientePorId(id);
        return View(cliente);
    }

    [HttpPost]
    public IActionResult Edit(Cliente cliente)
    {
        conexion.ActualizarCliente(cliente);
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var cliente = conexion.ObtenerClientePorId(id);
        return View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        conexion.EliminarCliente(id);
        return RedirectToAction("Index");
    }

    public IActionResult Details(int id)
    {
        var cliente = conexion.ObtenerClientePorId(id);
        return View(cliente);
    }

    // Método para devolver todos los clientes en JSON (para llenar el modal)
    [HttpGet]
    public JsonResult ObtenerClientes()
    {
        var clientes = conexion.ObtenerClientes();
        return Json(clientes);
    }

    // Método para buscar clientes via Ajax como dijo el Profe
    [HttpGet]
  public IActionResult Buscar(string filtro, int pagina = 1)
{
    int registrosPorPagina = 5;
    var clientes = conexion.ObtenerClientes();

    if (!string.IsNullOrEmpty(filtro))
    {
        filtro = filtro.ToLower();
        clientes = clientes.Where(c =>
            (c.NombreCliente != null && c.NombreCliente.ToLower().Contains(filtro)) ||
            (c.DniCliente != null && c.DniCliente.Contains(filtro))
        ).ToList();
    }

    var totalClientes = clientes.Count;
    var totalPaginas = (int)Math.Ceiling((double)totalClientes / registrosPorPagina);

    var clientesPagina = clientes
        .Skip((pagina - 1) * registrosPorPagina)
        .Take(registrosPorPagina)
        .ToList();

    ViewBag.PaginaActual = pagina;
    ViewBag.TotalPaginas = totalPaginas;
    ViewBag.Filtro = filtro;

    return PartialView("_TablaClientes", clientesPagina);
}


}
