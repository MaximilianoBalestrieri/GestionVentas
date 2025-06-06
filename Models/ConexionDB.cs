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
using Microsoft.AspNetCore.Mvc;


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
        //-------------------------- 
       public List<Venta> ObtenerVentasPorUsuario(string nombreUsuario)
{
    List<Venta> lista = new List<Venta>();

    using (MySqlConnection conexion = ObtenerConexion())
    {
        conexion.Open();
        string sql = "SELECT vendedor, montoVenta, diaVenta FROM facturas WHERE vendedor = @vendedor";

        using (MySqlCommand cmd = new MySqlCommand(sql, conexion))
        {
            cmd.Parameters.AddWithValue("@vendedor", nombreUsuario);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Venta v = new Venta
                    {
                        Vendedor = reader.GetString("vendedor"),
                        MontoVenta = reader.GetDecimal("montoVenta"),
                        DiaVenta = reader.GetDateTime("diaVenta")
                    };
                    lista.Add(v);
                }
            }
        }
    }

    return lista;
}






        //--------------------------PROVEEDORES --------------------------------
        public List<Proveedor> ObtenerProveedores()
        {
            var lista = new List<Proveedor>();

            using (var conexion = new MySqlConnection(_connectionString))
            {
                conexion.Open();
                var comando = new MySqlCommand("SELECT * FROM Proveedor", conexion);
                var reader = comando.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Proveedor
                    {
                        IdProv = Convert.ToInt32(reader["idProv"]),
                        Nombre = reader["nombre"].ToString(),
                        Telefono = reader["telefono"].ToString(),
                        Domicilio = reader["domicilio"].ToString(),
                        Localidad = reader["localidad"].ToString()
                    });
                }
            }

            return lista;
        }

    // Agregar proveedor
    public void AgregarProveedor(Proveedor prov)
    {
        using (var conexion = new MySqlConnection(_connectionString))
        {
            conexion.Open();
            var query = "INSERT INTO Proveedor (nombre, telefono, domicilio, localidad) VALUES (@nombre, @telefono, @domicilio, @localidad)";
            var comando = new MySqlCommand(query, conexion);
            comando.Parameters.AddWithValue("@nombre", prov.Nombre);
            comando.Parameters.AddWithValue("@telefono", prov.Telefono);
            comando.Parameters.AddWithValue("@domicilio", prov.Domicilio);
            comando.Parameters.AddWithValue("@localidad", prov.Localidad);
            comando.ExecuteNonQuery();
        }
    }

    // Actualizar proveedor
    public void ActualizarProveedor(Proveedor prov)
    {
        using (var conexion = new MySqlConnection(_connectionString))
        {
            conexion.Open();
            var query = "UPDATE Proveedor SET nombre = @nombre, telefono = @telefono, domicilio = @domicilio, localidad = @localidad WHERE idProv = @id";
            var comando = new MySqlCommand(query, conexion);
            comando.Parameters.AddWithValue("@id", prov.IdProv);
            comando.Parameters.AddWithValue("@nombre", prov.Nombre);
            comando.Parameters.AddWithValue("@telefono", prov.Telefono);
            comando.Parameters.AddWithValue("@domicilio", prov.Domicilio);
            comando.Parameters.AddWithValue("@localidad", prov.Localidad);
            comando.ExecuteNonQuery();
        }
    }

    // Eliminar proveedor
    public void EliminarProveedor(int id)
    {
        using (var conexion = new MySqlConnection(_connectionString))
        {
            conexion.Open();
            var comando = new MySqlCommand("DELETE FROM Proveedor WHERE idProv = @id", conexion);
            comando.Parameters.AddWithValue("@id", id);
            comando.ExecuteNonQuery();
        }
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
                        Localidad = reader["localidad"].ToString(),
                        TelefonoCliente = reader["telefonoCliente"].ToString()
                    });
                }
            }
            return lista;
        }

        public List<Cliente> ObtenerTodosLosClientes()
        {
            List<Cliente> lista = new List<Cliente>();

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                conexion.Open();
                string query = "SELECT * FROM Clientes";

                using (MySqlCommand comando = new MySqlCommand(query, conexion))
                using (MySqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Cliente c = new Cliente()
                        {
                            IdCliente = Convert.ToInt32(reader["idCliente"]),
                            DniCliente = reader["dniCliente"].ToString(),
                            NombreCliente = reader["nombreCliente"].ToString(),
                            Domicilio = reader["domicilio"].ToString(),
                            Localidad = reader["localidad"].ToString(),
                            TelefonoCliente = reader["telefonoCliente"].ToString()
                        };

                        lista.Add(c);
                    }
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
                        Localidad = reader["localidad"].ToString(),
                        TelefonoCliente = reader["telefonoCliente"].ToString()
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
                var cmd = new MySqlCommand("INSERT INTO Clientes (dniCliente, nombreCliente, domicilio, localidad, telefonoCliente) VALUES (@dni, @nombre, @domicilio, @localidad, @telefonoCliente)", conn);
                cmd.Parameters.AddWithValue("@dni", cliente.DniCliente);
                cmd.Parameters.AddWithValue("@nombre", cliente.NombreCliente);
                cmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
                cmd.Parameters.AddWithValue("@localidad", cliente.Localidad);
                cmd.Parameters.AddWithValue("@telefonoCliente", cliente.TelefonoCliente);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarCliente(Cliente cliente)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE Clientes SET dniCliente=@dni, nombreCliente=@nombre, domicilio=@domicilio, localidad=@localidad, telefonoCliente=@telefonoCliente WHERE idCliente=@id", conn);
                cmd.Parameters.AddWithValue("@dni", cliente.DniCliente);
                cmd.Parameters.AddWithValue("@nombre", cliente.NombreCliente);
                cmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
                cmd.Parameters.AddWithValue("@localidad", cliente.Localidad);
                cmd.Parameters.AddWithValue("@telefonoCliente", cliente.TelefonoCliente);
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


        public (bool success, int idFactura, string error) RegistrarVenta(VentaCompleta venta)
        {
            Console.WriteLine("🚀 RegistrarVenta fue llamado.");
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int idFactura = 0;

                        string insertFactura = @"
                    INSERT INTO facturas (diaVenta, montoVenta, vendedor, idCliente)
                    VALUES (@diaVenta, @montoVenta, @vendedor, @idCliente);";

                        using (var cmdInsert = new MySqlCommand(insertFactura, conn, transaction))
                        {

                            cmdInsert.Parameters.AddWithValue("@diaVenta", DateTime.Now);
                            cmdInsert.Parameters.AddWithValue("@montoVenta", venta.MontoVenta);
                            cmdInsert.Parameters.AddWithValue("@vendedor", venta.Vendedor);
                            cmdInsert.Parameters.AddWithValue("@idCliente", venta.IdCliente);


                            Console.WriteLine("---- DATOS PARA FACTURA ----");
                            Console.WriteLine("Día de venta: " + DateTime.Now);
                            Console.WriteLine("Monto: " + venta.MontoVenta);
                            Console.WriteLine("Vendedor: " + venta.Vendedor);
                            Console.WriteLine("ID Cliente: " + venta.IdCliente);


                            cmdInsert.ExecuteNonQuery();

                            // Obtener ID generado
                            using (var cmdGetId = new MySqlCommand("SELECT LAST_INSERT_ID();", conn, transaction))
                            {
                                idFactura = Convert.ToInt32(cmdGetId.ExecuteScalar());
                            }

                            Console.WriteLine("ID FACTURA generado: " + idFactura);
                        }

                        if (idFactura <= 0)
                            throw new Exception("No se generó el ID de la factura.");

                        foreach (var item in venta.Productos)
                        {
                            // Insertar item de factura
                            string insertItem = @"
                        INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio)
                        VALUES (@idFactura, @idItem, @nombreProd, @cantidad, @precio);";

                            using (var cmdItem = new MySqlCommand(insertItem, conn, transaction))
                            {
                                cmdItem.Parameters.AddWithValue("@idFactura", idFactura);
                                cmdItem.Parameters.AddWithValue("@idItem", item.IdProducto);
                                cmdItem.Parameters.AddWithValue("@nombreProd", item.NombreProd);
                                cmdItem.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmdItem.Parameters.AddWithValue("@precio", item.Precio);
                                cmdItem.ExecuteNonQuery();
                            }

                            // Actualizar stock
                            string restarStock = "UPDATE productos SET stockActual = stockActual - @cantidad WHERE idProducto = @id";
                            using (var cmdStock = new MySqlCommand(restarStock, conn, transaction))
                            {
                                cmdStock.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmdStock.Parameters.AddWithValue("@id", item.IdProducto);
                                cmdStock.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return (true, idFactura, "");

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, 0, ex.Message);
                    }
                }
            }
        }


        public int ObtenerProximoAutoIncremento(string facturas, string gestionventas)
        {
            int autoIncrement = 1;

            using (var conn = ObtenerConexion())
            {
                conn.Open();

                string sql = @"
            SELECT AUTO_INCREMENT
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = @gestionventas
            AND TABLE_NAME = @facturas;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@gestionventas", gestionventas);
                    cmd.Parameters.AddWithValue("@facturas", facturas);

                    var result = cmd.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        autoIncrement = Convert.ToInt32(result);
                    }
                }
            }

            return autoIncrement;
        }



        public void GuardarItemFactura(int idFactura, int idItem, string nombreProd, int cantidad, decimal precio)
        {
            try
            {
                using (MySqlConnection conn = ObtenerConexion())
                {
                    conn.Open();

                    string query = @"INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio) 
                             VALUES (@idFactura, @idItem, @nombreProd, @cantidad, @precio);";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idFactura", idFactura);
                        cmd.Parameters.AddWithValue("@idItem", idItem);
                        cmd.Parameters.AddWithValue("@nombreProd", nombreProd);
                        cmd.Parameters.AddWithValue("@cantidad", cantidad);
                        cmd.Parameters.AddWithValue("@precio", precio);

                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("✅ Item factura guardado OK.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR al guardar item factura: " + ex.ToString());
                throw; // Volvemos a lanzar para que el controlador lo atrape
            }
        }


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


        public List<Factura> ObtenerFacturas()
        {
            List<Factura> lista = new List<Factura>();

            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string query = @"
            SELECT f.idFactura, f.diaVenta, f.montoVenta, f.vendedor, c.nombreCliente AS nombreCliente
            FROM facturas f
            JOIN clientes c ON f.idCliente = c.idCliente
            ORDER BY f.idFactura DESC
            ";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Factura
                        {
                            IdFactura = Convert.ToInt32(reader["idFactura"]),
                            DiaVenta = Convert.ToDateTime(reader["diaVenta"]),
                            MontoVenta = Convert.ToDecimal(reader["montoVenta"]),
                            Vendedor = reader["vendedor"].ToString(),
                            NombreCliente = reader["nombreCliente"].ToString()
                        });
                    }
                }
            }

            return lista;
        }


        public Factura ObtenerFacturaConItems(int idFactura)
        {
            Factura factura = null;

            using (var conn = ObtenerConexion())
            {
                conn.Open();

                string queryFactura = @"
            SELECT f.idFactura, f.diaVenta, f.montoVenta, f.vendedor, c.nombreCliente, c.dniCliente, c.domicilio, c.localidad, c.telefonoCliente
            FROM facturas f
            JOIN clientes c ON f.idCliente = c.idCliente
            WHERE f.idFactura = @id";

                using (var cmd = new MySqlCommand(queryFactura, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            factura = new Factura
                            {
                                IdFactura = Convert.ToInt32(reader["idFactura"]),
                                DiaVenta = Convert.ToDateTime(reader["diaVenta"]),
                                MontoVenta = Convert.ToDecimal(reader["montoVenta"]),
                                Vendedor = reader["vendedor"].ToString(),
                                NombreCliente = reader["nombreCliente"].ToString(),
                                DniCliente = reader["dniCliente"].ToString(),
                                Domicilio = reader["domicilio"].ToString(),
                                Localidad = reader["localidad"].ToString(),
                                TelefonoCliente = reader["telefonoCliente"].ToString(),
                                Items = new List<FacturaItem>()
                            };
                        }
                    }
                }

                if (factura != null)
                {
                    string queryItems = @"
                SELECT fi.nombreProd, fi.cantidad, fi.precio
                FROM facturaitem fi
                WHERE fi.idFactura = @id";

                    using (var cmd = new MySqlCommand(queryItems, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idFactura);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                factura.Items.Add(new FacturaItem
                                {
                                    NombreProd = reader["nombreProd"].ToString(),
                                    Cantidad = Convert.ToInt32(reader["cantidad"]),
                                    Precio = Convert.ToDecimal(reader["precio"])
                                });
                            }
                        }
                    }
                }
            }

            return factura;
        }


        public void EliminarFactura(int idFactura)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                // Primero eliminamos los items asociados
                using (var cmd = new MySqlCommand("DELETE FROM facturaitem WHERE idFactura = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    cmd.ExecuteNonQuery();
                }

                // Luego eliminamos la factura
                using (var cmd = new MySqlCommand("DELETE FROM facturas WHERE idFactura = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    cmd.ExecuteNonQuery();
                }
            }
        }


       


    }
}