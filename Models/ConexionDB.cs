using System;
using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using GestionVentas.Models;

namespace GestionVentas.Models
{
    // Agregamos ": DbContext" para que el controlador pueda usar Cajas y SaveChanges
    public class ConexionDB : DbContext
    {
        private readonly string _connectionString;

        // CONSTRUCTOR COMPATIBLE:
        // Mantenemos el IConfiguration para que tus métodos manuales sigan funcionando
        public ConexionDB(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // Este constructor es para que Entity Framework (la Caja) no de errores
        public ConexionDB(DbContextOptions<ConexionDB> options, IConfiguration config)
            : base(options)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // --- SECCIÓN DE TABLAS (Solo esto es lo "nuevo") ---
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }

        // --- TU MÉTODO DE SIEMPRE (No se toca) ---
        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_connectionString);
        }

        // Configuramos EF para que use la misma conexión que tus otros controladores
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }





        //-------------------------- 
        public List<Venta> ObtenerVentasPorUsuario(string nombreUsuario)
        {
            List<Venta> lista = new List<Venta>();

            using (SqlConnection conexion = ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT idFactura, vendedor, montoVenta, diaVenta, idCliente FROM facturas WHERE vendedor = @vendedor";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@vendedor", nombreUsuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Venta v = new Venta
                            {
                                // --- CAMBIOS APLICADOS AQUÍ ---
                                // Reemplazar reader.GetTipo("Nombre") por reader.GetTipo(reader.GetOrdinal("Nombre"))

                                IdFactura = reader.GetInt32(reader.GetOrdinal("idFactura")),
                                DiaVenta = reader.GetDateTime(reader.GetOrdinal("diaVenta")),
                                MontoVenta = reader.GetDecimal(reader.GetOrdinal("montoVenta")),
                                Vendedor = reader.GetString(reader.GetOrdinal("vendedor")),
                                IdCliente = reader.GetInt32(reader.GetOrdinal("idCliente"))
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

            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var comando = new SqlCommand("SELECT * FROM Proveedor", conexion);
                var reader = comando.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Proveedor
                    {
                        // Mapear la columna 'idProv' a la propiedad 'IdProveedor'
                        IdProveedor = Convert.ToInt32(reader["idProv"]),

                        // Mapear la columna 'nombre' a la propiedad 'NombreProveedor'
                        NombreProveedor = reader["nombre"].ToString(),

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
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var query = "INSERT INTO Proveedor (nombre, telefono, domicilio, localidad) VALUES (@nombre, @telefono, @domicilio, @localidad)";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@nombre", prov.NombreProveedor);
                comando.Parameters.AddWithValue("@telefono", prov.Telefono);
                comando.Parameters.AddWithValue("@domicilio", prov.Domicilio);
                comando.Parameters.AddWithValue("@localidad", prov.Localidad);
                comando.ExecuteNonQuery();
            }
        }

        // Actualizar proveedor
        public void ActualizarProveedor(Proveedor prov)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var query = "UPDATE Proveedor SET nombre = @nombre, telefono = @telefono, domicilio = @domicilio, localidad = @localidad WHERE idProv = @id";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@id", prov.IdProveedor);
                comando.Parameters.AddWithValue("@nombre", prov.NombreProveedor);
                comando.Parameters.AddWithValue("@telefono", prov.Telefono);
                comando.Parameters.AddWithValue("@domicilio", prov.Domicilio);
                comando.Parameters.AddWithValue("@localidad", prov.Localidad);
                comando.ExecuteNonQuery();
            }
        }

        // Eliminar proveedor
        public void EliminarProveedor(int id)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var comando = new SqlCommand("DELETE FROM Proveedor WHERE idProv = @id", conexion);
                comando.Parameters.AddWithValue("@id", id);
                comando.ExecuteNonQuery();
            }
        }

        //--------------------- CLIENTES -------------------------------------

        public List<Cliente> ObtenerClientes()
        {
            var lista = new List<Cliente>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Clientes", conn);
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

            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                string query = "SELECT * FROM Clientes";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                using (SqlDataReader reader = comando.ExecuteReader())
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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Clientes WHERE idCliente = @id", conn);
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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Clientes (dniCliente, nombreCliente, domicilio, localidad, telefonoCliente) VALUES (@dni, @nombre, @domicilio, @localidad, @telefonoCliente)", conn);
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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Clientes SET dniCliente=@dni, nombreCliente=@nombre, domicilio=@domicilio, localidad=@localidad, telefonoCliente=@telefonoCliente WHERE idCliente=@id", conn);
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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Clientes WHERE idCliente=@id", conn);
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
                        //var queryPresupuesto = "INSERT INTO Presupuesto (NombreCliente, TelefonoCliente, Fecha) VALUES (@nombre, @telefono, @fecha); SELECT LAST_INSERT_ID();";
                        var queryPresupuesto = "INSERT INTO Presupuesto (NombreCliente, TelefonoCliente, Fecha) VALUES (@nombre, @telefono, @fecha); SELECT SCOPE_IDENTITY();";
                        using (var comando = new SqlCommand(queryPresupuesto, conexion, transaccion))
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
                            using (var cmdItem = new SqlCommand(queryItem, conexion, transaccion))
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




        // 1. VENTAS POR FECHA (DIARIO)
        public List<object> ObtenerTotalVentasPorFecha(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                // Usamos CAST en el WHERE para que SQL no se confunda con las horas del servidor
                var cmd = new SqlCommand(@"
            SELECT CAST(diaVenta AS DATE) as fecha, SUM(montoVenta) as totalVendido
            FROM facturas
            WHERE CAST(diaVenta AS DATE) >= CAST(@desde AS DATE) 
              AND CAST(diaVenta AS DATE) <= CAST(@hasta AS DATE)
            GROUP BY CAST(diaVenta AS DATE)
            ORDER BY fecha", conn);

                // Enviamos las fechas tal cual vienen de la vista
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

        // 2. VENTAS POR MES
        public List<object> ObtenerTotalVentasPorMes(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT 
                DATEFROMPARTS(YEAR(diaVenta), MONTH(diaVenta), 1) as fecha, 
                SUM(montoVenta) as totalVendido
            FROM facturas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY YEAR(diaVenta), MONTH(diaVenta)
            ORDER BY fecha", conn);

                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddTicks(-1));

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

        // 3. VENTAS POR AÑO
        public List<object> ObtenerTotalVentasPorAnio(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT 
                DATEFROMPARTS(YEAR(diaVenta), 1, 1) as fecha, 
                SUM(montoVenta) as totalVendido
            FROM facturas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY YEAR(diaVenta)
            ORDER BY fecha", conn);

                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddTicks(-1));

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


        //---- USUARIOS ------

        public Usuario ObtenerUsuarioPorNombre(string nombreUsuario)
        {
            Usuario u = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT IdUsuario, Usuario, NombreYApellido, Rol, Contraseña, FotoPerfil FROM Usuarios WHERE Usuario = @nombre";

                using (var command = new SqlCommand(query, connection))
                {
                    // Nota: Se recomienda usar Add para tipado más estricto, pero AddWithValue funciona
                    command.Parameters.AddWithValue("@nombre", nombreUsuario);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = new Usuario
                            {
                                // ❌ ANTES (Error): IdUsuario = reader.GetInt32("IdUsuario"),
                                // ✅ AHORA: Usamos GetOrdinal para obtener el índice y pasarlo a GetInt32
                                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),

                                // ✅ AHORA: Usamos GetOrdinal para los strings también, aunque hay otra forma
                                UsuarioNombre = reader.GetString(reader.GetOrdinal("Usuario")),
                                NombreyApellido = reader.GetString(reader.GetOrdinal("NombreYApellido")),
                                Rol = reader.GetString(reader.GetOrdinal("Rol")),
                                Contraseña = reader.GetString(reader.GetOrdinal("Contraseña")),

                                // Mantenemos la lógica de verificación de nulos (que es correcta aquí)
                                FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("FotoPerfil")) : null
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
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET FotoPerfil = @fotoperfil WHERE idUsuario = @idUsuario";
                using (var command = new SqlCommand(query, connection))
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
                var cmd = new SqlCommand("SELECT * FROM usuarios WHERE idUsuario = @idUsuario", con);
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
                        FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("FotoPerfil")) : null
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
                var cmd = new SqlCommand("INSERT INTO usuarios (Usuario, contraseña, rol, nombreyApellido, FotoPerfil) VALUES (@nombre,  @contraseña, @rol, @nombreyApellido, @FotoPerfil)", con);
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
                var cmd = new SqlCommand("UPDATE usuarios SET Usuario=@nombre, contraseña=@contraseña, rol=@rol, nombreyApellido=@nombreyApellido, FotoPerfil=@FotoPerfil WHERE idUsuario=@idUsuario", con);

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
                var cmd = new SqlCommand("DELETE FROM usuarios WHERE idUsuario=@id", con);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }



        public void ActualizarClave(Usuario u)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET contraseña = @contraseña WHERE IdUsuario = @id";
                using (var command = new SqlCommand(query, connection))
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
                var cmd = new SqlCommand("SELECT idUsuario, Usuario, contraseña, rol, nombreyApellido, FotoPerfil FROM Usuarios", con);
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string sql = "SELECT * FROM usuarios WHERE usuario = @usuario AND contraseña = @contraseña";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = @"UPDATE Usuario SET 
                        UsuarioNombre = @usuario, 
                        NombreyApellido = @nombre, 
                        Contraseña = @pass, 
                        Rol = @rol, 
                        FotoPerfil = @foto
                    WHERE IdUsuario = @id";
                using (var cmd = new SqlCommand(sql, conn))
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

            using (SqlConnection conn = ObtenerConexion())
            {
                conn.Open();

                // La consulta SQL explícita es crucial (sin PrecioVenta, ya que es calculado en C#).
                string sql = @"
            SELECT 
                IdProducto, Codigo, Nombre, Descripcion, Categoria, 
                PrecioCosto, RecargoPorcentaje, 
                StockActual, StockMinimo, NombreProveedor, Imagen 
            FROM Productos";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // --- 1. Obtener los índices de las columnas (Ordinales) ---
                    int ordIdProducto = reader.GetOrdinal("IdProducto");
                    int ordCodigo = reader.GetOrdinal("Codigo");
                    int ordNombre = reader.GetOrdinal("Nombre");
                    int ordDescripcion = reader.GetOrdinal("Descripcion");
                    int ordCategoria = reader.GetOrdinal("Categoria");
                    int ordPrecioCosto = reader.GetOrdinal("PrecioCosto");
                    int ordRecargoPorcentaje = reader.GetOrdinal("RecargoPorcentaje");
                    int ordStockActual = reader.GetOrdinal("StockActual");
                    int ordStockMinimo = reader.GetOrdinal("StockMinimo");
                    int ordNombreProveedor = reader.GetOrdinal("NombreProveedor");
                    int ordImagen = reader.GetOrdinal("Imagen");

                    while (reader.Read())
                    {
                        productos.Add(new Productos
                        {
                            IdProducto = reader.GetInt32(ordIdProducto),
                            Codigo = reader.GetString(ordCodigo),
                            Nombre = reader.GetString(ordNombre),

                            // Manejo de nulos para strings
                            Descripcion = reader.IsDBNull(ordDescripcion) ? null : reader.GetString(ordDescripcion),
                            Categoria = reader.IsDBNull(ordCategoria) ? null : reader.GetString(ordCategoria),

                            // CORRECCIÓN CRÍTICA: Uso de GetValue y Convert.ToDecimal para evitar InvalidCastException (INT a DECIMAL)
                            PrecioCosto = reader.IsDBNull(ordPrecioCosto) ? 0.00M : Convert.ToDecimal(reader.GetValue(ordPrecioCosto)),
                            RecargoPorcentaje = reader.IsDBNull(ordRecargoPorcentaje) ? 0.00M : Convert.ToDecimal(reader.GetValue(ordRecargoPorcentaje)),

                            // Manejo de nulos para enteros
                            StockActual = reader.IsDBNull(ordStockActual) ? 0 : reader.GetInt32(ordStockActual),
                            StockMinimo = reader.IsDBNull(ordStockMinimo) ? 0 : reader.GetInt32(ordStockMinimo),

                            NombreProveedor = reader.IsDBNull(ordNombreProveedor) ? null : reader.GetString(ordNombreProveedor),
                            Imagen = reader.IsDBNull(ordImagen) ? null : reader.GetString(ordImagen)
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

                using (var cmd = new SqlCommand(query, con))
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

                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@IdProducto", idProducto);
                    comando.ExecuteNonQuery();
                }
            }
        }


        public Productos ObtenerProductoPorId(int id)
        {
            Productos prod = null;

            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                // La consulta SQL es explícita y excluye PrecioVenta (porque es calculada)
                string sql = @"
            SELECT 
                IdProducto, Codigo, Nombre, Descripcion, Categoria, 
                PrecioCosto, RecargoPorcentaje, 
                StockActual, StockMinimo, NombreProveedor, Imagen 
            FROM Productos p 
            WHERE p.IdProducto = @id";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            // --- 1. Obtener Ordinales ---
                            int ordIdProducto = lector.GetOrdinal("IdProducto");
                            int ordCodigo = lector.GetOrdinal("Codigo");
                            int ordNombre = lector.GetOrdinal("Nombre");
                            int ordCategoria = lector.GetOrdinal("Categoria");
                            int ordDescripcion = lector.GetOrdinal("Descripcion");
                            int ordPrecioCosto = lector.GetOrdinal("PrecioCosto");
                            int ordRecargoPorcentaje = lector.GetOrdinal("RecargoPorcentaje");
                            int ordStockActual = lector.GetOrdinal("StockActual");
                            int ordStockMinimo = lector.GetOrdinal("StockMinimo");
                            int ordNombreProveedor = lector.GetOrdinal("NombreProveedor");
                            int ordImagen = lector.GetOrdinal("Imagen");

                            prod = new Productos
                            {
                                // Lectura de campos básicos
                                IdProducto = lector.GetInt32(ordIdProducto),
                                Codigo = lector.GetString(ordCodigo),
                                Nombre = lector.GetString(ordNombre),

                                // Manejo de nulos (Strings)
                                Categoria = lector.IsDBNull(ordCategoria) ? null : lector.GetString(ordCategoria),
                                Descripcion = lector.IsDBNull(ordDescripcion) ? null : lector.GetString(ordDescripcion),

                                // CORRECCIÓN CRÍTICA (INT a DECIMAL): Uso de GetValue y Convert.ToDecimal
                                PrecioCosto = lector.IsDBNull(ordPrecioCosto) ? 0.00M : Convert.ToDecimal(lector.GetValue(ordPrecioCosto)),
                                RecargoPorcentaje = lector.IsDBNull(ordRecargoPorcentaje) ? 0.00M : Convert.ToDecimal(lector.GetValue(ordRecargoPorcentaje)),
                                // NOTA: PrecioVenta no se asigna, ya que es calculado en el modelo.

                                // Manejo de nulos (Enteros)
                                StockActual = lector.IsDBNull(ordStockActual) ? 0 : lector.GetInt32(ordStockActual),
                                StockMinimo = lector.IsDBNull(ordStockMinimo) ? 0 : lector.GetInt32(ordStockMinimo),

                                NombreProveedor = lector.IsDBNull(ordNombreProveedor) ? null : lector.GetString(ordNombreProveedor),
                                Imagen = lector.IsDBNull(ordImagen) ? null : lector.GetString(ordImagen)
                            };
                        }
                    }
                }
            }

            return prod;
        }


        public void ActualizarProducto(Productos producto)
        {
            using (var conexion = new SqlConnection(_connectionString))
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

                using (var comando = new SqlCommand(sql, conexion))
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
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT MAX(idPresupuesto) FROM Presupuesto";
                using (var cmd = new SqlCommand(query, connection))
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
                using (var cmd = new SqlCommand(query, conn))
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
            Console.WriteLine("🚀 RegistrarVenta con Medios de Pago iniciado.");
            using (var conn = ObtenerConexion())
            {
                conn.Open();

                // --- PASO A: BUSCAR LA CAJA ABIERTA ACTUAL ---
                int idCajaActual = 0;
                string sqlCaja = "SELECT TOP 1 Id FROM Cajas WHERE EstaAbierta = 1 ORDER BY FechaApertura DESC";
                using (var cmdCaja = new SqlCommand(sqlCaja, conn))
                {
                    var res = cmdCaja.ExecuteScalar();
                    if (res != null) idCajaActual = Convert.ToInt32(res);
                }

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int idFactura = 0;

                        // 1. INSERTAR FACTURA (Agregamos medioPago y tipoVenta)
                        string insertFactura = @"
                    INSERT INTO facturas (diaVenta, montoVenta, vendedor, idCliente, medioPago, tipoVenta)
                    VALUES (@diaVenta, @montoVenta, @vendedor, @idCliente, @medioPago, @tipoVenta);
                    SELECT SCOPE_IDENTITY();";

                        using (var cmdInsert = new SqlCommand(insertFactura, conn, transaction))
                        {
                            cmdInsert.Parameters.AddWithValue("@diaVenta", DateTime.Now);
                            cmdInsert.Parameters.AddWithValue("@montoVenta", venta.MontoVenta);
                            cmdInsert.Parameters.AddWithValue("@vendedor", venta.Vendedor ?? (object)DBNull.Value);
                            cmdInsert.Parameters.AddWithValue("@idCliente", venta.IdCliente);
                            cmdInsert.Parameters.AddWithValue("@medioPago", venta.MedioPago);
                            cmdInsert.Parameters.AddWithValue("@tipoVenta", venta.TipoVenta);

                            idFactura = Convert.ToInt32(cmdInsert.ExecuteScalar());
                        }

                        // 2. INSERTAR ITEMS Y ACTUALIZAR STOCK
                        foreach (var item in venta.Items)
                        {
                            string insertItem = @"
        INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio)
        VALUES (@idFactura, @idItem, @nombreProd, @cantidad, @precio);"; // Agregamos nombreProd

                            using (var cmdItem = new SqlCommand(insertItem, conn, transaction))
                            {
                                cmdItem.Parameters.AddWithValue("@idFactura", idFactura);
                                cmdItem.Parameters.AddWithValue("@idItem", item.IdProducto);
                                cmdItem.Parameters.AddWithValue("@nombreProd", item.NombreProd ?? "Producto"); // <--- Línea clave
                                cmdItem.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmdItem.Parameters.AddWithValue("@precio", item.Precio);
                                cmdItem.ExecuteNonQuery();
                            }

                            string restarStock = "UPDATE productos SET stockActual = stockActual - @cantidad WHERE idProducto = @id";
                            using (var cmdStock = new SqlCommand(restarStock, conn, transaction))
                            {
                                cmdStock.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmdStock.Parameters.AddWithValue("@id", item.IdProducto);
                                cmdStock.ExecuteNonQuery();
                            }
                        }

                        // 3. REGISTRAR MOVIMIENTO DE CAJA (SOLO SI ES EFECTIVO)
                        if (idCajaActual > 0)
                        {
    string insertMov = @"
        INSERT INTO MovimientosCaja (CajaId, Tipo, Monto, Fecha, Concepto, Usuario) 
        VALUES (@cajaId, @tipo, @monto, @fecha, @concepto, @usuario)";

    using (var cmdMov = new SqlCommand(insertMov, conn, transaction))
    {
        cmdMov.Parameters.AddWithValue("@cajaId", idCajaActual);
        cmdMov.Parameters.AddWithValue("@tipo", 0); // 0 = Ingreso
        cmdMov.Parameters.AddWithValue("@monto", venta.MontoVenta);
        cmdMov.Parameters.AddWithValue("@fecha", DateTime.Now);
        // Aquí aclaramos el medio de pago en el concepto
        cmdMov.Parameters.AddWithValue("@concepto", $"Venta ({venta.MedioPago}) Fac. Nro " + idFactura);
        cmdMov.Parameters.AddWithValue("@usuario", venta.Vendedor ?? "Sistema");
        cmdMov.ExecuteNonQuery();
    }

    // Actualizamos el MontoEsperado de la caja (Total general)
    string sqlUpdateCaja = "UPDATE Cajas SET MontoEsperado = MontoEsperado + @monto WHERE Id = @idCaja";
    using (var cmdUpd = new SqlCommand(sqlUpdateCaja, conn, transaction))
    {
        cmdUpd.Parameters.AddWithValue("@monto", venta.MontoVenta);
        cmdUpd.Parameters.AddWithValue("@idCaja", idCajaActual);
        cmdUpd.ExecuteNonQuery();
    }
}

                        // 4. SI ES CUENTA CORRIENTE (FIADO), ACTUALIZAR SALDO CLIENTE
                        if (venta.MedioPago == "CtaCte")
                        {
                            // Registramos la deuda en una tabla de movimientos de cliente (opcional pero recomendado)
                            // Y actualizamos el saldo del cliente
                            string sqlSaldo = "UPDATE Clientes SET SaldoActual = ISNULL(SaldoActual, 0) + @monto WHERE idCliente = @idCli";
                            using (var cmdSaldo = new SqlCommand(sqlSaldo, conn, transaction))
                            {
                                cmdSaldo.Parameters.AddWithValue("@monto", venta.MontoVenta);
                                cmdSaldo.Parameters.AddWithValue("@idCli", venta.IdCliente);
                                cmdSaldo.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return (true, idFactura, "");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("❌ Error en DB: " + ex.Message);
                        return (false, 0, ex.Message);
                    }
                }
            }
        }
        public List<object> ObtenerBalanceMovimientosPorFecha(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT CAST(Fecha AS DATE), SUM(CASE WHEN Tipo = 0 THEN Monto ELSE -Monto END)
                       FROM MovimientosCaja
                       WHERE CAST(Fecha AS DATE) BETWEEN @desde AND @hasta AND Concepto NOT LIKE '%Apertura%'
                       GROUP BY CAST(Fecha AS DATE) ORDER BY CAST(Fecha AS DATE) ASC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.Date);
                    cmd.Parameters.AddWithValue("@hasta", hasta.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                Fecha = Convert.ToDateTime(reader[0]).ToString("yyyy-MM-dd"),
                                Total = Convert.ToDecimal(reader[1])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<object> ObtenerBalanceMovimientosPorMes(DateTime desde, DateTime hasta)
        {
            // Forzamos rango del mes completo
            DateTime inicio = new DateTime(desde.Year, desde.Month, 1);
            DateTime fin = new DateTime(hasta.Year, hasta.Month, 1).AddMonths(1).AddDays(-1);

            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT MONTH(Fecha), YEAR(Fecha), SUM(CASE WHEN Tipo = 0 THEN Monto ELSE -Monto END)
                       FROM MovimientosCaja
                       WHERE CAST(Fecha AS DATE) BETWEEN @desde AND @hasta AND Concepto NOT LIKE '%Apertura%'
                       GROUP BY YEAR(Fecha), MONTH(Fecha) ORDER BY YEAR(Fecha), MONTH(Fecha)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", inicio);
                    cmd.Parameters.AddWithValue("@hasta", fin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                Fecha = reader[0].ToString().PadLeft(2, '0') + "/" + reader[1].ToString(),
                                Total = Convert.ToDecimal(reader[2])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<object> ObtenerBalanceMovimientosPorAnio(DateTime desde, DateTime hasta)
        {
            // Forzamos rango del año completo
            DateTime inicio = new DateTime(desde.Year, 1, 1);
            DateTime fin = new DateTime(hasta.Year, 12, 31);

            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT YEAR(Fecha), SUM(CASE WHEN Tipo = 0 THEN Monto ELSE -Monto END)
                       FROM MovimientosCaja
                       WHERE CAST(Fecha AS DATE) BETWEEN @desde AND @hasta AND Concepto NOT LIKE '%Apertura%'
                       GROUP BY YEAR(Fecha) ORDER BY YEAR(Fecha)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", inicio);
                    cmd.Parameters.AddWithValue("@hasta", fin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                Fecha = reader[0].ToString(),
                                Total = Convert.ToDecimal(reader[1])
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public int ObtenerProximoAutoIncremento(string tabla, string baseDeDatos)
        {
            // El argumento 'baseDeDatos' (gestionventas) ya no es necesario para IDENT_CURRENT
            // pero lo dejamos en la firma del método para no romper otras llamadas.

            using (var conn = ObtenerConexion())
            {
                conn.Open();

                // *** Consulta SQL Server (Reemplazando la de MySQL) ***
                // IDENT_CURRENT devuelve el último valor de identidad generado para la tabla.
                string sql = "SELECT IDENT_CURRENT(@tabla);";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Usamos el parámetro @tabla para pasar el nombre 'facturas'
                    cmd.Parameters.AddWithValue("@tabla", tabla);

                    var result = cmd.ExecuteScalar();

                    // Si la tabla está vacía, IDENT_CURRENT puede devolver NULL.
                    if (result == DBNull.Value || result == null)
                    {
                        // Si no hay filas, el primer ID será 1.
                        return 1;
                    }
                    else
                    {
                        // El resultado es el ÚLTIMO ID usado. El próximo será el resultado + 1.
                        int ultimoId = Convert.ToInt32(result);
                        return ultimoId + 1;
                    }
                }
            }
        }


        public void GuardarItemFactura(int idFactura, int idItem, string nombreProd, int cantidad, decimal precio)
        {
            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();

                    string query = @"INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio) 
                             VALUES (@idFactura, @idItem, @nombreProd, @cantidad, @precio);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. CAMBIO: Añadimos "; SELECT SCOPE_IDENTITY();" a la query
                string query = "INSERT INTO Ventas (diaVenta, montoVenta) VALUES (@dia, @monto); SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@dia", venta.DiaVenta);
                cmd.Parameters.AddWithValue("@monto", venta.MontoVenta);

                // 2. CAMBIO: Usamos ExecuteScalar y convertimos el resultado a int
                // Este comando ejecuta el INSERT y luego el SELECT, devolviendo el ID.
                object result = cmd.ExecuteScalar();

                // Verificamos si la operación fue exitosa antes de convertir
                if (result != null && result != DBNull.Value)
                {
                    // El ID devuelto es un decimal, por lo que Convert.ToInt32 es seguro.
                    return Convert.ToInt32(result);
                }

                // Si no se pudo obtener el ID (ej: la tabla no tiene IDENTITY), devolvemos 0 o -1
                return -1;
            }
        }

        public int ObtenerUltimaFactura()
        {
            int ultimo = 0;
            using (SqlConnection conn = ObtenerConexion())
            {
                conn.Open();
                string query = "SELECT MAX(idFactura) FROM Ventas";
                SqlCommand cmd = new SqlCommand(query, conn);
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

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT stockActual FROM productos WHERE idProducto = @idProducto";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
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

                using (var cmd = new SqlCommand(query, conn))
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

                using (var cmd = new SqlCommand(queryFactura, conn))
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

                    using (var cmd = new SqlCommand(queryItems, conn))
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
                using (var cmd = new SqlCommand("DELETE FROM facturaitem WHERE idFactura = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    cmd.ExecuteNonQuery();
                }

                // Luego eliminamos la factura
                using (var cmd = new SqlCommand("DELETE FROM facturas WHERE idFactura = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public int ActualizarPreciosPorProveedor(string proveedor, decimal porcentaje)
        {
            using (SqlConnection conn = ObtenerConexion())
            {
                conn.Open();
                // SQL: Sumamos el nuevo porcentaje al que ya existe en la columna RecargoPorcentaje
                string sql = @"UPDATE productos 
                       SET RecargoPorcentaje = RecargoPorcentaje + @porcentaje 
                       WHERE NombreProveedor = @proveedor";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Pasamos el porcentaje (ejemplo: 10)
                    cmd.Parameters.AddWithValue("@porcentaje", porcentaje);
                    cmd.Parameters.AddWithValue("@proveedor", proveedor);
                    return cmd.ExecuteNonQuery();
                }
            }
        }


        public bool VerificarSiHayCajaAbierta()
        {
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Cajas WHERE FechaCierre IS NULL";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }



    }
}