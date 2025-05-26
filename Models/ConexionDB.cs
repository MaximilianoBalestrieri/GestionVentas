using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Web;
using MySql.Data.MySqlClient;
using GestionVentas.Models;


namespace GestionVentas.Models
{
    public class ConexionDB
    {
        public string CadenaConexion => _connectionString;

        private MySqlConnection conexion;
        private string _connectionString;

        // Constructor donde inicializamos la conexión
        public ConexionDB()
        {
            string servidor = "localhost";
            string baseDatos = "gestionventas";  // Nombre de tu base de datos
            string usuario = "root";  // Usuario por defecto en XAMPP
            string contrasena = "";   // En XAMPP no suele tener contraseña por defecto

            _connectionString = $"Server={servidor}; database={baseDatos}; UID={usuario}; password={contrasena};";
            conexion = new MySqlConnection(_connectionString);
        }

        public ConexionDB(string cadenaConexion)
        {
            _connectionString = cadenaConexion;
            conexion = new MySqlConnection(_connectionString);
        }

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(_connectionString);
        }


        //--------------------- PRESUPUESTO -------------------------------------

public List<Presupuesto> ObtenerPresupuestos()
{
    var lista = new List<Presupuesto>();

    using (var conn = ObtenerConexion())
    {
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Presupuesto ORDER BY Fecha DESC";

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(new Presupuesto
                {
                    IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                    NombreCliente = reader["NombreCliente"].ToString(),
                    TelefonoCliente = reader["TelefonoCliente"].ToString(),
                    Fecha = Convert.ToDateTime(reader["Fecha"])
                });
            }
        }
    }

    return lista;
}

public void EliminarPresupuesto(int id)
{
    using (var conn = ObtenerConexion())
    {
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Presupuesto WHERE IdPresupuesto = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}

        public Presupuesto ObtenerPresupuestoPorId(int id)
        {
            Presupuesto presupuesto = null;

            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Presupuesto WHERE IdPresupuesto = @id";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        presupuesto = new Presupuesto
                        {
                            IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                            NombreCliente = reader["NombreCliente"].ToString(),
                            TelefonoCliente = reader["TelefonoCliente"].ToString(),
                            Fecha = Convert.ToDateTime(reader["Fecha"])
                        };
                    }
                }

                if (presupuesto != null)
                {
                    presupuesto.Items = new List<PresupuestoItem>();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM PresupuestoItem WHERE IdPresupuesto = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            presupuesto.Items.Add(new PresupuestoItem
                            {
                                IdItem = Convert.ToInt32(reader["IdItem"]),
                                IdPresupuesto = id,
                                Descripcion = reader["Descripcion"].ToString(),
                                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"])
                            });
                        }
                    }
                }
            }

            return presupuesto;
        }

      




public void AgregarPresupuesto(Presupuesto presupuesto)
{
    using (var conn = ObtenerConexion())
    {
        conn.Open();

        using (var tran = conn.BeginTransaction())
        {
            try
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = "INSERT INTO Presupuesto (NombreCliente, TelefonoCliente, Fecha) VALUES (@nombre, @telefono, @fecha)";
                cmd.Parameters.AddWithValue("@nombre", presupuesto.NombreCliente);
                cmd.Parameters.AddWithValue("@telefono", presupuesto.TelefonoCliente);
                cmd.Parameters.AddWithValue("@fecha", presupuesto.Fecha);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT LAST_INSERT_ID()";
                presupuesto.IdPresupuesto = Convert.ToInt32(cmd.ExecuteScalar());

                foreach (var item in presupuesto.Items)
                {
                    cmd = conn.CreateCommand();
                    cmd.Transaction = tran;
                    cmd.CommandText = @"INSERT INTO PresupuestoItem (IdPresupuesto, Descripcion, Cantidad, PrecioUnitario) 
                                        VALUES (@idPresupuesto, @descripcion, @cantidad, @precio)";
                    cmd.Parameters.AddWithValue("@idPresupuesto", presupuesto.IdPresupuesto);
                    cmd.Parameters.AddWithValue("@descripcion", item.Descripcion);
                    cmd.Parameters.AddWithValue("@cantidad", item.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", item.PrecioUnitario);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}


        //---------------------------- VENTAS POR FECHA ---------------------
        public List<object> ObtenerTotalVentasPorFecha(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
            SELECT DATE(diaVenta) as fecha, SUM(montoVenta) as totalVendido
            FROM ventas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY DATE(diaVenta)
            ORDER BY diaVenta", conn);

                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new
                    {
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        totalVendido = Convert.ToDecimal(reader["totalVendido"])
                    });
                }
            }
            return lista;
        }


public List<object> ObtenerTotalVentasPorMes(DateTime desde, DateTime hasta)
{
    List<object> lista = new List<object>();
    using (var conn = ObtenerConexion())
    {
        conn.Open();
        var cmd = new MySqlCommand(@"
            SELECT YEAR(diaVenta) AS anio, MONTH(diaVenta) AS mes, SUM(montoVenta) AS totalVendido
            FROM ventas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY anio, mes
            ORDER BY anio, mes", conn);

        cmd.Parameters.AddWithValue("@desde", desde);
        cmd.Parameters.AddWithValue("@hasta", hasta);

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int anio = Convert.ToInt32(reader["anio"]);
            int mes = Convert.ToInt32(reader["mes"]);
            lista.Add(new
            {
                fecha = new DateTime(anio, mes, 1),
                totalVendido = Convert.ToDecimal(reader["totalVendido"])
            });
        }
    }
    return lista;
}

public List<object> ObtenerTotalVentasPorAnio(DateTime desde, DateTime hasta)
{
    List<object> lista = new List<object>();
    using (var conn = ObtenerConexion())
    {
        conn.Open();
        var cmd = new MySqlCommand(@"
            SELECT YEAR(diaVenta) AS anio, SUM(montoVenta) AS totalVendido
            FROM ventas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY anio
            ORDER BY anio", conn);

        cmd.Parameters.AddWithValue("@desde", desde);
        cmd.Parameters.AddWithValue("@hasta", hasta);

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int anio = Convert.ToInt32(reader["anio"]);
            lista.Add(new
            {
                fecha = new DateTime(anio, 1, 1),
                totalVendido = Convert.ToDecimal(reader["totalVendido"])
            });
        }
    }
    return lista;
}



        //---- USUARIOS ------

        public Usuario ObtenerUsuarioPorNombre(string nombreUsuario)
        {
            Usuario u = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM Usuarios WHERE Usuario = @nombre";
                using (var command = new MySqlCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@nombre", nombreUsuario);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = new Usuario
                            {
                                IdUsuario = reader.GetInt32("IdUsuario"),
                                UsuarioNombre = reader.GetString("Usuario"),
                                NombreyApellido = reader.GetString("NombreYApellido"),
                                Rol = reader.GetString("Rol"),
                                Contraseña = reader.GetString("Contraseña"),
                                FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? reader.GetString("FotoPerfil") : null

                            };
                            Console.WriteLine("Ruta de la maldita foto: " + u.FotoPerfil);

                        }
                    }
                }
            }

            return u;
        }

        public void ActualizarRutaFoto(Usuario u)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET FotoPerfil = @fotoperfil WHERE idUsuario = @idUsuario";
                using (var command = new MySqlCommand(query, connection))
                {
                    Console.WriteLine("ID del usuario: " + u.IdUsuario);

                    command.Parameters.AddWithValue("@fotoPerfil", (object?)u.FotoPerfil ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuario", u.IdUsuario);
                    command.ExecuteNonQuery();
                }
            }
        }


        public Usuario ObtenerUsuarioPorId(int idUsuario)
        {
            Usuario usuario = null;
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT * FROM usuarios WHERE idUsuario = @idUsuario", con);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    usuario = new Usuario
                    {
                        IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                        UsuarioNombre = reader["Usuario"].ToString(),
                        Contraseña = reader["contraseña"].ToString(),
                        Rol = reader["rol"].ToString(),
                        NombreyApellido = reader["nombreyApellido"].ToString(),
                        FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? reader.GetString("FotoPerfil") : null
                    };
                }
            }
            return usuario;
        }

        public void AgregarUsuario(Usuario u)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new MySqlCommand("INSERT INTO usuarios (Usuario, contraseña, rol, nombreyApellido, FotoPerfil) VALUES (@nombre,  @contraseña, @rol, @nombreyApellido, @FotoPerfil)", con);
                cmd.Parameters.AddWithValue("@nombre", u.UsuarioNombre);
                cmd.Parameters.AddWithValue("@contraseña", u.Contraseña);
                cmd.Parameters.AddWithValue("@rol", u.Rol);
                cmd.Parameters.AddWithValue("@nombreyApellido", u.NombreyApellido);
                cmd.Parameters.AddWithValue("@FotoPerfil", u.FotoPerfil);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarUsuario(Usuario u)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new MySqlCommand("UPDATE usuarios SET Usuario=@nombre, contraseña=@contraseña, rol=@rol, nombreyApellido=@nombreyApellido, FotoPerfil=@FotoPerfil WHERE idUsuario=@idUsuario", con);

                cmd.Parameters.AddWithValue("@idUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@nombre", u.UsuarioNombre);
                cmd.Parameters.AddWithValue("@contraseña", u.Contraseña);
                cmd.Parameters.AddWithValue("@rol", u.Rol);
                cmd.Parameters.AddWithValue("@nombreyApellido", u.NombreyApellido);
                cmd.Parameters.AddWithValue("@FotoPerfil", u.FotoPerfil);
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarUsuario(int idUsuario)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new MySqlCommand("DELETE FROM usuarios WHERE idUsuario=@id", con);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }



        public void ActualizarClave(Usuario u)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET contraseña = @contraseña WHERE IdUsuario = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@contraseña", u.Contraseña);
                    command.Parameters.AddWithValue("@id", u.IdUsuario);
                    command.ExecuteNonQuery();
                }
            }
        }



        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT idUsuario, Usuario, contraseña, rol, nombreyApellido, FotoPerfil FROM Usuarios", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Usuario
                    {
                        IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                        UsuarioNombre = reader["Usuario"].ToString(),
                        Contraseña = reader["contraseña"].ToString(),
                        Rol = reader["rol"].ToString(),
                        NombreyApellido = reader["nombreyApellido"].ToString(),
                        FotoPerfil = !reader.IsDBNull(reader.GetOrdinal("FotoPerfil")) ? reader["FotoPerfil"].ToString() : null
                    });
                }
            }

            //  Asegurar la foto por defecto si está vacía
            foreach (var u in lista)
            {
                if (string.IsNullOrEmpty(u.FotoPerfil))
                {
                    u.FotoPerfil = "/imagenes/usuarios/default.png";
                }
            }

            return lista;
        }




        public Usuario BuscarUsuario(string usuario, string contraseña)
        {
            Usuario encontrado = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = "SELECT * FROM usuarios WHERE usuario = @usuario AND contraseña = @contraseña";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@contraseña", contraseña);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            encontrado = new Usuario
                            {
                                IdUsuario = reader.GetInt32(reader.GetOrdinal("idUsuario")),
                                UsuarioNombre = reader.GetString(reader.GetOrdinal("Usuario")),
                                Rol = reader.GetString(reader.GetOrdinal("rol")),
                                NombreyApellido = reader.GetString(reader.GetOrdinal("nombreyApellido")),
                                FotoPerfil = reader.GetString(reader.GetOrdinal("fotoPerfil")),
                            };
                        }
                    }
                }
            }
            return encontrado;
        }

        public void EditarUsuario(Usuario usuario)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sql = @"UPDATE Usuario SET 
                        UsuarioNombre = @usuario, 
                        NombreyApellido = @nombre, 
                        Contraseña = @pass, 
                        Rol = @rol, 
                        FotoPerfil = @foto
                    WHERE IdUsuario = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario.UsuarioNombre);
                    cmd.Parameters.AddWithValue("@nombre", usuario.NombreyApellido);
                    cmd.Parameters.AddWithValue("@pass", usuario.Contraseña);
                    cmd.Parameters.AddWithValue("@rol", usuario.Rol);
                    cmd.Parameters.AddWithValue("@foto", usuario.FotoPerfil ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        //-------------------------------PRODUCTOS-----------------------------

        public List<Productos> ObtenerProductos()
        {
            List<Productos> productos = new List<Productos>();

            using (MySqlConnection conn = ObtenerConexion())
            {
                conn.Open();
                string sql = "SELECT * FROM Productos";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
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
                            Imagen = reader["Imagen"].ToString(),
                            Descripcion = reader["Descripcion"].ToString(),
                            StockActual = Convert.ToInt32(reader["StockActual"])
                        });
                    }
                }
            }

            return productos;
        }
        //-------------------CREAR PRODUCTOS-------------------
        public void AgregarProducto(Productos p)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                string query = @"INSERT INTO Productos 
        (codigo, nombre, descripcion, categoria, precioCosto, recargoPorcentaje, precioVenta, stockActual, stockMinimo, proveedor, imagen) 
        VALUES 
        (@codigo, @nombre, @descripcion, @categoria, @precioCosto, @recargoPorcentaje, @precioVenta, @stockActual, @stockMinimo, @proveedor, @imagen)";

                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@codigo", p.Codigo);
                    cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", p.Descripcion);
                    cmd.Parameters.AddWithValue("@categoria", p.Categoria);
                    cmd.Parameters.AddWithValue("@precioCosto", p.PrecioCosto);
                    cmd.Parameters.AddWithValue("@recargoPorcentaje", p.RecargoPorcentaje);
                    cmd.Parameters.AddWithValue("@precioVenta", p.PrecioVenta);
                    cmd.Parameters.AddWithValue("@stockActual", p.StockActual);
                    cmd.Parameters.AddWithValue("@stockMinimo", p.StockMinimo);
                    cmd.Parameters.AddWithValue("@proveedor", p.Proveedor);
                    cmd.Parameters.AddWithValue("@imagen", p.Imagen ?? "");


                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void EliminarProducto(int idProducto)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                string consulta = "DELETE FROM Productos WHERE IdProducto = @IdProducto";

                using (var comando = new MySqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@IdProducto", idProducto);
                    comando.ExecuteNonQuery();
                }
            }
        }


        public Productos ObtenerProductoPorId(int id)
        {
            Productos prod = null;

            using (var conexion = new MySqlConnection(_connectionString))
            {
                conexion.Open();
                string sql = "SELECT * FROM Productos WHERE IdProducto = @id";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            prod = new Productos();
                            prod.IdProducto = Convert.ToInt32(lector["IdProducto"]);
                            prod.Codigo = lector["Codigo"].ToString();
                            prod.Nombre = lector["Nombre"].ToString();
                            prod.Categoria = lector["Categoria"].ToString();
                            prod.Descripcion = lector["Descripcion"].ToString();
                            prod.PrecioCosto = Convert.ToDecimal(lector["PrecioCosto"]);
                            prod.RecargoPorcentaje = Convert.ToDecimal(lector["RecargoPorcentaje"]);
                            // prod.PrecioVenta = Convert.ToDecimal(lector["PrecioVenta"]);
                            prod.StockActual = Convert.ToInt32(lector["StockActual"]);
                            prod.StockMinimo = Convert.ToInt32(lector["StockMinimo"]);
                            prod.Proveedor = lector["Proveedor"].ToString();
                            prod.Imagen = lector["Imagen"].ToString();
                        }
                    }
                }
            }

            return prod;
        }

        public void ActualizarProducto(Productos producto)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                conexion.Open();
                string sql = @"UPDATE Productos SET 
                        Codigo = @Codigo,
                        Nombre = @Nombre,
                        Categoria = @Categoria,
                        Descripcion = @Descripcion,
                        PrecioCosto = @PrecioCosto,
                        RecargoPorcentaje = @RecargoPorcentaje,
                        StockActual = @StockActual,
                        StockMinimo = @StockMinimo,
                        Proveedor = @Proveedor,
                        Imagen = @Imagen
                      WHERE IdProducto = @IdProducto";

                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@Codigo", producto.Codigo);
                    comando.Parameters.AddWithValue("@Nombre", producto.Nombre);
                    comando.Parameters.AddWithValue("@Categoria", producto.Categoria);
                    comando.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                    comando.Parameters.AddWithValue("@PrecioCosto", producto.PrecioCosto);
                    comando.Parameters.AddWithValue("@RecargoPorcentaje", producto.RecargoPorcentaje);
                    comando.Parameters.AddWithValue("@StockActual", producto.StockActual);
                    comando.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                    comando.Parameters.AddWithValue("@Proveedor", producto.Proveedor);
                    comando.Parameters.AddWithValue("@Imagen", producto.Imagen ?? (object)DBNull.Value);
                    comando.Parameters.AddWithValue("@IdProducto", producto.IdProducto);

                    comando.ExecuteNonQuery();
                }
            }
        }

        //----------------------------FIN DE PRODUCTOS---------------------------


        //------------------------------ STOCK ---------------
        public void RestarStock(int idProducto, int cantidadVendida)
        {
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string query = "UPDATE productos SET stockActual = stockActual - @cantidad WHERE idProducto = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@cantidad", cantidadVendida);
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //------------------- VENTAS --------------------
public int GuardarVenta(Venta venta)
{
    using (MySqlConnection conn = new MySqlConnection(_connectionString))
    {
        conn.Open();

        string query = "INSERT INTO Ventas (diaVenta, montoVenta) VALUES (@dia, @monto)";
        MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@dia", venta.DiaVenta);
        cmd.Parameters.AddWithValue("@monto", venta.MontoVenta);
        cmd.ExecuteNonQuery();

        // Obtener el ID autogenerado
        return (int)cmd.LastInsertedId;
    }
}

public int ObtenerUltimaFactura()
{
    int ultimo = 0;
    using (MySqlConnection conn = ObtenerConexion())
    {
        conn.Open();
        string query = "SELECT MAX(idFactura) FROM Ventas";
        MySqlCommand cmd = new MySqlCommand(query, conn);
        object result = cmd.ExecuteScalar();
        if (result != DBNull.Value && result != null)
        {
            ultimo = Convert.ToInt32(result);
        }
    }
    return ultimo;
}

public int ObtenerStockActual(int idProducto)
{
    int stock = 0;

    using (MySqlConnection conn = new MySqlConnection(_connectionString))
    {
        conn.Open();
        string sql = "SELECT stockActual FROM productos WHERE idProducto = @idProducto";

        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            object result = cmd.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int cantidad))
            {
                stock = cantidad;
            }
        }
    }

    return stock;
}


    }
}