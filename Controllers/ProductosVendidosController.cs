using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using GestionVentas.Models;
using System;
using System.Collections.Generic;

namespace GestionVentas.Controllers
{
    public class ProductosVendidosController : Controller
    {
        public ActionResult Index(DateTime? desde, DateTime? hasta)
        {
            List<ProductoVendidoViewModel> lista = new List<ProductoVendidoViewModel>();

            ConexionDB db = new ConexionDB();

            using (MySqlConnection conn = new MySqlConnection(db.CadenaConexion))
            {
                conn.Open();

                    string consulta = @"
                    SELECT fi.nombreProd, SUM(fi.Cantidad) AS TotalCantidad, fi.Precio
                    FROM facturaitem fi
                    INNER JOIN facturas f ON fi.IdFactura = f.idFactura
                    WHERE (@desde IS NULL OR f.diaVenta >= @desde)
                    AND (@hasta IS NULL OR f.diaVenta <= @hasta)
                    GROUP BY fi.nombreProd, fi.Precio;
                ";

                using (var cmd = new MySqlCommand(consulta, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", (object)desde ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@hasta", (object)hasta ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ProductoVendidoViewModel
                            {
                                Nombre = reader["NombreProd"].ToString(),
                                Cantidad = Convert.ToInt32(reader["TotalCantidad"]),
                                PrecioUnitario = Convert.ToDecimal(reader["Precio"]),
                               
                            });
                        }
                    }
                }

                conn.Close();
            }

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            return View(lista);
        }
    }
}
