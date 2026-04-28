using APIDatero.Model.Datero;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.Datero;

namespace VelsatBackendAPI.Data.Repositories
{
    public class DateroRepository : IDateroRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DateroRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        //INICIO DATERO
        public async Task<UrbanoAsigna> GetUrbanoAsigna(string placa)
        {
            const string consulta = @"SELECT codigo, deviceID, fecreg, fechaini, fechafin, codconductor, codruta, isruta FROM urbano_asigna WHERE deviceID = @Placa AND eliminado != 1 ORDER BY codigo DESC LIMIT 1";

            const string getid = @"SELECT androidID from androidsid where placa = @Placa and eliminado = 0";

            using var connection = CreateConnection();

            var raw = await connection.QueryFirstOrDefaultAsync<UrbanoAsignaRaw>(consulta, new { Placa = placa });

            if (raw == null) return null;

            var androidID = await connection.QueryFirstOrDefaultAsync<string>(getid, new { Placa = placa });

            if (raw.Isruta == "0")
            {
                return new UrbanoAsigna
                {
                    Codigo = 0,
                    DeviceID = null,
                    Fecreg = null,
                    Fechaini = null,
                    Fechafin = null,
                    Codconductor = null,
                    Codruta = null,
                    Isruta = "0",
                    AndroidID = null
                };
            }

            return new UrbanoAsigna
            {
                Codigo = raw.Codigo,
                DeviceID = raw.DeviceID,
                Fecreg = UnixToHHmm(raw.Fecreg),
                Fechaini = UnixToHHmm(raw.Fechaini),
                Fechafin = raw.Fechafin.HasValue ? UnixToHHmm(raw.Fechafin.Value) : null,
                Codconductor = raw.Codconductor,
                Codruta = raw.Codruta,
                Isruta = raw.Isruta,
                AndroidID = androidID
            };
        }

        private string UnixToHHmm(int unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().ToString("HH:mm");
        }

        public async Task<IEnumerable<LogUrbano>> GetLogUrbano(string codasig)
        {
            const string query = @"Select * from recor_control where codasig = @CodAsig group by nom_control ORDER BY codigo";

            using var connection = CreateConnection();
            return await connection.QueryAsync<LogUrbano>(query, new { CodAsig = codasig });
        }

        public async Task<string> EnvioControl(LogUrbano log)
        {
            const string sql = @"INSERT INTO recor_control (codasig, deviceID, codconductor, codruta, nom_control, hora_registro, hora_inicio, hora_estimada, hora_llegada, volado, fecha) VALUES (@Codasig, @DeviceID, @Codconductor, @Codruta, @NomControl, @HoraRegistro, @HoraInicio, @HoraEstimada, @HoraLlegada, @Volado, @Fecha)";

            using var connection = CreateConnection();

            var parametros = new
            {
                Codasig = log.Codasig,
                DeviceID = log.Deviceid,
                Codconductor = log.Codconductor,
                Codruta = log.Codruta,
                NomControl = log.Nom_control,
                HoraRegistro = log.Hora_registro,
                HoraInicio = log.Hora_inicio,
                HoraEstimada = log.Hora_estimada,
                HoraLlegada = log.Hora_llegada,
                Volado = log.Volado,
                Fecha = log.Fecha
            };

            var rows = await connection.ExecuteAsync(sql, parametros);

            return rows > 0 ? "Control registrado correctamente" : "No se pudo registrar el control";
        }

        public async Task<IEnumerable<UnidadesDatero>> GetDevices(string accountID)
        {
            const string consulta = @"Select deviceID from device where accountID = @AccountID";

            using var connection = CreateConnection();
            return await connection.QueryAsync<UnidadesDatero>(consulta, new { AccountID = accountID });
        }

        public async Task<IEnumerable<DeviceOrden>> GetDevicesOrden(string rutaact)
        {
            const string consulta = @"Select deviceID, lastValidLatitude, lastValidLongitude, lastValidSpeed, direccion from device where accountID = 'transporvilla' and rutaact = @Rutaact";

            using var connection = CreateConnection();
            return await connection.QueryAsync<DeviceOrden>(consulta, new { Rutaact = rutaact });
        }

        public async Task<string> EndRuta(string deviceID)
        {
            const string sql1 = @"UPDATE device SET rutaact = '0', feciniruta = '0', origen = NULL, destino = NULL WHERE deviceID = @DeviceID";
            const string sql2 = @"UPDATE urbano_asigna SET isruta = '0', fechafin = @Fechaact WHERE deviceID = @DeviceID ORDER BY codigo DESC LIMIT 1";
            const string sql3 = @"UPDATE recent_urbano SET fechaini = NULL, isruta = '0' WHERE deviceID = @DeviceID";

            var fechaActual = DateTimeOffset.Now.ToUnixTimeSeconds();

            using var connection = CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                var rows1 = await connection.ExecuteAsync(sql1, new { DeviceID = deviceID }, transaction);
                var rows2 = await connection.ExecuteAsync(sql2, new { DeviceID = deviceID, Fechaact = fechaActual }, transaction);
                var rows3 = await connection.ExecuteAsync(sql3, new { DeviceID = deviceID }, transaction);

                transaction.Commit();
                return (rows1 > 0 || rows2 > 0)
                    ? "Ruta finalizada correctamente."
                    : "No se encontró el dispositivo o la asignación.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return $"Error al finalizar la ruta: {ex.Message}";
            }
        }

        public async Task<string> AsignarID(string placa, string nuevoAndroidID)
        {
            const string selectIds = @"SELECT androidID FROM androidsid";

            const string selectSql = @"SELECT id, placa, androidID, usuario, eliminado FROM androidsid WHERE placa = @Placa and eliminado = '0'";

            const string updateSql = @"UPDATE androidsid SET androidID = @AndroidID WHERE id = @Id AND androidID IS NULL";

            using var connection = CreateConnection();

            var idsExistentes = await connection.QueryAsync<string>(selectIds);
            if (idsExistentes.Any(id => id == nuevoAndroidID))
            {
                return "ID ya asignado a otra placa.";
            }

            var registro = await connection.QueryFirstOrDefaultAsync<Imei>(selectSql, new { Placa = placa });
            if (registro == null)
            {
                return "La placa no existe en la tabla.";
            }

            if (!string.IsNullOrWhiteSpace(registro.AndroidID))
            {
                return "Esta placa ya tiene un ID asignado.";
            }

            var filasAfectadas = await connection.ExecuteAsync(updateSql, new
            {
                Id = registro.Id,
                AndroidID = nuevoAndroidID
            });

            return filasAfectadas > 0 ? "ID asignado correctamente." : "No se pudo asignar el ID.";
        }

        public async Task<IEnumerable<DespachoAgrupado>> ControlDespachoAgrupado(string fecha, string codruta, string username)
        {
            const string consulta = @"SELECT r.*, t.apellidos AS NombreConductor FROM recor_control r LEFT JOIN taxi t ON r.codconductor = t.codtaxi AND t.estado = 'A' WHERE r.fecha LIKE @Fecha AND r.codruta = @Codruta AND r.eliminado = '0' ORDER BY r.codasig, r.codigo";

            string tablaGPS = username == "etudvrb" ? "control_gpse" : "control_gps";

            string consultaGPS = $@"SELECT codasig, deviceID, nom_control, hora_estimada, hora_llegada, volado, fecha, iSGPS FROM {tablaGPS} WHERE fecha LIKE @Fecha ORDER BY codasig";

            using var connection = CreateConnection();

            var registros = await connection.QueryAsync<Controll>(consulta, new { Fecha = fecha + "%", Codruta = codruta });
            var registrosGPS = await connection.QueryAsync<Controll>(consultaGPS, new { Fecha = fecha + "%", Codruta = codruta });

            var agrupado = registros
                .GroupBy(r => new
                {
                    r.Codasig,
                    r.Deviceid,
                    r.Codconductor,
                    r.Codruta,
                    r.NombreConductor,
                    r.Hora_registro,
                    r.Hora_inicio
                })
                .Select(g => new DespachoAgrupado
                {
                    Codasig = g.Key.Codasig,
                    Deviceid = g.Key.Deviceid,
                    Codconductor = g.Key.Codconductor,
                    Codruta = g.Key.Codruta,
                    NombreConductor = g.Key.NombreConductor,
                    Hora_registro = g.Key.Hora_registro,
                    Hora_inicio = g.Key.Hora_inicio,
                    Controles = g.Select(c => new PuntoControl
                    {
                        Nom_control = c.Nom_control,
                        Hora_estimada = c.Hora_estimada,
                        Hora_llegada = c.Hora_llegada,
                        Volado = c.Volado,
                        Fecha = c.Fecha,
                        IsGPS = c.IsGPS
                    }).ToList()
                });

            // Agrupar datos de control_gps
            var agrupadoGPS = registrosGPS
                .GroupBy(r => new
                {
                    r.Codasig,
                    r.Deviceid,
                    r.Codconductor,
                    r.Codruta,
                    r.NombreConductor,
                    r.Hora_registro,
                    r.Hora_inicio
                })
                .Select(g => new DespachoAgrupado
                {
                    Codasig = g.Key.Codasig,
                    Deviceid = g.Key.Deviceid,
                    Codconductor = g.Key.Codconductor,
                    Codruta = g.Key.Codruta,
                    NombreConductor = g.Key.NombreConductor,
                    Hora_registro = g.Key.Hora_registro,
                    Hora_inicio = g.Key.Hora_inicio,
                    Controles = g.Select(c => new PuntoControl
                    {
                        Nom_control = c.Nom_control,
                        Hora_estimada = c.Hora_estimada,
                        Hora_llegada = c.Hora_llegada,
                        Volado = c.Volado,
                        Fecha = c.Fecha,
                        IsGPS = c.IsGPS // Marcar como GPS
                    }).ToList()
                });

            var todasLasListas = agrupado.Concat(agrupadoGPS); 
            // Unir por codasig y deviceID
            var resultadoFinal = todasLasListas
                .GroupBy(d => new { d.Codasig, d.Deviceid }) 
                .Select(g =>
                { 
                    // Tomar el primer despacho como base (puede ser de cualquiera de las dos listas)
                    var despachoBase = g.First(); 
                    // Combinar todos los controles de todos los despachos del grupo
                    var todosLosControles = g.SelectMany(d => d.Controles); 
                    // Agrupar controles por nombre y tomar el que tiene IsGPS = '0' si existe
                    var controlesFiltrados = todosLosControles .GroupBy(c => c.Nom_control) .Select(gc => gc.OrderBy(c => c.IsGPS).First())// 0 viene antes que 1
                    .ToList();


            return new DespachoAgrupado
                    {
                        Codasig = despachoBase.Codasig,
                        Deviceid = despachoBase.Deviceid,
                        Codconductor = despachoBase.Codconductor,
                        Codruta = despachoBase.Codruta,
                        NombreConductor = despachoBase.NombreConductor,
                        Hora_registro = despachoBase.Hora_registro,
                        Hora_inicio = despachoBase.Hora_inicio,
                        Controles = controlesFiltrados
                    };
                }).Where(d => !string.IsNullOrEmpty(d.Codruta));

            return resultadoFinal;
        }

        public async Task<IEnumerable<DespachoAgrupado>> ControlDespachoAgrupadoEdu(string fecha, string codruta, string username)
        {
            const string consulta = @"SELECT r.*, t.apellidos AS NombreConductor FROM recor_control r LEFT JOIN taxi t ON r.codconductor = t.codtaxi AND t.estado = 'A' WHERE r.fecha LIKE @Fecha AND r.codruta = @Codruta AND r.eliminado = '0' ORDER BY r.codasig, r.codigo";

            string tablaGPS = (username == "etudvrb" || username == "etudvrg" || username == "etudv22" || username == "serfrymh")
                ? "control_gpse"
                : "control_gps";

            string consultaGPS = $@"SELECT codasig, deviceID, nom_control, hora_estimada, hora_llegada, volado, fecha, iSGPS FROM {tablaGPS} WHERE fecha LIKE @Fecha ORDER BY codasig";

            using var connection = CreateConnection();

            var registros = await connection.QueryAsync<Controll>(consulta, new { Fecha = fecha + "%", Codruta = codruta });
            var registrosGPS = await connection.QueryAsync<Controll>(consultaGPS, new { Fecha = fecha + "%", Codruta = codruta });

            var agrupado = registros
                .GroupBy(r => new
                {
                    r.Codasig,
                    Deviceid = r.Deviceid?.ToUpper(), // Normalizar a mayúsculas
                    r.Codconductor,
                    r.Codruta,
                    r.NombreConductor,
                    r.Hora_registro,
                    r.Hora_inicio
                })
                .Select(g => new DespachoAgrupado
                {
                    Codasig = g.Key.Codasig,
                    Deviceid = g.First().Deviceid, // Mantener el deviceid original
                    Codconductor = g.Key.Codconductor,
                    Codruta = g.Key.Codruta,
                    NombreConductor = g.Key.NombreConductor,
                    Hora_registro = g.Key.Hora_registro,
                    Hora_inicio = g.Key.Hora_inicio,
                    Controles = g.Select(c => new PuntoControl
                    {
                        Nom_control = c.Nom_control,
                        Hora_estimada = c.Hora_estimada,
                        Hora_llegada = c.Hora_llegada,
                        Volado = c.Volado,
                        Fecha = c.Fecha,
                        IsGPS = c.IsGPS
                    }).ToList()
                });

            // Agrupar datos de control_gps
            var agrupadoGPS = registrosGPS
                .GroupBy(r => new
                {
                    r.Codasig,
                    Deviceid = r.Deviceid?.ToUpper(), // Normalizar a mayúsculas
                    r.Codconductor,
                    r.Codruta,
                    r.NombreConductor,
                    r.Hora_registro,
                    r.Hora_inicio
                })
                .Select(g => new DespachoAgrupado
                {
                    Codasig = g.Key.Codasig,
                    Deviceid = g.First().Deviceid, // Mantener el deviceid original
                    Codconductor = g.Key.Codconductor,
                    Codruta = g.Key.Codruta,
                    NombreConductor = g.Key.NombreConductor,
                    Hora_registro = g.Key.Hora_registro,
                    Hora_inicio = g.Key.Hora_inicio,
                    Controles = g.Select(c => new PuntoControl
                    {
                        Nom_control = c.Nom_control,
                        Hora_estimada = c.Hora_estimada,
                        Hora_llegada = c.Hora_llegada,
                        Volado = c.Volado,
                        Fecha = c.Fecha,
                        IsGPS = c.IsGPS
                    }).ToList()
                });

            var todasLasListas = agrupado.Concat(agrupadoGPS);

            // Unir por codasig y deviceID (case-insensitive)
            var resultadoFinal = todasLasListas
                .GroupBy(d => new { d.Codasig, Deviceid = d.Deviceid?.ToUpper() }) // Normalizar aquí también
                .Select(g =>
                {
                    var despachoBase = g.First();
                    var todosLosControles = g.SelectMany(d => d.Controles);

                    // Filtrar controles con nombres válidos antes de agrupar
                    var controlesFiltrados = todosLosControles
                        .Where(c => !string.IsNullOrEmpty(c.Nom_control))
                        .GroupBy(c => c.Nom_control)
                        .Select(gc => gc
                            .OrderBy(c => Math.Abs(
                                (TimeSpan.Parse(c.Hora_llegada ?? "00:00") -
                                 TimeSpan.Parse(c.Hora_estimada ?? "00:00")).TotalMinutes))
                            .First())
                        .ToList();

                    return new DespachoAgrupado
                    {
                        Codasig = despachoBase.Codasig,
                        Deviceid = despachoBase.Deviceid, // Usar el deviceid original
                        Codconductor = despachoBase.Codconductor,
                        Codruta = despachoBase.Codruta,
                        NombreConductor = despachoBase.NombreConductor,
                        Hora_registro = despachoBase.Hora_registro,
                        Hora_inicio = despachoBase.Hora_inicio,
                        Controles = controlesFiltrados
                    };
                }).Where(d => !string.IsNullOrEmpty(d.Codruta));

            return resultadoFinal;
        }

        //public async Task<IEnumerable<DespachoAgrupado>> ControlDespachoGPSE(string fecha, string codruta)
        //{
        //    const string consultaDespachos = @"
        //SELECT 
        //    r.codasig,
        //    r.deviceid,
        //    r.codconductor,
        //    r.codruta,
        //    r.hora_registro,
        //    r.hora_inicio,
        //    t.apellidos AS NombreConductor
        //FROM recor_control r 
        //LEFT JOIN taxi t ON r.codconductor = t.codtaxi AND t.estado = 'A' 
        //WHERE r.fecha LIKE @Fecha 
        //AND r.codruta = @Codruta 
        //AND r.eliminado = '0'
        //GROUP BY r.codasig, r.deviceid, r.codconductor, r.codruta, r.hora_registro, r.hora_inicio, t.apellidos
        //ORDER BY r.codasig";

        //    const string consultaControlesGPSE = @"
        //SELECT 
        //    codasig, 
        //    deviceID, 
        //    nom_control,
        //    hora_estimada, 
        //    hora_llegada, 
        //    volado, 
        //    fecha, 
        //    isGPS 
        //FROM control_gpse 
        //WHERE fecha LIKE @Fecha 
        //ORDER BY codasig, deviceID, hora_estimada";

        //    using var connection = CreateConnection();

        //    var despachos = await connection.QueryAsync<Controll>(consultaDespachos, new { Fecha = fecha + "%", Codruta = codruta });
        //    var controlesGPSE = await connection.QueryAsync<Controll>(consultaControlesGPSE, new { Fecha = fecha + "%" });

        //    // Agrupar controles por codasig y deviceID (case-insensitive)
        //    var controlesAgrupados = controlesGPSE
        //        .GroupBy(c => new
        //        {
        //            Codasig = c.Codasig,
        //            Deviceid = c.Deviceid?.ToUpper() // Normalizar a mayúsculas
        //        })
        //        .ToDictionary(
        //            g => (g.Key.Codasig, g.Key.Deviceid),
        //            g => g.Select(c => new PuntoControl
        //            {
        //                Nom_control = c.Nom_control,
        //                Hora_estimada = c.Hora_estimada,
        //                Hora_llegada = c.Hora_llegada,
        //                Volado = c.Volado,
        //                Fecha = c.Fecha,
        //                IsGPS = c.IsGPS
        //            })
        //            .Where(c => !string.IsNullOrEmpty(c.Nom_control))
        //            .OrderBy(c => c.Hora_estimada)
        //            .ToList()
        //        );

        //    // Combinar despachos con sus controles de control_gpse
        //    var resultado = despachos.Select(d => new DespachoAgrupado
        //    {
        //        Codasig = d.Codasig,
        //        Deviceid = d.Deviceid,
        //        Codconductor = d.Codconductor,
        //        Codruta = d.Codruta,
        //        NombreConductor = d.NombreConductor,
        //        Hora_registro = d.Hora_registro,
        //        Hora_inicio = d.Hora_inicio,
        //        Controles = controlesAgrupados.TryGetValue((d.Codasig, d.Deviceid?.ToUpper()), out var controles)
        //            ? controles
        //            : new List<PuntoControl>()
        //    });

        //    return resultado;
        //}

        //APIS PARA REGISTROS DE GPS VEHICULAR

        public async Task<IEnumerable<DeviceDespachadas>> DevicesDespacho(string fecha, string ruta)
        {
            // Parsear fecha en formato "YYYY-MM-DD"
            var fechaBase = DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Crear timestamp de inicio del día (00:00:00)
            var fechaInicio = fechaBase;
            var fechaInicioPeruana = TimeZoneInfo.ConvertTimeToUtc(fechaInicio, peruTimeZone);
            var timestamp1 = ((DateTimeOffset)fechaInicioPeruana).ToUnixTimeSeconds();

            // Crear timestamp de fin del día (23:59:59)
            var fechaFin = fechaBase.AddHours(23).AddMinutes(59).AddSeconds(59);
            var fechaFinPeruana = TimeZoneInfo.ConvertTimeToUtc(fechaFin, peruTimeZone);
            var timestamp2 = ((DateTimeOffset)fechaFinPeruana).ToUnixTimeSeconds();

            const string consulta = @"SELECT codigo, deviceID, fechaini, fechafin FROM urbano_asigna WHERE fecreg BETWEEN @Fecha1 AND @Fecha2 and codruta = @Ruta and eliminado = '0' ORDER BY fechaini DESC";

            using var connection = CreateConnection();
            var resultados = await connection.QueryAsync(consulta, new { Fecha1 = timestamp1, Fecha2 = timestamp2, Ruta = ruta });

            return resultados.Select(row => new DeviceDespachadas
            {
                Codigo = row.codigo,
                DeviceID = row.deviceID,
                Fechaini = ConvertirTimestampAHora(row.fechaini),
                Fechafin = row.fechafin != null
                    ? ConvertirTimestampAHora(row.fechafin + 1559)
                    : ConvertirTimestampAHora(row.fechaini + 12659)
            });
        }

        public async Task<IEnumerable<DeviceDespachadas>> DevicesDespachoEdu(string fecha, string ruta)
        {
            // Parsear fecha en formato "YYYY-MM-DD"
            var fechaBase = DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Crear timestamp de inicio del día (00:00:00)
            var fechaInicio = fechaBase;
            var fechaInicioPeruana = TimeZoneInfo.ConvertTimeToUtc(fechaInicio, peruTimeZone);
            var timestamp1 = ((DateTimeOffset)fechaInicioPeruana).ToUnixTimeSeconds();

            // Crear timestamp de fin del día (23:59:59)
            var fechaFin = fechaBase.AddHours(23).AddMinutes(59).AddSeconds(59);
            var fechaFinPeruana = TimeZoneInfo.ConvertTimeToUtc(fechaFin, peruTimeZone);
            var timestamp2 = ((DateTimeOffset)fechaFinPeruana).ToUnixTimeSeconds();

            const string consulta = @"SELECT codigo, deviceID, fechaini, fechafin FROM urbano_asigna WHERE fecreg BETWEEN @Fecha1 AND @Fecha2 and codruta = @Ruta and eliminado = '0' ORDER BY fechaini DESC";

            using var connection = CreateConnection();
            var resultados = await connection.QueryAsync(consulta, new { Fecha1 = timestamp1, Fecha2 = timestamp2, Ruta = ruta });

            return resultados.Select(row => new DeviceDespachadas
            {
                Codigo = row.codigo,
                DeviceID = row.deviceID,
                Fechaini = ConvertirTimestampAHora(row.fechaini),
                Fechafin = row.fechafin != null
                    ? ConvertirTimestampAHora(row.fechafin + 1559)
                    : ConvertirTimestampAHora(row.fechaini + 6300)
            });
        }

        private string ConvertirTimestampAHora(long timestamp)
        {
            var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var fechaUtc = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            var fechaPeruana = TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, peruTimeZone);
            return fechaPeruana.ToString("HH:mm");
        }


        public async Task<string> EnvioControlGPS(ControlGPS[] logs, string user)
        {
            if (logs == null || logs.Length == 0)
                return "No hay registros para procesar";

            // Usar INSERT normal para "etudvrb", INSERT IGNORE para otros usuarios
            string sql = user.Equals("etudvrb", StringComparison.OrdinalIgnoreCase)
                ? @"INSERT INTO control_gpse (codasig, deviceID, nom_control, hora_inicio, hora_estimada, hora_llegada, volado, fecha) 
            VALUES (@Codasig, @DeviceID, @NomControl, @HoraInicio, @HoraEstimada, @HoraLlegada, @Volado, @Fecha)"
                : @"INSERT IGNORE INTO control_gps (codasig, deviceID, nom_control, hora_inicio, hora_estimada, hora_llegada, volado, fecha) 
            VALUES (@Codasig, @DeviceID, @NomControl, @HoraInicio, @HoraEstimada, @HoraLlegada, @Volado, @Fecha)";

            var parametros = logs.Select(gps => new
            {
                Codasig = gps.Codasig,
                DeviceID = gps.DeviceID,
                NomControl = gps.Nom_control,
                HoraInicio = gps.Hora_inicio,
                HoraEstimada = gps.Hora_estimada,
                HoraLlegada = gps.Hora_llegada,
                Volado = gps.Volado,
                Fecha = gps.Fecha
            });

            using var connection = CreateConnection();

            try
            {
                var registrosInsertados = await connection.ExecuteAsync(sql, parametros);
                return $"Se procesaron {registrosInsertados} de {logs.Length} registros correctamente";
            }
            catch (Exception ex)
            {
                // Para "etudvrb" capturamos errores de duplicados
                if (user.Equals("etudvrb", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Error al insertar registros: {ex.Message}";
                }
                throw;
            }
        }

        public async Task<string> EnvioControlB(ControlGPS[] logs, string user)
        {
            if (logs == null || logs.Length == 0)
                return "No hay registros para procesar";

            // Usar INSERT normal para "etudvrb", INSERT IGNORE para otros usuarios
            string sql = @"INSERT IGNORE INTO control_gpse (codasig, deviceID, nom_control, hora_inicio, hora_estimada, hora_llegada, volado, fecha) 
            VALUES (@Codasig, @DeviceID, @NomControl, @HoraInicio, @HoraEstimada, @HoraLlegada, @Volado, @Fecha)";

            var parametros = logs.Select(gps => new
            {
                Codasig = gps.Codasig,
                DeviceID = gps.DeviceID,
                NomControl = gps.Nom_control,
                HoraInicio = gps.Hora_inicio,
                HoraEstimada = gps.Hora_estimada,
                HoraLlegada = gps.Hora_llegada,
                Volado = gps.Volado,
                Fecha = gps.Fecha
            });

            using var connection = CreateConnection();

            try
            {
                var registrosInsertados = await connection.ExecuteAsync(sql, parametros);
                return $"Se procesaron {registrosInsertados} de {logs.Length} registros correctamente";
            }
            catch (Exception ex)
            {
                // Para "etudvrb" capturamos errores de duplicados
                if (user.Equals("etudvrb", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Error al insertar registros: {ex.Message}";
                }
                throw;
            }
        }

        //CAMBIAR RUTAACT PARA etudvrb AL LLEGAR A PARADERO 15
        public async Task<bool> ActualizarRutaact(string placa)
        {
            string sql = @"UPDATE device SET rutaact = '26' WHERE deviceID = @DeviceID";

            try
            {
                using var connection = CreateConnection();
                var result = await connection.ExecuteAsync(sql, new { DeviceID = placa });
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //CAMBIAR RUTAACT PARA etudvrg AL LLEGAR A TUPAC
        public async Task<bool> ActualizarRutaactG(string placa)
        {
            string sql = @"UPDATE device SET rutaact = '68' WHERE deviceID = @DeviceID";

            try
            {
                using var connection = CreateConnection();
                var result = await connection.ExecuteAsync(sql, new { DeviceID = placa });
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //CAMBIAR RUTAACT PARA serfrymh AL LLEGAR A WASHINTON
        public async Task<bool> ActualizarRutaactFR(string placa, string rutaact)
        {
            string sql = @"UPDATE device SET rutaact = @Rutaact WHERE deviceID = @DeviceID";

            try
            {
                using var connection = CreateConnection();
                var result = await connection.ExecuteAsync(sql, new { DeviceID = placa, Rutaact = rutaact });
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //FIN DATERO
    }
}