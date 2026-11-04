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

                if (numdias > 5)
                {
                    return new KilometrosReporting
                    {
                        Mensaje = "La diferencia entre las fechas es mayor a 5 días; seleccione otra fechas"
                    };
                }

                // La depuración diaria mueve a historicos, con un retraso de 7 días, el registro
                // del día calendario correspondiente (hoy - 7). Todo lo posterior a ese día
                // (los últimos 6 días hasta hoy) aún no ha sido migrado y sigue en eventdata.
                DateTime diaLimite = DateTime.Now.Date.AddDays(-7);
                int unixLimite = DateUnix(diaLimite.ToString("dd/MM/yyyy") + " 23:59");

                using var defaultConnection = CreateDefaultConnection();

                List<KilometrosRecorridos> lista;

                if (resultadoDias.UnixFechaFin <= unixLimite)
                {
                    lista = await ConsultarHistoricosKm(defaultConnection, accountID, deviceID, resultadoDias.UnixFechaInicio, resultadoDias.UnixFechaFin);
                }
                else if (resultadoDias.UnixFechaInicio > unixLimite)
                {
                    lista = await ConsultarEventDataKm(defaultConnection, accountID, deviceID, resultadoDias.UnixFechaInicio, resultadoDias.UnixFechaFin);
                }
                else
                {
                    // El rango solicitado cruza el límite de depuración: una parte está en
                    // historicos y la parte más reciente todavía está en eventdata. El máximo y
                    // mínimo de odómetro deben combinarse por dispositivo, no concatenarse.
                    var datosHistoricos = await ConsultarHistoricosKm(defaultConnection, accountID, deviceID, resultadoDias.UnixFechaInicio, unixLimite);
                    var datosEventData = await ConsultarEventDataKm(defaultConnection, accountID, deviceID, unixLimite + 1, resultadoDias.UnixFechaFin);

                    lista = CombinarKilometros(datosHistoricos, datosEventData);
                }

                var kilometrosReporting = new KilometrosReporting
                {
                    ListaKilometros = lista
                };

                for (int i = 0; i < kilometrosReporting.ListaKilometros.Count; i++)
                {
                    kilometrosReporting.ListaKilometros[i].Item = i + 1;
                }

                if (kilometrosReporting.ListaKilometros.Count == 0)
                {
                    return new KilometrosReporting
                    {
                        Mensaje = "No se encontro datos disponible en el rango de fechas ingresado"
                    };
                }

                return kilometrosReporting;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<List<KilometrosRecorridos>> ConsultarHistoricosKm(IDbConnection defaultConnection, string accountID, string deviceID, int unixFechaInicio, int unixFechaFin)
        {
            const string sql = "select tabla from historicos where timeini<=@FechafinUnix and timefin>=@FechainiUnix";

            var nombresTablas = (await defaultConnection.QueryAsync<Historicos>(sql, new { FechainiUnix = unixFechaInicio, FechafinUnix = unixFechaFin })).ToList();

            if (nombresTablas.Count == 0)
            {
                // Si no hay tablas históricas para el rango, se consulta directamente "eventdata"
                return await ConsultarEventDataKm(defaultConnection, accountID, deviceID, unixFechaInicio, unixFechaFin);
            }

            var lista = new List<KilometrosRecorridos>();

            using var secondConnection = CreateSecondConnection();

            foreach (var nombreTabla in nombresTablas)
            {
                string consultaTabla = nombreTabla.Tabla;

                string sqlR = $@"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo, (MAX(odometerKM) - MIN(odometerKM)) as kilometros from {consultaTabla} where accountID = @AccountID and deviceID = @DeviceID and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

                var datosTabla = await secondConnection.QueryAsync<KilometrosRecorridos>(sqlR, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = unixFechaInicio, FechafinUnix = unixFechaFin });

                lista.AddRange(datosTabla);
            }

            return lista;
        }

        private async Task<List<KilometrosRecorridos>> ConsultarEventDataKm(IDbConnection defaultConnection, string accountID, string deviceID, int unixFechaInicio, int unixFechaFin)
        {
            const string sqlEventData = @"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo, (MAX(odometerKM) - MIN(odometerKM)) as kilometros from eventdata where accountID = @AccountID and deviceID = @DeviceID and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

            var datos = await defaultConnection.QueryAsync<KilometrosRecorridos>(sqlEventData, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = unixFechaInicio, FechafinUnix = unixFechaFin });

            return datos.ToList();
        }

        private List<KilometrosRecorridos> CombinarKilometros(List<KilometrosRecorridos> a, List<KilometrosRecorridos> b)
        {
            var combinado = new List<KilometrosRecorridos>();

            foreach (var grupo in a.Concat(b).GroupBy(k => k.DeviceId))
            {
                double maximo = grupo.Max(k => k.Maximo);
                double minimo = grupo.Min(k => k.Minimo);

                combinado.Add(new KilometrosRecorridos
                {
                    DeviceId = grupo.Key,
                    Maximo = maximo,
                    Minimo = minimo,
                    Kilometros = maximo - minimo
                });
            }

            return combinado;
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

                if (numdias > 5)
                {
                    return new KilometrosReporting
                    {
                        Mensaje = "La diferencia entre las fechas es mayor a 5 días; seleccione otra fechas"
                    };
                }

                DateTime diaLimite = DateTime.Now.Date.AddDays(-7);
                int unixLimite = DateUnix(diaLimite.ToString("dd/MM/yyyy") + " 23:59");

                using var defaultConnection = CreateDefaultConnection();

                List<KilometrosRecorridos> lista;

                if (resultadoDias.UnixFechaFin <= unixLimite)
                {
                    lista = await ConsultarHistoricosKmAll(defaultConnection, accountID, resultadoDias.UnixFechaInicio, resultadoDias.UnixFechaFin);
                }
                else if (resultadoDias.UnixFechaInicio > unixLimite)
                {
                    lista = await ConsultarEventDataKmAll(defaultConnection, accountID, resultadoDias.UnixFechaInicio, resultadoDias.UnixFechaFin);
                }
                else
                {
                    var datosHistoricos = await ConsultarHistoricosKmAll(defaultConnection, accountID, resultadoDias.UnixFechaInicio, unixLimite);
                    var datosEventData = await ConsultarEventDataKmAll(defaultConnection, accountID, unixLimite + 1, resultadoDias.UnixFechaFin);

                    lista = CombinarKilometros(datosHistoricos, datosEventData);
                }

                var kilometrosReporting = new KilometrosReporting
                {
                    ListaKilometros = lista
                };

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
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<List<KilometrosRecorridos>> ConsultarHistoricosKmAll(IDbConnection defaultConnection, string accountID, int unixFechaInicio, int unixFechaFin)
        {
            const string sql = "select tabla from historicos where timeini <= @FechafinUnix and timefin >= @FechainiUnix";
            var nombresTablas = (await defaultConnection.QueryAsync<Historicos>(sql, new { FechainiUnix = unixFechaInicio, FechafinUnix = unixFechaFin })).ToList();

            if (nombresTablas.Count == 0)
            {
                // Si no hay tablas históricas para el rango, se consulta directamente "eventdata"
                return await ConsultarEventDataKmAll(defaultConnection, accountID, unixFechaInicio, unixFechaFin);
            }

            var lista = new List<KilometrosRecorridos>();

            using var secondConnection = CreateSecondConnection();

            foreach (var nombreTabla in nombresTablas)
            {
                string consultaTabla = nombreTabla.Tabla;

                // Consultar cada tabla histórica
                string sqlR = $"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo " +
                    $"from {consultaTabla} where deviceID in (select deviceID from device where accountID=@AccountID) and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

                var parameters = new DynamicParameters();
                parameters.Add("FechainiUnix", unixFechaInicio);
                parameters.Add("FechafinUnix", unixFechaFin);
                parameters.Add("AccountID", accountID);

                var datosTabla = await secondConnection.QueryAsync<KilometrosRecorridos>(sqlR, parameters);

                lista.AddRange(datosTabla);
            }

            return lista;
        }

        private async Task<List<KilometrosRecorridos>> ConsultarEventDataKmAll(IDbConnection defaultConnection, string accountID, int unixFechaInicio, int unixFechaFin)
        {
            string sqlEventData = $"select deviceID, MAX(odometerKM) AS maximo, MIN(odometerKM) AS minimo " +
                $"from eventdata where deviceID in (select deviceID from device where accountID=@AccountID) and timestamp between @FechainiUnix and @FechafinUnix group by deviceID";

            var parameters = new DynamicParameters();
            parameters.Add("FechainiUnix", unixFechaInicio);
            parameters.Add("FechafinUnix", unixFechaFin);
            parameters.Add("AccountID", accountID);

            var datos = await defaultConnection.QueryAsync<KilometrosRecorridos>(sqlEventData, parameters);

            return datos.ToList();
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