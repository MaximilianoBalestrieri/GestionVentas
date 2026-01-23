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

        public ActionResult Create()
        {
            bool hayCaja = db.VerificarSiHayCajaAbierta(); 

            if (!hayCaja)
            {
                TempData["MensajeError"] = "DEBE ABRIR CAJA PRIMERO PARA PODER VENDER.";
                return RedirectToAction("AbrirCaja", "Caja");
            }

            var productos = db.ObtenerProductos() ?? new List<Productos>();
            ViewBag.nombreyApellido = User.Identity.Name ?? "Vendedor"; 
            return View(productos);
        }

      [HttpPost]
public JsonResult RegistrarVenta([FromBody] VentaCompleta datos)
{
    try
    {
        if (datos == null || datos.Items == null || datos.Items.Count == 0)
            return Json(new { success = false, message = "No hay productos seleccionados." });

        // 1. Registramos la venta en la DB (Factura, Items, Stock, y Saldo de Cliente)
        var resultado = db.RegistrarVenta(datos);

        if (resultado.success) 
        {
            // --- LÓGICA DE IMPACTO EN CAJA ---
            var cajaAbierta = db.Cajas.FirstOrDefault(c => c.EstaAbierta);
            
            if (cajaAbierta != null)
            {
                string medio = datos.MedioPago ?? "Efectivo";

                // FILTRO CLAVE: Solo impacta en caja si NO es Fiado/CtaCte
                // Agregamos todas las variantes posibles por seguridad
                bool esCtaCte = medio.Equals("CtaCte", StringComparison.OrdinalIgnoreCase) || 
                                medio.Equals("Cuenta Corriente", StringComparison.OrdinalIgnoreCase);

                if (!esCtaCte)
                {
                    // Si llegamos acá, es Efectivo o Transferencia
                    string etiquetaMedio = medio.Contains("Transferencia") ? "(Transferencia)" : "(Efectivo)";
                    
                    var mov = new MovimientoCaja
                    {
                        CajaId = cajaAbierta.Id,
                        Fecha = DateTime.Now,
                        Monto = datos.MontoVenta,
                        Concepto = $"Venta Factura N° {resultado.idFactura} {etiquetaMedio}",
                        Tipo = TipoMovimiento.Ingreso
                    };

                    db.MovimientosCaja.Add(mov);

                    // Solo sumamos al esperado si entró dinero físico o banco
                    cajaAbierta.MontoEsperado += datos.MontoVenta;
                    
                    db.Cajas.Update(cajaAbierta);
                    db.SaveChanges();
                }
            }

            string nroFacturaFormateado = "0001-" + resultado.idFactura.ToString("D8");
            return Json(new { 
                success = true, 
                idFactura = resultado.idFactura, 
                nroFactura = nroFacturaFormateado 
            });
        }
        else
        {
            return Json(new { success = false, message = resultado.error });
        }
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Error: " + ex.Message });
    }
}

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