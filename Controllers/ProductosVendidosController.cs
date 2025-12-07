using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GestionVentas.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace GestionVentas.Controllers
{
    public class ProductosVendidosController : Controller
    {
        private readonly ConexionDB db;

        public ProductosVendidosController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

       public ActionResult Index(DateTime? desde, DateTime? hasta)
{
    List<ProductoVendidoViewModel> lista = new List<ProductoVendidoViewModel>();

    using (SqlConnection conn = db.ObtenerConexion())
    {
        conn.Open();

        string consulta = @"
            SELECT 
                fi.nombreProd AS ProductoNombre, -- Usar un alias único para evitar conflictos
                SUM(fi.Cantidad) AS TotalCantidad, 
                fi.Precio AS PrecioUnitario
            FROM facturaitem fi
            INNER JOIN facturas f ON fi.IdFactura = f.idFactura
            WHERE 1=1 -- Usar siempre WHERE 1=1 para facilitar la adición de condiciones
            
            -- Lógica para incluir condiciones de fecha solo si se proporcionan
            " + (desde.HasValue ? " AND f.diaVenta >= @desde " : "") + @"
            " + (hasta.HasValue ? " AND f.diaVenta <= @hasta " : "") + @"
            
            GROUP BY fi.nombreProd, fi.Precio
            ORDER BY TotalCantidad DESC;
        ";

        using (var cmd = new SqlCommand(consulta, conn))
        {
            // 1. CORRECCIÓN: Agregar parámetros de fecha solo si tienen valor.
            // Si son null, la consulta simplemente ignora la cláusula 'AND'.
            if (desde.HasValue)
            {
                // Usamos Add() en lugar de AddWithValue() para asegurar el tipo SqlDbType
                cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde.Value;
            }
            if (hasta.HasValue)
            {
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hasta.Value;
            }

            using (var reader = cmd.ExecuteReader())
            {
                // 2. CORRECCIÓN: Lectura segura con GetOrdinal
                int ordNombre = reader.GetOrdinal("ProductoNombre"); // Usamos el nuevo alias
                int ordCantidad = reader.GetOrdinal("TotalCantidad");
                int ordPrecio = reader.GetOrdinal("PrecioUnitario");

                while (reader.Read())
                {
                    lista.Add(new ProductoVendidoViewModel
                    {
                        // 3. CORRECCIÓN: Uso de métodos GetX y manejo de nulos (aunque SUM y GROUP BY deben evitar nulos aquí)
                        Nombre = reader.GetString(ordNombre),
                        Cantidad = reader.GetInt32(ordCantidad),
                        PrecioUnitario = reader.GetDecimal(ordPrecio)
                    });
                }
            }
        }
    }

    ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
    ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

    return View(lista);

    }

    }
}

