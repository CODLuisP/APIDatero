using Dapper;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Net;
using Serilog;
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
    public class HistoricosRepository : IHistoricosRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HistoricosRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            Console.WriteLine("[DEBUG] HistoricosRepository inicializado con IDbConnectionFactory");
        }

        private IDbConnection CreateDefaultConnection() => _connectionFactory.GetDefaultConnection();
        private IDbConnection CreateSecondConnection() => _connectionFactory.GetSecondConnection();

        public async Task<DatosReporting> GetDataReporting(string fechaini, string fechafin, string deviceID, string accountID)
        {
            try
            {
                // Paso 1: Buscar accountID en device
                const string sqlAccountFromDevice = "SELECT accountID FROM device WHERE deviceID = @DeviceID";

                using var defaultConnection = CreateDefaultConnection();

                var newAccountID = await defaultConnection.QueryFirstOrDefaultAsync<string>(sqlAccountFromDevice, new { DeviceID = deviceID });

                if (!string.IsNullOrEmpty(newAccountID))
                {
                    accountID = newAccountID;
                }
                else
                {
                    // Validar que tengamos un accountID válido
                    if (string.IsNullOrEmpty(accountID))
                    {
                        return new DatosReporting
                        {
                            Mensaje = $"No se encontró información para el deviceID: {deviceID}"
                        };
                    }
                }

                var dates = FormatDate(fechaini, fechafin);
                fechaini = dates.dateStart;
                fechafin = dates.dateEnd;

                var resultadoDias = CalcularDias(fechaini, fechafin);
                double numdias = resultadoDias.NumDias;

                if (numdias <= 3)
                {
                    const string sql = "select tabla from historicos where timeini<=@FechafinUnix and timefin>=@FechainiUnix";

                    var nombresTablas = await defaultConnection.QueryAsync<Historicos>(sql, new { FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                    var nombresTablasList = nombresTablas.ToList();

                    var datosReporting = new DatosReporting
                    {
                        ListaTablas = new List<TablasReporting>()
                    };

                    if (nombresTablasList.Count == 0)
                    {
                        // Si no se encontraron nombres de tablas, consultamos directamente la tabla "eventdata"
                        string sqlEventData = @"
                            SELECT deviceID, timestamp, speedKPH, longitude, latitude, odometerKM, address 
                             FROM eventdata 
                                WHERE accountID = @AccountID AND deviceID = @DeviceID AND timestamp BETWEEN @FechainiUnix AND @FechafinUnix
                                    ORDER BY timestamp";

                        var eventDataList = await defaultConnection.QueryAsync<TablasReporting>(sqlEventData, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                        datosReporting.ListaTablas = eventDataList.ToList();

                        for (int i = 0; i < datosReporting.ListaTablas.Count; i++)
                        {
                            datosReporting.ListaTablas[i].Item = i + 1;
                        }
                    }
                    else
                    {
                        using var secondConnection = CreateSecondConnection();

                        foreach (var nombreTabla in nombresTablasList)
                        {
                            string consultaTabla = nombreTabla.Tabla;

                            string sqlR = $@"
                               SELECT deviceID, timestamp, speedKPH, longitude, latitude, odometerKM, address 
                                    FROM {consultaTabla} 
                                        WHERE accountID = @AccountID AND deviceID = @DeviceID AND timestamp BETWEEN @FechainiUnix AND @FechafinUnix
                                          ORDER BY timestamp";

                            var datosTabla = await secondConnection.QueryAsync<TablasReporting>(sqlR, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin });
                            var datosTablaList = datosTabla.ToList();

                            datosReporting.ListaTablas.AddRange(datosTablaList);
                        }

                        for (int i = 0; i < datosReporting.ListaTablas.Count; i++)
                        {
                            datosReporting.ListaTablas[i].Item = i + 1;
                        }
                    }

                    if (datosReporting.ListaTablas.Count == 0 || datosReporting.ListaTablas == null)
                    {
                        return new DatosReporting
                        {
                            Mensaje = "No se encontro datos disponible en el rango de fechas ingresado"
                        };
                    }

                    return datosReporting;
                }
                else
                {
                    return new DatosReporting
                    {
                        Mensaje = "La diferencia entre las fechas es mayor a 3 días; seleccione otra fechas"
                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int DateUnix(string fecha)
        {
            Console.WriteLine($"[DEBUG] Convirtiendo fecha a Unix: {fecha}");

            try
            {
                fecha = WebUtility.UrlDecode(fecha);
                DateTime fechaTime = DateTime.ParseExact(fecha, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                int unixFecha = (int)(fechaTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

                Console.WriteLine($"[DEBUG] Fecha convertida exitosamente: {unixFecha}");
                return unixFecha;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error convirtiendo fecha: {ex.Message}");
                throw;
            }
        }

        public class ResultadosCalculoDias
        {
            public double NumDias { get; set; }
            public int UnixFechaInicio { get; set; }
            public int UnixFechaFin { get; set; }
        }

        public ResultadosCalculoDias CalcularDias(string fechaI, string fechaF)
        {
            Console.WriteLine($"[DEBUG] Calculando días entre {fechaI} y {fechaF}");

            try
            {
                int fechainiUnix = DateUnix(fechaI);
                int fechafinUnix = DateUnix(fechaF);

                int totalsegundos = fechafinUnix - fechainiUnix;
                double numdias = (double)totalsegundos / 86400;

                Console.WriteLine($"[DEBUG] Cálculo completado: {numdias} días");

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

        public async Task<List<SpeedReporting>> GetSpeedData(string fechaini, string fechafin, string deviceID, double speedKPH, string accountID)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetSpeedData para deviceID: {deviceID}, speedKPH: {speedKPH}");

            try
            {
                var datosreporte = await GetDataReporting(fechaini, fechafin, deviceID, accountID);

                List<SpeedReporting> SpeedData = new List<SpeedReporting>();

                if (datosreporte != null && datosreporte.ListaTablas.Count > 0)
                {
                    for (int i = 0; i < datosreporte.ListaTablas.Count - 1; i++)
                    {
                        if (datosreporte.ListaTablas[i].SpeedKPH >= speedKPH)
                        {
                            SpeedReporting data = new SpeedReporting
                            {
                                Item = SpeedData.Count + 1,
                                SpeedKPH = datosreporte.ListaTablas[i].SpeedKPH,
                                Date = datosreporte.ListaTablas[i].Fecha,
                                Time = datosreporte.ListaTablas[i].Hora,
                                Latitude = datosreporte.ListaTablas[i].Latitude,
                                Longitude = datosreporte.ListaTablas[i].Longitude,
                                Address = datosreporte.ListaTablas[i].Address
                            };
                            SpeedData.Add(data);
                        }
                    }
                }

                Console.WriteLine($"[DEBUG] GetSpeedData completado: {SpeedData.Count} registros encontrados");
                return SpeedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en GetSpeedData: {ex.Message}");
                throw;
            }
        }

        public async Task<List<StopsReporting>> GetStopData(string fechaini, string fechafin, string deviceID, string accountID)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetStopData para deviceID: {deviceID}");

            try
            {
                var datosreporte = await GetDataReporting(fechaini, fechafin, deviceID, accountID);

                List<StopsReporting> StopsData = new List<StopsReporting>();

                if (datosreporte != null && datosreporte.ListaTablas.Count > 0)
                {
                    int Contador = 0;
                    int PuntoFinal = 0;

                    for (int i = 0; i < datosreporte.ListaTablas.Count - 1; i++)
                    {
                        if (datosreporte.ListaTablas[i].SpeedKPH == 0 && datosreporte.ListaTablas[i + 1].SpeedKPH == 0)
                        {
                            Contador++;

                            int ultimoelemento = datosreporte.ListaTablas.Count - 1;

                            if ((i + 1) == ultimoelemento && Contador > 0)
                            {
                                PuntoFinal = i + 1;

                                StopsReporting stop = new StopsReporting
                                {
                                    Item = StopsData.Count + 1,
                                    StartDate = datosreporte.ListaTablas[PuntoFinal - Contador].Fecha,
                                    StartTime = datosreporte.ListaTablas[PuntoFinal - Contador].Hora,
                                    EndDate = datosreporte.ListaTablas[PuntoFinal].Fecha,
                                    EndTime = datosreporte.ListaTablas[PuntoFinal].Hora,
                                    Longitude = datosreporte.ListaTablas[PuntoFinal - Contador].Longitude,
                                    Latitude = datosreporte.ListaTablas[PuntoFinal - Contador].Latitude,
                                    Address = datosreporte.ListaTablas[PuntoFinal].Address,
                                    TimeStampIni = datosreporte.ListaTablas[PuntoFinal - Contador].Timestamp,
                                    TimeStampEnd = datosreporte.ListaTablas[PuntoFinal].Timestamp
                                };

                                int diferenciaEnSegundos = stop.TimeStampEnd - stop.TimeStampIni;

                                int horas = diferenciaEnSegundos / 3600;
                                int minutos = (diferenciaEnSegundos % 3600) / 60;
                                int segundos = diferenciaEnSegundos % 60;

                                string totalTime = $"{horas:D2}H:{minutos:D2}M:{segundos:D2}S";

                                stop.TotalTime = totalTime;

                                StopsData.Add(stop);
                                Contador = 0;
                            }
                        }
                        else
                        {
                            if (Contador > 0)
                            {
                                StopsReporting stop = new StopsReporting
                                {
                                    Item = StopsData.Count + 1,
                                    StartDate = datosreporte.ListaTablas[i - Contador].Fecha,
                                    StartTime = datosreporte.ListaTablas[i - Contador].Hora,
                                    EndDate = datosreporte.ListaTablas[i].Fecha,
                                    EndTime = datosreporte.ListaTablas[i].Hora,
                                    Longitude = datosreporte.ListaTablas[i - Contador].Longitude,
                                    Latitude = datosreporte.ListaTablas[i - Contador].Latitude,
                                    Address = datosreporte.ListaTablas[i].Address,
                                    TimeStampIni = datosreporte.ListaTablas[i - Contador].Timestamp,
                                    TimeStampEnd = datosreporte.ListaTablas[i].Timestamp
                                };

                                int diferenciaEnSegundos = stop.TimeStampEnd - stop.TimeStampIni;

                                int horas = diferenciaEnSegundos / 3600;
                                int minutos = (diferenciaEnSegundos % 3600) / 60;
                                int segundos = diferenciaEnSegundos % 60;

                                string totalTime = $"{horas:D2}H:{minutos:D2}M:{segundos:D2}S";

                                stop.TotalTime = totalTime;

                                StopsData.Add(stop);
                                Contador = 0;
                            }
                        }
                    }
                }

                Console.WriteLine($"[DEBUG] GetStopData completado: {StopsData.Count} paradas encontradas");
                return StopsData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en GetStopData: {ex.Message}");
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
            Console.WriteLine($"[DEBUG] Formateando fechas: {dateS} - {dateE}");

            try
            {
                DateTime.TryParse(dateS, out DateTime fechaInicio);
                DateTime.TryParse(dateE, out DateTime fechaFin);

                string fechaInicioString = fechaInicio.ToString("dd/MM/yyyy HH:mm");
                string fechaFinString = fechaFin.ToString("dd/MM/yyyy HH:mm");

                Console.WriteLine($"[DEBUG] Fechas formateadas: {fechaInicioString} - {fechaFinString}");

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

        public async Task<List<RouteDetails>> GetRouteDetails(string fechaini, string fechafin, string deviceID, string accountID)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetRouteDetails para deviceID: {deviceID}");

            try
            {
                var datareport = await GetDataReporting(fechaini, fechafin, deviceID, accountID);

                List<RouteDetails> DetailsData = new List<RouteDetails>();

                if (datareport != null && datareport.ListaTablas.Count > 0)
                {
                    int Contador = 0;
                    int PuntoFinal = 0;

                    int ultimoelemento = datareport.ListaTablas.Count - 1;

                    for (int i = 0; i < datareport.ListaTablas.Count - 1; i++)
                    {
                        if (datareport.ListaTablas[i].SpeedKPH == 0 && datareport.ListaTablas[i + 1].SpeedKPH == 0)
                        {
                            Contador++;

                            if ((i + 1) == ultimoelemento && Contador > 0)
                            {
                                PuntoFinal = i + 1;

                                RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[PuntoFinal - Contador]);

                                DetailsData.Add(stop);
                                Contador = 0;
                            }
                        }
                        else
                        {
                            if (Contador == 0)
                            {
                                RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[i]);
                                DetailsData.Add(stop);
                                Contador = 0;
                            }

                            if (Contador > 0)
                            {
                                RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[i - Contador]);

                                DetailsData.Add(stop);
                                Contador = 0;
                            }

                            if ((i + 1) == ultimoelemento && datareport.ListaTablas[ultimoelemento].SpeedKPH > 0)
                            {
                                RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[ultimoelemento]);
                                DetailsData.Add(stop);
                                Contador = 0;
                            }
                        }
                    }
                }

                Console.WriteLine($"[DEBUG] GetRouteDetails completado: {DetailsData.Count} detalles de ruta");
                return DetailsData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en GetRouteDetails: {ex.Message}");
                throw;
            }
        }

        private RouteDetails CreateRouteDetails(TablasReporting gpsData)
        {
            return new RouteDetails
            {
                Date = gpsData.Fecha,
                Time = gpsData.Hora,
                Speed = gpsData.SpeedKPH,
                Longitude = gpsData.Longitude,
                Latitude = gpsData.Latitude,
            };
        }

        public string UserName(string deviceID)
        {
            Console.WriteLine($"[DEBUG] Obteniendo UserName para deviceID: {deviceID}");

            try
            {
                using var connection = CreateDefaultConnection();

                const string sql = "select accountID from gestion_villa.device where deviceID = @DeviceID";
                string account = connection.QueryFirstOrDefault<string>(sql, new { DeviceID = deviceID });
                Console.WriteLine($"[DEBUG] Account obtenido: {account}");

                const string sqlUser = "Select description from gestion_villa.usuarios where accountID = @AccountId";
                string userName = connection.QueryFirstOrDefault<string>(sqlUser, new { AccountId = account });
                Console.WriteLine($"[DEBUG] UserName obtenido: {userName}");

                return userName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en UserName: {ex.Message}");
                throw;
            }
        }
    }
}