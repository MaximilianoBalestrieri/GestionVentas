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


        //--------------------- CLIENTES -------------------------------------
        
public List<Cliente> ObtenerClientes()
    {
        var lista = new List<Cliente>();
        using (var conn = new MySqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM Clientes", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    IdCliente = Convert.ToInt32(reader["idCliente"]),
                    DniCliente = reader["dniCliente"].ToString(),
                    NombreCliente = reader["nombreCliente"].ToString(),
                    Domicilio = reader["domicilio"].ToString(),
                    Localidad = reader["localidad"].ToString()
                });
            }
        }
        return lista;
    }

    public Cliente ObtenerClientePorId(int id)
    {
        Cliente cliente = null;
        using (var conn = new MySqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM Clientes WHERE idCliente = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                cliente = new Cliente
                {
                    IdCliente = Convert.ToInt32(reader["idCliente"]),
                    DniCliente = reader["dniCliente"].ToString(),
                    NombreCliente = reader["nombreCliente"].ToString(),
                    Domicilio = reader["domicilio"].ToString(),
                    Localidad = reader["localidad"].ToString()
                };
            }
        }
        return cliente;
    }

    public void AgregarCliente(Cliente cliente)
    {
        using (var conn = new MySqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO Clientes (dniCliente, nombreCliente, domicilio, localidad) VALUES (@dni, @nombre, @domicilio, @localidad)", conn);
            cmd.Parameters.AddWithValue("@dni", cliente.DniCliente);
            cmd.Parameters.AddWithValue("@nombre", cliente.NombreCliente);
            cmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
            cmd.Parameters.AddWithValue("@localidad", cliente.Localidad);
            cmd.ExecuteNonQuery();
        }
    }

    public void ActualizarCliente(Cliente cliente)
    {
        using (var conn = new MySqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new MySqlCommand("UPDATE Clientes SET dniCliente=@dni, nombreCliente=@nombre, domicilio=@domicilio, localidad=@localidad WHERE idCliente=@id", conn);
            cmd.Parameters.AddWithValue("@dni", cliente.DniCliente);
            cmd.Parameters.AddWithValue("@nombre", cliente.NombreCliente);
            cmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
            cmd.Parameters.AddWithValue("@localidad", cliente.Localidad);
            cmd.Parameters.AddWithValue("@id", cliente.IdCliente);
            cmd.ExecuteNonQuery();
        }
    }

    public void EliminarCliente(int id)
    {
        using (var conn = new MySqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM Clientes WHERE idCliente=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
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
                                Nombre = reader["Nombre"].ToString(),
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
    using (var conexion = ObtenerConexion())
    {
        conexion.Open();
        using (var transaccion = conexion.BeginTransaction())
        {
            try
            {
                // Insertar presupuesto
                var queryPresupuesto = "INSERT INTO Presupuesto (NombreCliente, TelefonoCliente, Fecha) VALUES (@nombre, @telefono, @fecha); SELECT LAST_INSERT_ID();";
                using (var comando = new MySqlCommand(queryPresupuesto, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@nombre", presupuesto.NombreCliente);
                    comando.Parameters.AddWithValue("@telefono", presupuesto.TelefonoCliente);
                    comando.Parameters.AddWithValue("@fecha", presupuesto.Fecha);

                    presupuesto.IdPresupuesto = Convert.ToInt32(comando.ExecuteScalar());
                }

                // Insertar items
                foreach (var item in presupuesto.Items)
                {
                    var queryItem = "INSERT INTO PresupuestoItem (IdPresupuesto, Nombre, Cantidad, PrecioUnitario) VALUES (@idPresupuesto, @nombre, @cantidad, @precio);";
                    using (var cmdItem = new MySqlCommand(queryItem, conexion, transaccion))
                    {
                        cmdItem.Parameters.AddWithValue("@idPresupuesto", presupuesto.IdPresupuesto);
                        cmdItem.Parameters.AddWithValue("@nombre", item.Nombre);
                        cmdItem.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        cmdItem.Parameters.AddWithValue("@precio", item.PrecioUnitario);

                        cmdItem.ExecuteNonQuery();
                    }
                }

                transaccion.Commit();
            }
            catch (Exception)
            {
                transaccion.Rollback();
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
        string sql = @"SELECT * 
                       FROM Productos ";

        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                productos.Add(new Productos
                {
                    IdProducto = Convert.ToInt32(reader["IdProducto"]),
                    Codigo = reader["Codigo"].ToString(),
                    Nombre = reader["Nombre"].ToString(),
                    Descripcion = reader["Descripcion"].ToString(),
                    Categoria = reader["Categoria"].ToString(),
                    PrecioCosto = Convert.ToDecimal(reader["PrecioCosto"]),
                    RecargoPorcentaje = Convert.ToDecimal(reader["RecargoPorcentaje"]),
                  //  PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                    StockActual = Convert.ToInt32(reader["StockActual"]),
                    StockMinimo = Convert.ToInt32(reader["StockMinimo"]),
                    NombreProveedor = reader["NombreProveedor"].ToString(),
                    Imagen = reader["Imagen"].ToString()
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
        (codigo, nombre, descripcion, categoria, precioCosto, recargoPorcentaje, precioVenta, stockActual, stockMinimo, nombreProveedor, imagen) 
        VALUES 
        (@codigo, @nombre, @descripcion, @categoria, @precioCosto, @recargoPorcentaje, @precioVenta, @stockActual, @stockMinimo, @nombreProveedor, @imagen)";

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
            cmd.Parameters.AddWithValue("@nombreProveedor", p.NombreProveedor);
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
        string sql = @"SELECT * 
                       FROM Productos p
                       WHERE p.IdProducto = @id";

        using (var comando = new MySqlCommand(sql, conexion))
        {
            comando.Parameters.AddWithValue("@id", id);
            using (var lector = comando.ExecuteReader())
            {
                if (lector.Read())
                {
                    prod = new Productos
                    {
                        IdProducto = Convert.ToInt32(lector["IdProducto"]),
                        Codigo = lector["Codigo"].ToString(),
                        Nombre = lector["Nombre"].ToString(),
                        Categoria = lector["Categoria"].ToString(),
                        Descripcion = lector["Descripcion"].ToString(),
                        PrecioCosto = Convert.ToDecimal(lector["PrecioCosto"]),
                        RecargoPorcentaje = Convert.ToDecimal(lector["RecargoPorcentaje"]),
                       // PrecioVenta = Convert.ToDecimal(lector["PrecioVenta"]),
                        StockActual = Convert.ToInt32(lector["StockActual"]),
                        StockMinimo = Convert.ToInt32(lector["StockMinimo"]),
                      
                        NombreProveedor = lector["NombreProveedor"].ToString(),
                        Imagen = lector["Imagen"].ToString()
                    };
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
                        nombreProveedor = @nombreProveedor,
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
           // comando.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
            comando.Parameters.AddWithValue("@StockActual", producto.StockActual);
            comando.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
            comando.Parameters.AddWithValue("@nombreProveedor", producto.NombreProveedor);
            comando.Parameters.AddWithValue("@Imagen", producto.Imagen ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@IdProducto", producto.IdProducto);

            comando.ExecuteNonQuery();
        }
    }
}

public List<Proveedor> ObtenerProveedores()
{
    var lista = new List<Proveedor>();
    using (var conexion = new MySqlConnection(_connectionString))
    {
        conexion.Open();
        var sql = "SELECT IdProv, Nombre FROM Proveedor";
        using (var comando = new MySqlCommand(sql, conexion))
        {
            using (var reader = comando.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Proveedor
                    {
                        IdProveedor = reader.GetInt32(0),
                        NombreProveedor = reader.GetString(1)
                    });
                }
            }
        }
    }
    return lista;
}

        //----------------------------OBTIENE EL ULTIMO NRO DE PRESUPUESTO---------------------------

        public int ObtenerMaximoNroPresupuesto()
        {
            int max = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT MAX(idPresupuesto) FROM Presupuesto";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        max = Convert.ToInt32(result);
                    }
                }
            }
            return max;
        }



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