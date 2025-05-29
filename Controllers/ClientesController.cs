using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System;
using System.Linq;

public class ClientesController : Controller
{
    private readonly ConexionDB conexion = new ConexionDB();

    // Página principal de clientes (si la usás)
    public IActionResult Index()
    {
        var clientes = conexion.ObtenerClientes();
        return View(clientes);
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

    // Método para buscar clientes por filtro en JSON (para el input de búsqueda)
    [HttpGet]
    public JsonResult BuscarClientes(string filtro)
    {
        var clientes = conexion.ObtenerClientes();

        if (!string.IsNullOrEmpty(filtro))
        {
            clientes = clientes
                .Where(c => c.NombreCliente.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                         || c.DniCliente.Contains(filtro))
                .ToList();
        }

        return Json(clientes);
    }
}
