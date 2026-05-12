using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public class KilometrosRepository : IKilometrosRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public KilometrosRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private IDbConnection CreateDefaultConnection() => _connectionFactory.GetDefaultConnection();
        private IDbConnection CreateSecondConnection() => _connectionFactory.GetSecondConnection();

        public async Task<KilometrosReporting> GetKmReporting(string fechaini, string fechafin, string deviceID, string accountID)
        {

            try
            {
                var dates = FormatDate(fechaini, fechafin);
                fechaini = dates.dateStart;
                fechafin = dates.dateEnd;

                var resultadoDias = CalcularDias(fechaini, fechafin);
                double numdias = resultadoDias.NumDias;

                if (numdias <= 5)
                {
                    using var defaultConnection = CreateDefaultConnection();

                    const string sql = "select tabla from historicos where timeini<=@FechafinUnix and timefin>=@FechainiUnix";

                    var nombresTablas = await defaultConnection.QueryAsync<Historicos>(sql, new { FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                    var nombresTablasList = nombresTablas.ToList();

                    var kilometrosReporting = new KilometrosReporting
                    {
                        ListaKilometros = new List<KilometrosRecorridos>()
                    };

                    if (nombresTablasList.Count == 0)
                    {
                        // Si no se encontraron nombres de tablas, consultamos directamente la tabla "eventdata"
                        string sqlEventData = @"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo, (MAX(odometerKM) - MIN(odometerKM)) as kilometros from eventdata where accountID = @AccountID and deviceID = @DeviceID and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

                        var eventDataList = await defaultConnection.QueryAsync<KilometrosRecorridos>(sqlEventData, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                        kilometrosReporting.ListaKilometros = eventDataList.ToList();

                        for (int i = 0; i < kilometrosReporting.ListaKilometros.Count; i++)
                        {
                            kilometrosReporting.ListaKilometros[i].Item = i + 1;
                        }
                    }
                    else
                    {
                        using var secondConnection = CreateSecondConnection();

                        foreach (var nombreTabla in nombresTablasList)
                        {
                            string consultaTabla = nombreTabla.Tabla;

                            string sqlR = $@"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo, (MAX(odometerKM) - MIN(odometerKM)) as kilometros from {consultaTabla} where accountID = @AccountID and deviceID = @DeviceID and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

                            var datosTabla = await secondConnection.QueryAsync<KilometrosRecorridos>(sqlR, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                            var datosTablaList = datosTabla.ToList();

                            kilometrosReporting.ListaKilometros.AddRange(datosTablaList);
                        }

                        for (int i = 0; i < kilometrosReporting.ListaKilometros.Count; i++)
                        {
                            kilometrosReporting.ListaKilometros[i].Item = i + 1;
                        }
                    }

                    if (kilometrosReporting.ListaKilometros.Count == 0 || kilometrosReporting.ListaKilometros == null)
                    {
                        return new KilometrosReporting
                        {
                            Mensaje = "No se encontro datos disponible en el rango de fechas ingresado"
                        };
                    }
                    return kilometrosReporting;
                }
                else
                {
                    return new KilometrosReporting
                    {
                        Mensaje = "La diferencia entre las fechas es mayor a 5 días; seleccione otra fechas"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<KilometrosReporting> GetAllKmReporting(string fechaini, string fechafin, string accountID)
        {
            try
            {
                var dates = FormatDate(fechaini, fechafin);
                fechaini = dates.dateStart;
                fechafin = dates.dateEnd;

                var resultadoDias = CalcularDias(fechaini, fechafin);
                double numdias = resultadoDias.NumDias;

                var kilometrosReporting = new KilometrosReporting
                {
                    ListaKilometros = new List<KilometrosRecorridos>()
                };

                if (numdias <= 5)
                {
                    using var defaultConnection = CreateDefaultConnection();

                    // Consultar la tabla "historicos"
                    const string sql = "select tabla from historicos where timeini <= @FechafinUnix and timefin >= @FechainiUnix";
                    var nombresTablas = await defaultConnection.QueryAsync<Historicos>(sql, new { FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                    var nombresTablasList = nombresTablas.ToList();

                    if (nombresTablasList.Count == 0)
                    {
                        // Consultar directamente la tabla "eventdata"
                        string sqlEventData = $"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo " +
                            $"from eventdata where deviceID in (select deviceID from device where accountID=@AccountID) and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

                        var parameters = new DynamicParameters();
                        parameters.Add("FechainiUnix", resultadoDias.UnixFechaInicio);
                        parameters.Add("FechafinUnix", resultadoDias.UnixFechaFin);
                        parameters.Add("AccountID", accountID);

                        var eventDataList = await defaultConnection.QueryAsync<KilometrosRecorridos>(sqlEventData, parameters);
                        kilometrosReporting.ListaKilometros = eventDataList.ToList();
                    }
                    else
                    {
                        using var secondConnection = CreateSecondConnection();
                        foreach (var nombreTabla in nombresTablasList)
                        {
                            string consultaTabla = nombreTabla.Tabla;

                            // Consultar cada tabla histórica
                            string sqlR = $"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo " +
                                $"from {consultaTabla} where deviceID in (select deviceID from gestion_villa.device where accountID=@AccountID) and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

                            var parameters = new DynamicParameters();
                            parameters.Add("FechainiUnix", resultadoDias.UnixFechaInicio);
                            parameters.Add("FechafinUnix", resultadoDias.UnixFechaFin);
                            parameters.Add("AccountID", accountID);

                            var datosTabla = await secondConnection.QueryAsync<KilometrosRecorridos>(sqlR, parameters);
                            var datosTablaList = datosTabla.ToList();

                            kilometrosReporting.ListaKilometros.AddRange(datosTablaList);
                        }
                    }

                    for (int i = 0; i < kilometrosReporting.ListaKilometros.Count; i++)
                    {
                        kilometrosReporting.ListaKilometros[i].Item = i + 1;
                    }

                    if (kilometrosReporting.ListaKilometros.Count == 0)
                    {
                        return new KilometrosReporting
                        {
                            Mensaje = "No se encontró datos disponible en el rango de fechas ingresado"
                        };
                    }

                    return kilometrosReporting;
                }
                else
                {
                    return new KilometrosReporting
                    {
                        Mensaje = "La diferencia entre las fechas es mayor a 5 días; seleccione otra fechas"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public class ResultadosCalculoDias
        {
            public double NumDias { get; set; }
            public int UnixFechaInicio { get; set; }
            public int UnixFechaFin { get; set; }
        }

        public int DateUnix(string fecha)
        {
            try
            {
                fecha = WebUtility.UrlDecode(fecha);
                DateTime fechaTime = DateTime.ParseExact(fecha, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                int unixFecha = (int)(fechaTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

                return unixFecha;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error convirtiendo fecha: {ex.Message}");
                throw;
            }
        }

        public ResultadosCalculoDias CalcularDias(string fechaI, string fechaF)
        {
            try
            {
                int fechainiUnix = DateUnix(fechaI);
                int fechafinUnix = DateUnix(fechaF);

                int totalsegundos = fechafinUnix - fechainiUnix;
                double numdias = (double)totalsegundos / 86400;

                return new ResultadosCalculoDias
                {
                    NumDias = numdias,
                    UnixFechaInicio = fechainiUnix,
                    UnixFechaFin = fechafinUnix,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error calculando días: {ex.Message}");
                throw;
            }
        }

        public class ResultDate
        {
            public string dateStart { get; set; }
            public string dateEnd { get; set; }
        }

        public ResultDate FormatDate(string dateS, string dateE)
        {
            try
            {
                DateTime.TryParse(dateS, out DateTime fechaInicio);
                DateTime.TryParse(dateE, out DateTime fechaFin);

                string fechaInicioString = fechaInicio.ToString("dd/MM/yyyy HH:mm");
                string fechaFinString = fechaFin.ToString("dd/MM/yyyy HH:mm");

                return new ResultDate
                {
                    dateStart = fechaInicioString,
                    dateEnd = fechaFinString,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error formateando fechas: {ex.Message}");
                throw;
            }
        }
    }
}