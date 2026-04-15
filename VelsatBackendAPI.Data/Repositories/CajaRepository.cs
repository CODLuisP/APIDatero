using APIDatero.Model.Documentacion;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System.Transactions;
using VelsatBackendAPI.Model;
using VelsatBackendAPI.Model.Caja;
using VelsatBackendAPI.Model.GestionVilla;

namespace VelsatBackendAPI.Data.Repositories
{
    //MÉTODOS PARA CAJA DE VILLA
    public class CajaRepository : ICajaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CajaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        public async Task<List<Carro>> GetUnidades(string username)
        {
            var unidades = new List<Carro>();
            int contador = 1;

            string sqlDevices = "SELECT deviceID FROM device WHERE habilitada = '1' and accountID = @AccountID";
            string sqlDeviceUsers = "SELECT DeviceID FROM deviceuser where UserId = @AccountID";

            using var connection = CreateConnection();

            var dispositivos = await connection.QueryAsync<string>(sqlDevices, new { AccountID = username });
            var dispositivosUsuarios = await connection.QueryAsync<string>(sqlDeviceUsers, new { AccountID = username });

            // Combina ambas listas y elimina duplicados
            var codigosUnicos = dispositivos.Concat(dispositivosUsuarios).Distinct();

            foreach (var device in codigosUnicos)
            {
                unidades.Add(new Carro
                {
                    Id = contador++,
                    Codunidad = device
                });
            }

            return unidades;
        }

        public async Task<List<Usuario>> GetConductores(string codusuario)
        {
            string sql = @"SELECT codtaxi, nombres, apellidos FROM taxi WHERE codusuario = @Codusuario and estado = 'A' and habilitado = '1'";

            using var connection = CreateConnection();
            var conductores = await connection.QueryAsync<dynamic>(sql, new { Codusuario = codusuario });

            var listaConductores = new List<Usuario>();

            foreach (var row in conductores)
            {
                var conductor = new Usuario
                {
                    Codigo = row.codtaxi.ToString(),
                    Nombre = row.nombres,
                    Apepate = row.apellidos
                };

                listaConductores.Add(conductor);
            }

            return listaConductores;
        }

        //MÉTODOS PARA GESTIÓN VILLA
        public async Task<List<RutaUrbano>> ListaRutas(string usuario) //Falta un método original para subusuarios (por ahora no era necesario)
        {
            string sql = @"SELECT codigo, nombre, codorigen, coddestino FROM rutaurbano WHERE eliminado = '0' AND usuario = @Usuario";

            using var connection = CreateConnection();
            var rutas = await connection.QueryAsync<RutaUrbano>(sql, new { Usuario = usuario });

            return rutas.ToList();
        }

        public async Task<List<ConductorDisp>> ListaConducDisp(string usuario)
        {
            var especiales = new[] { "lmmaldonado", "jbohorquezc", "jguevarar", "plangev", "emaylleg" };

            var codigo = especiales.Contains(usuario) ? "realstar" : usuario;

            string sql = @"SELECT nombres, apellidos, codtaxi FROM taxi WHERE estado = 'A' and codusuario = @Usuario";

            using var connection = CreateConnection();
            var resultado = await connection.QueryAsync<ConductorDisp>(sql, new { Usuario = codigo });

            return resultado.ToList();
        }

        public async Task<List<Carro>> ListUnidDisp(string usuario)
        {
            var especiales = new[] { "lmmaldonado", "jbohorquezc", "jguevarar", "plangev", "emaylleg" };
            var codigo = especiales.Contains(usuario) ? "realstar" : usuario;

            string sql = @"SELECT deviceID, codconductoract, rutadefault FROM device WHERE rutaact = '0' AND accountID = @Usuario";

            using var connection = CreateConnection();
            var unidadesRaw = await connection.QueryAsync(sql, new { Usuario = codigo });

            var unidades = new List<Carro>();

            foreach (var row in unidadesRaw)
            {
                var unidad = new Carro
                {
                    Codunidad = row.deviceID,
                    Conductor = !string.IsNullOrWhiteSpace((string?)row.codconductoract) ? await GetDetalleConductor(row.codconductoract) : null
                };

                unidades.Add(unidad);
            }

            return unidades;
        }

        public async Task<Usuario> GetDetalleConductor(string codtaxi)
        {
            if (!int.TryParse(codtaxi, out int codtaxiInt))
            {
                return null; // Retornamos null si la conversión falla
            }

            string sql = "SELECT codtaxi, nombres, apellidos, sexo, dni, telefono FROM taxi WHERE estado = 'A' and codtaxi = @Codtaxi";

            var parameters = new
            {
                Codtaxi = codtaxiInt
            };

            using var connection = CreateConnection();
            var results = await connection.QueryAsync(sql, parameters);

            if (!results.Any())
            {
                return null;
            }

            var row = results.First();

            var conductor = new Usuario
            {
                Codigo = row.codtaxi.ToString(),
                Nombre = row.nombres,
                Apepate = row.apellidos,
                Sexo = row.sexo,
                Dni = row.dni,
                Telefono = row.telefono
            };

            return conductor;
        }

        public async Task<List<DespachoVilla>> ListDespachoIniciado(string codruta)
        {
            var fecsys = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 28800;
            var fecha = fecsys;

            string sql = @"SELECT a.codigo, a.deviceID, a.fechaini, a.fechafin, a.fecprog, t.nombres, t.codtaxi, t.apellidos, d.ultimocontrol as nombrecontrol, a.eliminado, a.boletos FROM urbano_asigna a JOIN taxi t ON a.codconductor = t.codtaxi JOIN device d ON a.deviceID = d.deviceID WHERE a.eliminado = 0 AND a.codruta = @Codruta AND a.fechaini >= @Fechaini ORDER BY a.fechaini DESC LIMIT 300";

            using var connection = CreateConnection();
            var despachosRaw = await connection.QueryAsync(sql, new { Codruta = codruta, Fechaini = fecha });

            var listaDespachos = new List<DespachoVilla>();

            foreach (var row in despachosRaw)
            {
                if (row == null)
                    continue;
                var despacho = new DespachoVilla
                {
                    Codigo = row.codigo?.ToString(),
                    Boletos = row.boletos,
                    Fecini = row.fechaini?.ToString(),
                    Fecfin = row.fechafin?.ToString(),
                    Fecprog = row.fecprog?.ToString(),
                    Estado = row.eliminado,
                    Carro = new Carro
                    {
                        Codunidad = row.deviceID
                    },
                    Conductor = new Usuario
                    {
                        Codigo = row.codtaxi?.ToString(),
                        Nombre = row.apellidos
                    },
                    Ultimocontrol = row.nombrecontrol
                };

                if (!string.IsNullOrWhiteSpace(despacho.Ultimocontrol) && despacho.Ultimocontrol != "0")
                {
                    var detalleControl = await GetDetalleControl(despacho.Ultimocontrol);

                    if (detalleControl != null)
                    {
                        despacho.Ultimocontrol = detalleControl.Nomcorto;
                    }
                }

                listaDespachos.Add(despacho);
            }

            return listaDespachos;
        }

        private async Task<ControlVilla> GetDetalleControl(string codigo)
        {
            string sql = @"SELECT codigo, nomcorto FROM controlurbano WHERE codigo = @Codigo";

            using var connection = CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<ControlVilla>(sql, new { Codigo = codigo });

            return result;
        }

        public async Task<string> AsignarDespacho(DespachoVilla despacho)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var fecProgConSegundos = despacho.Fecprog + ":00";
                var fecDateTime = DateTime.ParseExact(fecProgConSegundos, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                var feciniUnix = new DateTimeOffset(fecDateTime).ToUnixTimeSeconds();
                var fecsysUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string fecini = feciniUnix.ToString();
                string fecreg = fecsysUnix.ToString();

                // Obtener detalle de la ruta
                var ruta = await GetDetalleRuta(despacho.Ruta?.Codigo, connection, transaction);
                if (ruta == null)
                {
                    return "Ruta no encontrada";
                }

                // Crear GPS
                var gps = new Gps
                {
                    Feciniruta = fecini,
                    Origen = ruta.Codorigen,
                    Destino = ruta.Coddestino,
                    Rutaact = ruta.Codigo,
                    Numequipo = despacho.Carro?.Codunidad,
                    Conductor = despacho.Conductor
                };

                // Actualizar registros anteriores del mismo deviceID para establecer isruta = 0
                string updateSql = @"UPDATE urbano_asigna SET isruta = '0' WHERE deviceID = @DeviceID";
                await connection.ExecuteAsync(updateSql, new { DeviceID = despacho.Carro?.Codunidad }, transaction);

                // Insertar en urbano_asigna
                string insertSql = @"INSERT INTO urbano_asigna (deviceID, codruta, fecprog, fechaini, codconductor, fecreg, boletos, isruta) 
     VALUES (@DeviceID, @Codruta, @Fecprog, @Fechaini, @Codconductor, @Fecreg, @Boletos, @Isruta)";
                var parametrosInsert = new
                {
                    DeviceID = despacho.Carro?.Codunidad,
                    Codruta = ruta.Codigo,
                    Fecprog = fecini,
                    Fechaini = fecini,
                    Codconductor = despacho.Conductor?.Codigo,
                    Fecreg = fecreg,
                    Boletos = "0",
                    Isruta = "1"
                };
                var filasAfectadas = await connection.ExecuteAsync(insertSql, parametrosInsert, transaction);
                if (filasAfectadas == 0)
                {
                    return "Error al guardar";
                }

                // Obtener el último despacho para esa ruta
                var despachoInsertado = await ObtenerUltimoDespacho(ruta.Codigo, connection, transaction);

                // Insertar en recor_control usando los datos del último despacho
                if (despachoInsertado != null)
                {
                    string insertControlSql = @"INSERT INTO recor_control (codasig, deviceID, codconductor, codruta, hora_registro, hora_inicio, fecha) 
                              VALUES (@Codasig, @DeviceID, @Codconductor, @Codruta, @HoraRegistro, @HoraInicio, @Fecha)";

                    var fechaActual = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");

                    // Convertir timestamps a formato HH:mm
                    var timeZonePeru = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                    var horaRegistro = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTimeOffset.FromUnixTimeSeconds(long.Parse(despachoInsertado.Fecreg)).DateTime,
                        timeZonePeru).ToString("HH:mm");
                    var horaInicio = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTimeOffset.FromUnixTimeSeconds(long.Parse(despachoInsertado.Fecini)).DateTime,
                        timeZonePeru).ToString("HH:mm");

                    var parametrosControl = new
                    {
                        Codasig = despachoInsertado.Codigo,
                        DeviceID = despachoInsertado.Carro?.Codunidad,
                        Codconductor = despachoInsertado.Conductor?.Codigo,
                        Codruta = despachoInsertado.Ruta?.Codigo,
                        HoraRegistro = horaRegistro,
                        HoraInicio = horaInicio,
                        Fecha = fechaActual
                    };

                    await connection.ExecuteAsync(insertControlSql, parametrosControl, transaction);
                }

                // Actualizar servicioactual del conductor
                await connection.ExecuteAsync("UPDATE taxi SET servicioactual = @Ruta WHERE codtaxi = @Codtaxi",
                    new { Ruta = ruta.Codigo, Codtaxi = despacho.Conductor?.Codigo }, transaction);

                // Actualizar datos del device
                await connection.ExecuteAsync(@"UPDATE device SET origen = @Origen, destino = @Destino, rutaact = @Rutaact, ultimocontrol='0', codconductoract = @Codconductor, feciniruta = @Feciniruta WHERE deviceID = @Numequipo", new
                {
                    gps.Origen,
                    gps.Destino,
                    gps.Rutaact,
                    Codconductor = gps.Conductor?.Codigo,
                    Feciniruta = gps.Feciniruta,
                    gps.Numequipo
                }, transaction);

                transaction.Commit();
                return "Datos Guardados";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return "Error al guardar";
            }
        }

        private async Task<DespachoVilla> ObtenerUltimoDespacho(string codruta, IDbConnection connection, IDbTransaction transaction)
        {
            string sql = @"SELECT codigo, deviceID, fecprog, fechaini, fecreg, codruta, codconductor FROM urbano_asigna WHERE codruta = @Codruta ORDER BY codigo DESC LIMIT 1";
            var row = await connection.QueryFirstOrDefaultAsync(sql, new { Codruta = codruta }, transaction);

            if (row == null) return null;

            return new DespachoVilla
            {
                Codigo = row.codigo.ToString(),
                Fecprog = row.fecprog,
                Fecini = row.fechaini.ToString(),
                Fecreg = row.fecreg.ToString(),
                Ruta = new RutaUrbano
                {
                    Codigo = codruta,
                },
                Carro = new Carro { Codunidad = row.deviceID },
                Conductor = new Usuario { Codigo = row.codconductor }
            };
        }

        private async Task<RutaUrbano> GetDetalleRuta(string codigo, IDbConnection connection, IDbTransaction transaction)
        {
            string sql = "SELECT codigo, codorigen, coddestino FROM rutaurbano WHERE codigo = @Codigo";
            return await connection.QueryFirstOrDefaultAsync<RutaUrbano>(sql, new { Codigo = codigo }, transaction);
        }

        public async Task<string> ObtenerUltimoCodasig(string placa)
        {
            const string sql = @"SELECT codigo FROM urbano_asigna 
                WHERE deviceID = @Placa 
                ORDER BY codigo DESC 
                LIMIT 1";

            using var connection = _connectionFactory.GetDefaultConnection();
            var codasig = await connection.QueryFirstOrDefaultAsync<int>(sql, new { Placa = placa });
            return codasig.ToString();
        }

        public async Task<string> EliminarDespacho(DespachoVilla despacho)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Timestamp actual en segundos (UTC - 8h)
                var fecelimUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 28800;
                var zonaLima = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); // Windows
                var fechaLima = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zonaLima);
                var fecelim = fechaLima.ToUnixTimeSeconds().ToString();

                // Obtener detalles del despacho
                var sqlDetalle = @"SELECT codigo, deviceID, codconductor FROM urbano_asigna WHERE codigo = @Codigo";
                var detalle = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlDetalle, new { Codigo = despacho.Codigo });
                if (detalle == null) return "Despacho no encontrado";

                var codUnidad = (string)detalle.deviceID;
                var codConductor = (string)detalle.codconductor;

                // Liberar conductor (servicioactual = null)
                var sqlLiberarConductor = @"UPDATE taxi SET servicioactual = NULL WHERE codtaxi = @Codtaxi";
                await connection.ExecuteAsync(sqlLiberarConductor, new { Codtaxi = codConductor }, transaction);

                // Marcar despacho como eliminado
                var sqlEliminarDespacho = @"UPDATE urbano_asigna SET eliminado = '1', motivoelim = @Motivo, fecelim = @Fecelim, isruta = '0' WHERE codigo = @Codigo";
                await connection.ExecuteAsync(sqlEliminarDespacho, new
                {
                    Motivo = despacho.Motivoelim,
                    Fecelim = fecelim,
                    Codigo = despacho.Codigo
                }, transaction);

                // Marcar control de ruta como eliminado
                var sqlEliminarControl = @"UPDATE recor_control SET eliminado = '1' WHERE codasig = @Codasig ORDER BY codigo LIMIT 1";
                await connection.ExecuteAsync(sqlEliminarControl, new { Codasig = despacho.Codigo }, transaction);

                // Limpiar ruta en tabla device
                var sqlLimpiarDevice = @"UPDATE device SET rutaact = '0', feciniruta = '0', origen = NULL, destino = NULL WHERE deviceID = @DeviceID";
                var filas = await connection.ExecuteAsync(sqlLimpiarDevice, new { DeviceID = codUnidad }, transaction);

                // Actualizar tabla recent_urbano
                var sqlActualizarRecentUrbano = @"UPDATE recent_urbano SET fechaini = NULL, isruta = '0' WHERE deviceID = @DeviceID";
                await connection.ExecuteAsync(sqlActualizarRecentUrbano, new { DeviceID = codUnidad }, transaction);

                transaction.Commit();
                return filas > 0 ? "Datos Guardados" : "Error al guardar";
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return "Error al guardar";
            }
        }
        //FIN

        //GESTIÓN
        public async Task<List<Conductor>> GetConductoresxUsuario(string codusuario)
        {
            string sql = @"SELECT codtaxi as Codigo, 
                          nombres as Nombres, 
                          apellidos as Apellidos, 
                          login as Login, 
                          clave as Clave, 
                          telefono as Telefono, 
                          dni as Dni, 
                          email as Email, 
                          brevete as Brevete, 
                          sctr as Sctr, 
                          direccion as Direccion, 
                          imagen as Imagen, 
                          catbrevete as CatBrevete, 
                          fecvalidbrevete as FecValidBrevete, 
                          estbrevete as EstBrevete, 
                          sexo as Sexo, 
                          habilitado as Habilitado 
                   FROM taxi 
                   WHERE codusuario = @Codusuario AND estado = 'A' 
                   ORDER BY habilitado DESC, apellidos";

            using var connection = CreateConnection();
            var conductores = await connection.QueryAsync<Conductor>(sql, new { Codusuario = codusuario });

            return conductores.ToList();
        }

        public async Task<List<Carro>> GetUnidadesxUsuario(string usuario)
        {
            var totalCarros = new List<Carro>();

            // Primera consulta - Tabla device
            var carrosDevice = await GetCarrosFromDeviceAsync(usuario);
            if (carrosDevice?.Any() == true)
            {
                totalCarros.AddRange(carrosDevice);
            }

            // Segunda consulta - Tabla DeviceUser
            var carrosDeviceUser = await GetCarrosFromDeviceUserAsync(usuario);
            if (carrosDeviceUser?.Any() == true)
            {
                totalCarros.AddRange(carrosDeviceUser);
            }

            // Ordenar por codunidad
            totalCarros = totalCarros.OrderBy(c => c.Codunidad).ToList();

            return totalCarros;
        }

        public async Task<List<Conductor>> GetCobradoresxUsuario(string codusuario)
        {
            string sql = @"SELECT codtaxi as Codigo, 
                          nombres as Nombres, 
                          apellidos as Apellidos, 
                          login as Login, 
                          clave as Clave, 
                          telefono as Telefono, 
                          dni as Dni, 
                          email as Email, 
                          brevete as Brevete, 
                          sctr as Sctr, 
                          direccion as Direccion, 
                          imagen as Imagen, 
                          catbrevete as CatBrevete, 
                          fecvalidbrevete as FecValidBrevete, 
                          estbrevete as EstBrevete, 
                          sexo as Sexo, 
                          habilitado as Habilitado 
                   FROM cobrador 
                   WHERE codusuario = @Codusuario AND estado = 'A' 
                   ORDER BY habilitado DESC, apellidos";

            using var connection = CreateConnection();
            var conductores = await connection.QueryAsync<Conductor>(sql, new { Codusuario = codusuario });

            return conductores.ToList();
        }

        private async Task<List<Carro>> GetCarrosFromDeviceAsync(string accountId)
        {
            string sql = @"SELECT deviceID, habilitada, rutadefault FROM device WHERE accountID = @AccountId ORDER BY deviceID";

            using var connection = CreateConnection();
            var resultado = await connection.QueryAsync(sql, new { AccountId = accountId });

            var carros = new List<Carro>();

            foreach (var row in resultado)
            {
                var carro = new Carro
                {
                    Codunidad = row.deviceID?.ToString()?.ToUpper(),
                    Tipo = "2",
                    Habilitado = row.habilitada?.ToString(),
                    RutaDefault = row.rutadefault?.ToString()
                };

                carros.Add(carro);
            }

            return carros;
        }

        private async Task<List<Carro>> GetCarrosFromDeviceUserAsync(string userId)
        {
            string sql = @"SELECT DeviceName, DeviceID FROM DeviceUser WHERE UserID = @UserId AND Status = '1' ORDER BY DeviceName";

            using var connection = CreateConnection();
            var resultado = await connection.QueryAsync(sql, new { UserId = userId });

            var carros = new List<Carro>();

            foreach (var row in resultado)
            {
                var carro = new Carro
                {
                    Codunidad = row.DeviceName?.ToString()?.ToUpper(),
                    Tipo = "2",
                };

                carros.Add(carro);
            }

            return carros;
        }

        public async Task<List<Carro>> ReporteVueltas(string fecha, string ruta)
        {
            string fechaini = fecha + " 00:00:00";
            string fechafin = fecha + " 23:59:59";

            // Conversión a timestamps Unix
            DateTime fec1 = DateTime.ParseExact(fechaini, "dd/MM/yyyy HH:mm:ss", null);
            DateTime fec2 = DateTime.ParseExact(fechafin, "dd/MM/yyyy HH:mm:ss", null);

            long nini = ((DateTimeOffset)fec1).ToUnixTimeSeconds();
            long nfin = ((DateTimeOffset)fec2).ToUnixTimeSeconds();

            // Obtener lista de despachos
            var lista = await ListaVueltasAsync(nini.ToString(), nfin.ToString(), ruta);

            var listaOrdenada = new List<Carro>();

            if (lista.Any())
            {
                foreach (var despacho in lista)
                {
                    // Buscar si ya existe el vehículo en la lista ordenada
                    var carroExistente = listaOrdenada.FirstOrDefault(c => c.Codunidad == despacho.Carro?.Codunidad);

                    if (carroExistente != null)
                    {
                        // Si existe, añadir el despacho a su lista
                        carroExistente.ListaDespachos?.Add(despacho);
                    }
                    else
                    {
                        // Si no existe, crear nuevo carro
                        var nuevoCarro = new Carro
                        {
                            Codunidad = despacho.Carro?.Codunidad,
                            Conductor = despacho.Conductor,
                            ListaDespachos = new List<Despacho> { despacho }
                        };

                        listaOrdenada.Add(nuevoCarro);
                    }
                }
            }

            return listaOrdenada;
        }

        private async Task<List<Despacho>> ListaVueltasAsync(string fechaini, string fechafin, string ruta)
        {
            string sql = @"SELECT a.deviceid, r.nombre as ruta, t.apellidos, a.eliminado, FROM_UNIXTIME(a.fechaini,'%H:%i') as fechainicio, FROM_UNIXTIME(a.fechafin,'%H:%i') as fechafin FROM gestion_villa.urbano_asigna a, taxi t, rutaurbano r WHERE a.codruta = r.codigo AND a.codconductor = t.codtaxi AND a.codruta = @Ruta AND a.fechaini >= @Fechaini AND a.fechaini <= @Fechafin ORDER BY a.fechaini";

            using var connection = CreateConnection();
            var resultado = await connection.QueryAsync(sql, new { Ruta = ruta, Fechaini = fechaini, Fechafin = fechafin });

            var despachos = new List<Despacho>();

            foreach (var row in resultado)
            {
                var despacho = new Despacho
                {
                    Fecini = row.fechainicio?.ToString(),
                    Fecfin = row.fechafin?.ToString(),
                    Carro = new Carro
                    {
                        Codunidad = row.deviceid?.ToString()
                    },
                    Ruta = new Ruta
                    {
                        Nombre = row.ruta?.ToString()
                    },
                    Conductor = new Usuario
                    {
                        Nombre = row.apellidos?.ToString()
                    }
                };

                despachos.Add(despacho);
            }

            return despachos;
        }

        public async Task<int> NuevoConductorAsync(Conductor conductor, string usuario)
        {
            const string sqlConductor = @"INSERT INTO taxi (nombres, apellidos, login, clave, estado, codusuario, telefono, email, brevete, dni, direccion, sctr, catbrevete, estbrevete, fecvalidbrevete) VALUES (@Nombres, @Apellidos, @Login, @Clave, 'A', @Codusuario, @Telefono, @Email, @Brevete, @Dni, @Direccion, @Sctr, @Catbrevete, @Estbrevete, @Fecvalidbrevete); SELECT LAST_INSERT_ID();";

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insertar en tabla cobrador y obtener el ID generado
                var conductorID = await connection.ExecuteScalarAsync<int>(sqlConductor, new
                {
                    Nombres = conductor.Nombres,
                    Apellidos = conductor.Apellidos,
                    Login = conductor.Login,
                    Clave = conductor.Clave,
                    Codusuario = usuario,
                    Telefono = conductor.Telefono,
                    Email = conductor.Email,
                    Brevete = conductor.Brevete,
                    Dni = conductor.Dni,
                    Direccion = conductor.Direccion,
                    Sctr = conductor.Sctr,
                    Catbrevete = conductor.CatBrevete,
                    Estbrevete = conductor.EstBrevete,
                    Fecvalidbrevete = conductor.FecValidBrevete
                }, transaction);

                transaction.Commit();
                return conductorID;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> GuardarCobradorAsync(Conductor cobrador, string usuario)
        {
            const string sqlCobrador = @"INSERT INTO cobrador (nombres, apellidos, login, clave, estado, codusuario, telefono, email, brevete, dni, direccion, sctr, catbrevete, estbrevete, fecvalidbrevete) VALUES (@Nombres, @Apellidos, @Login, @Clave, 'A', @Codusuario, @Telefono, @Email, @Brevete, @Dni, @Direccion, @Sctr, @Catbrevete, @Estbrevete, @Fecvalidbrevete); SELECT LAST_INSERT_ID();";

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insertar en tabla cobrador y obtener el ID generado
                var cobradorId = await connection.ExecuteScalarAsync<int>(sqlCobrador, new
                {
                    Nombres = cobrador.Nombres,
                    Apellidos = cobrador.Apellidos,
                    Login = cobrador.Login,
                    Clave = cobrador.Clave,
                    Codusuario = usuario,
                    Telefono = cobrador.Telefono,
                    Email = cobrador.Email,
                    Brevete = cobrador.Brevete,
                    Dni = cobrador.Dni,
                    Direccion = cobrador.Direccion,
                    Sctr = cobrador.Sctr,
                    Catbrevete = cobrador.CatBrevete,
                    Estbrevete = cobrador.EstBrevete,
                    Fecvalidbrevete = cobrador.FecValidBrevete
                }, transaction);

                transaction.Commit();
                return cobradorId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> ModificarConductorAsync(Conductor conductor)
        {
            const string sql = @"
        UPDATE taxi 
        SET nombres = @Nombres, 
            apellidos = @Apellidos, 
            telefono = @Telefono, 
            email = @Email, 
            brevete = @Brevete, 
            dni = @Dni, 
            direccion = @Direccion, 
            sctr = @Sctr, 
            catbrevete = @CatBrevete, 
            estbrevete = @EstBrevete, 
            fecvalidbrevete = @FecValidBrevete 
        WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();

            return await connection.ExecuteAsync(sql, new
            {
                Nombres = conductor.Nombres,
                Apellidos = conductor.Apellidos,
                Telefono = conductor.Telefono,
                Email = conductor.Email,
                Brevete = conductor.Brevete,
                Dni = conductor.Dni,
                Direccion = conductor.Direccion,
                Sctr = conductor.Sctr,
                CatBrevete = conductor.CatBrevete,
                EstBrevete = conductor.EstBrevete,
                FecValidBrevete = conductor.FecValidBrevete,
                Codigo = conductor.Codigo
            });
        }

        public async Task<int> ModificarCobradorAsync(Conductor cobrador)
        {
            const string sql = @"
        UPDATE cobrador 
        SET nombres = @Nombres, 
            apellidos = @Apellidos, 
            telefono = @Telefono, 
            email = @Email, 
            brevete = @Brevete, 
            dni = @Dni, 
            direccion = @Direccion, 
            sctr = @Sctr, 
            catbrevete = @CatBrevete, 
            estbrevete = @EstBrevete, 
            fecvalidbrevete = @FecValidBrevete 
        WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();

            return await connection.ExecuteAsync(sql, new
            {
                Nombres = cobrador.Nombres,
                Apellidos = cobrador.Apellidos,
                Telefono = cobrador.Telefono,
                Email = cobrador.Email,
                Brevete = cobrador.Brevete,
                Dni = cobrador.Dni,
                Direccion = cobrador.Direccion,
                Sctr = cobrador.Sctr,
                CatBrevete = cobrador.CatBrevete,
                EstBrevete = cobrador.EstBrevete,
                FecValidBrevete = cobrador.FecValidBrevete,
                Codigo = cobrador.Codigo
            });
        }

        public async Task<int> HabilitarConductorAsync(int codigoConductor)
        {
            const string sql = "UPDATE taxi SET habilitado = '1' WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoConductor });
        }

        public async Task<int> HabilitarCobradorAsync(int codigoCobrador)
        {
            const string sql = "UPDATE cobrador SET habilitado = '1' WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoCobrador });
        }

        public async Task<int> DeshabilitarConductorAsync(int codigoConductor)
        {
            const string sql = "UPDATE taxi SET habilitado = '0' WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoConductor });
        }

        public async Task<int> DeshabilitarCobradorAsync(int codigoCobrador)
        {
            const string sql = "UPDATE cobrador SET habilitado = '0' WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoCobrador });
        }

        public async Task<int> LiberarConductorAsync(int codigoConductor)
        {
            const string sql = "UPDATE taxi SET servicioactual = NULL WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoConductor });
        }

        public async Task<int> EliminarConductorAsync(int codigoConductor)
        {
            const string sql = "UPDATE taxi SET estado = 'E' WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoConductor });
        }

        public async Task<int> EliminarCobradorAsync(int codigoCobrador)
        {
            const string sql = "UPDATE cobrador SET estado = 'E' WHERE codtaxi = @Codigo";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Codigo = codigoCobrador });
        }

        public async Task<int> HabilitarUnidadAsync(string placa)
        {
            const string sql = "UPDATE device SET habilitada = '1' WHERE deviceid = @Placa";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Placa = placa });
        }

        public async Task<int> DeshabilitarUnidadAsync(string placa)
        {
            const string sql = "UPDATE device SET habilitada = '0' WHERE deviceid = @Placa";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Placa = placa });
        }

        public async Task<int> LiberarUnidadAsync(string placa)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Actualizar tabla device
                const string sqlDevice = "UPDATE device SET rutaact = 0, feciniruta = 0, origen = null, destino = null WHERE deviceID = @DeviceID";
                var filasDevice = await connection.ExecuteAsync(sqlDevice, new { DeviceID = placa }, transaction);

                // Actualizar tabla recent_urbano
                const string sqlRecentUrbano = "UPDATE recent_urbano SET fechaini = NULL, isruta = '0' WHERE deviceID = @DeviceID";
                await connection.ExecuteAsync(sqlRecentUrbano, new { DeviceID = placa }, transaction);

                transaction.Commit();
                return filasDevice;
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }

        public async Task<int> LiberarTotalUnidadesAsync(string username)
        {
            const string sql = "UPDATE device set rutaact = 0, feciniruta = 0, origen = null, destino = null WHERE accountID = @AccountID'";
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, new { AccountID = username});
        }

        //APIS PARA CONTROL GPS
        public async Task<string> EnviarHoraSolicitud(string codruta)
        {
            const string sql = @"INSERT INTO horasolicitud (fecha, codruta) VALUES (@Fecha, @Codruta)";

            using var connection = CreateConnection();
            var parametros = new
            {
                Fecha = DateTime.UtcNow.AddHours(-5).ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                Codruta = codruta
            };

            var rows = await connection.ExecuteAsync(sql, parametros);
            return rows > 0 ? "Hora de solicitud registrada correctamente" : "No se pudo registrar la hora de solicitud";
        }

        public async Task<string> GetHoraSolicitud(string codruta)
        {
            // Obtener fecha actual en hora peruana
            var fechaActual = DateTime.UtcNow.AddHours(-5).Date;
            var fechaActualStr = fechaActual.ToString("yyyy-MM-dd");

            const string sql = @"SELECT fecha FROM horasolicitud WHERE DATE(fecha) = @FechaActual AND codruta = @Codruta ORDER BY id DESC LIMIT 1";

            using var connection = CreateConnection();
            var ultimoregistro = await connection.QueryFirstOrDefaultAsync<string>(sql, new { FechaActual = fechaActualStr, Codruta = codruta });

            return ultimoregistro ?? "No hay registros";
        }

        //DOCUMENTACIÓN
        //----------------------------------UNIDAD--------------------------------------------------//
        public async Task<List<Docunidad>> GetByDeviceID(string deviceID)
        {
            const string sql = @"SELECT * FROM docunidad WHERE DeviceID = @DeviceID ORDER BY Fecha_vencimiento DESC";

            using var connection = CreateConnection();
            var documentos = await connection.QueryAsync<Docunidad>(sql, new { DeviceID = deviceID });
            return documentos.ToList();
        }

        public async Task<int> Create(Docunidad docunidad)
        {
            const string sql = @"INSERT INTO docunidad (DeviceID, Tipo_documento, Archivo_url, Fecha_vencimiento, Observaciones, Usuario) VALUES 
        (@DeviceID, @Tipo_documento, @Archivo_url, @Fecha_vencimiento, @Observaciones, @Usuario); SELECT LAST_INSERT_ID();";

            using var connection = CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                docunidad.DeviceID,
                docunidad.Tipo_documento,
                docunidad.Archivo_url,
                docunidad.Fecha_vencimiento,
                docunidad.Observaciones,
                docunidad.Usuario
            });

            return id;
        }

        public async Task<bool> DeleteUnidad(int id)
        {
            const string sql = @"DELETE FROM docunidad WHERE Id = @Id";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });

            return affectedRows > 0;
        }

        public async Task<List<Docunidad>> GetDocumentosUnidadProximosVencer(string usuario)
        {
            // Obtener fecha actual en hora peruana
            var fechaActual = DateTime.UtcNow.AddHours(-5).Date;
            var fechaLimite = fechaActual.AddDays(30);

            const string sql = @"SELECT * FROM docunidad WHERE Fecha_vencimiento IS NOT NULL AND Fecha_vencimiento <= @FechaLimite AND usuario = @Usuario ORDER BY Fecha_vencimiento ASC";

            using var connection = CreateConnection();
            var documentos = await connection.QueryAsync<Docunidad>(sql, new
            {
                FechaLimite = fechaLimite,
                Usuario = usuario
            });

            return documentos.ToList();
        }

        //----------------------------------CONDUCTOR--------------------------------------------------//
        public async Task<List<Docconductor>> GetByCodtaxi(int codtaxi)
        {
            const string sql = @"SELECT * FROM docconductor WHERE Codtaxi = @Codtaxi ORDER BY Fecha_vencimiento DESC";

            using var connection = CreateConnection();
            var documentos = await connection.QueryAsync<Docconductor>(sql, new { Codtaxi = codtaxi });
            return documentos.ToList();
        }

        public async Task<int> Create(Docconductor docconductor)
        {
            const string sql = @"INSERT INTO docconductor (Codtaxi, Tipo_documento, Archivo_url, Fecha_vencimiento, Observaciones, Usuario) VALUES (@Codtaxi, @Tipo_documento, @Archivo_url, @Fecha_vencimiento, @Observaciones, @Usuario); SELECT LAST_INSERT_ID();";

            using var connection = CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                docconductor.Codtaxi,
                docconductor.Tipo_documento,
                docconductor.Archivo_url,
                docconductor.Fecha_vencimiento,
                docconductor.Observaciones,
                docconductor.Usuario
            });

            return id;
        }

        public async Task<bool> DeleteConductor(int id)
        {
            const string sql = @"DELETE FROM docconductor WHERE Id = @Id";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });

            return affectedRows > 0;
        }

        public async Task<List<Docconductor>> GetDocumentosConductorProximosVencer(string usuario)
        {
            // Obtener fecha actual en hora peruana
            var fechaActual = DateTime.UtcNow.AddHours(-5).Date;
            var fechaLimite = fechaActual.AddDays(30);

            const string sql = @"SELECT * FROM docconductor WHERE Fecha_vencimiento IS NOT NULL AND Fecha_vencimiento <= @FechaLimite AND usuario = @Usuario ORDER BY Fecha_vencimiento ASC";

            using var connection = CreateConnection();
            var documentos = await connection.QueryAsync<Docconductor>(sql, new
            {
                FechaLimite = fechaLimite,
                Usuario = usuario
            });

            return documentos.ToList();
        }

        //----------------------------------COBRADOR--------------------------------------------------//
        public async Task<List<Docconductor>> GetByCobrador(int codtaxi)
        {
            const string sql = @"SELECT * FROM doccobrador WHERE Codtaxi = @Codtaxi ORDER BY Fecha_vencimiento DESC";

            using var connection = CreateConnection();
            var documentos = await connection.QueryAsync<Docconductor>(sql, new { Codtaxi = codtaxi });
            return documentos.ToList();
        }

        public async Task<int> CreateCobrador(Docconductor doccobrador)
        {
            const string sql = @"INSERT INTO doccobrador (Codtaxi, Tipo_documento, Archivo_url, Fecha_vencimiento, Observaciones, Usuario) VALUES (@Codtaxi, @Tipo_documento, @Archivo_url, @Fecha_vencimiento, @Observaciones, @Usuario); SELECT LAST_INSERT_ID();";

            using var connection = CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                doccobrador.Codtaxi,
                doccobrador.Tipo_documento,
                doccobrador.Archivo_url,
                doccobrador.Fecha_vencimiento,
                doccobrador.Observaciones,
                doccobrador.Usuario
            });

            return id;
        }

        public async Task<bool> DeleteCobrador(int id)
        {
            const string sql = @"DELETE FROM doccobrador WHERE Id = @Id";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });

            return affectedRows > 0;
        }

        public async Task<List<Docconductor>> GetDocumentosCobradorProximosVencer(string usuario)
        {
            // Obtener fecha actual en hora peruana
            var fechaActual = DateTime.UtcNow.AddHours(-5).Date;
            var fechaLimite = fechaActual.AddDays(30);

            const string sql = @"SELECT * FROM doccobrador WHERE Fecha_vencimiento IS NOT NULL AND Fecha_vencimiento <= @FechaLimite AND usuario = @Usuario ORDER BY Fecha_vencimiento ASC";

            using var connection = CreateConnection();
            var documentos = await connection.QueryAsync<Docconductor>(sql, new
            {
                FechaLimite = fechaLimite,
                Usuario = usuario
            });

            return documentos.ToList();
        }

        public async Task<Usuario> GetDetalleCobrador(string codtaxi)
        {
            if (!int.TryParse(codtaxi, out int codtaxiInt))
            {
                return null; // Retornamos null si la conversión falla
            }

            string sql = "SELECT codtaxi, nombres, apellidos, sexo, dni, telefono FROM cobrador WHERE estado = 'A' and codtaxi = @Codtaxi";

            var parameters = new
            {
                Codtaxi = codtaxiInt
            };

            using var connection = CreateConnection();
            var results = await connection.QueryAsync(sql, parameters);

            if (!results.Any())
            {
                return null;
            }

            var row = results.First();

            var conductor = new Usuario
            {
                Codigo = row.codtaxi.ToString(),
                Nombre = row.nombres,
                Apepate = row.apellidos,
                Sexo = row.sexo,
                Dni = row.dni,
                Telefono = row.telefono
            };

            return conductor;
        }
    }
}