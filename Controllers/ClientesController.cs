using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;

public class ClientesController : Controller
{
    private readonly ConexionDB conexion = new ConexionDB();

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
}
