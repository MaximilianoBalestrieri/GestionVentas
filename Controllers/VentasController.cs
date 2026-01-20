using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GestionVentas.Controllers
{
    public class VentasController : Controller
    {
        private readonly ConexionDB db;

        public VentasController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

        // --- VISTA PRINCIPAL DE VENTA ---
        public ActionResult Create()
        {
            // Intentamos obtener la caja abierta usando la lógica que ya tenés en ConexionDB
            // Si el método RegistrarVenta puede encontrar una caja, nosotros lo simulamos aquí
            bool hayCaja = db.VerificarSiHayCajaAbierta(); 

            if (!hayCaja)
            {
                TempData["MensajeError"] = "DEBE ABRIR CAJA PRIMERO PARA PODER VENDER.";
                return RedirectToAction("AbrirCaja", "Caja");
            }

            var productos = db.ObtenerProductos() ?? new List<Productos>();
            
            // Nombre del vendedor para la vista
            ViewBag.nombreyApellido = User.Identity.Name ?? "Vendedor"; 
            return View(productos);
        }

        // --- PROCESAR LA VENTA ---
        [HttpPost]
        public JsonResult RegistrarVenta([FromBody] VentaCompleta datos)
        {
            try
            {
                if (datos == null || datos.Items == null || datos.Items.Count == 0)
                    return Json(new { success = false, message = "No hay productos seleccionados." });

                // Llamamos al método de ConexionDB que ya modificamos juntos
                // Este ya maneja: Factura, Items, Stock, Caja (si es efectivo) y CtaCte
                var resultado = db.RegistrarVenta(datos);

                if (!resultado.success) 
                    return Json(new { success = false, message = resultado.error });

                string nroFacturaFormateado = "0001-" + resultado.idFactura.ToString("D8");
                
                return Json(new { 
                    success = true, 
                    idFactura = resultado.idFactura, 
                    nroFactura = nroFacturaFormateado 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // --- UTILIDAD PARA LA VISTA ---
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
    }
}