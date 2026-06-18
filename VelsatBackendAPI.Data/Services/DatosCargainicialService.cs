using APIDatero.Model;
using Dapper;
using MySqlConnector;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Services
{
    public class DatosCargainicialService : IDatosCargainicialService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private static List<Geocercausu> _geocercaUsuarios;
        private static readonly object _geocercaLock = new object();
        private List<string> _deviceIds;
        private string _currentLogin;

        public DatosCargainicialService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private void EnsureGeocercasLoaded()
        {
            if (_geocercaUsuarios != null) return;
            lock (_geocercaLock)
            {
                if (_geocercaUsuarios != null) return;
                try
                {
                    using var connection = _connectionFactory.GetDefaultConnection();
                    _geocercaUsuarios = connection.Query<Geocercausu>("SELECT codigo, descripcion, latitud, longitud FROM geocercausu").ToList();
                }
                catch
                {
                    _geocercaUsuarios = new List<Geocercausu>();
                }
            }
        }

        // Método para cargar los deviceIDs una sola vez por usuario
        private async Task<List<string>> GetDeviceIdsAsync(string login)
        {
            try
            {
                // Si ya tenemos los IDs para este usuario, los devolvemos
                if (_deviceIds != null && _currentLogin == login)
                {
                    return _deviceIds;
                }

                using var connection = _connectionFactory.GetDefaultConnection();

                // Cargar los deviceIDs
                const string sqlGetAllDeviceIds = @"
                    SELECT DISTINCT deviceID 
                    FROM (
                        SELECT deviceID FROM device WHERE accountID = @Login 
                        UNION ALL 
                        SELECT deviceID FROM gestion_villa.deviceuser WHERE userID = @Login AND Status = 1
                    ) AS combined_devices";

                _deviceIds = (await connection.QueryAsync<string>(sqlGetAllDeviceIds, new { Login = login })).ToList();
                _currentLogin = login;

                return _deviceIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error obteniendo deviceIds para {login}: {ex.Message}");
                return new List<string>();
            }
        }

        public Geocercausu ObtenerGeocercausuPorCodigo(string codigo)
        {
            EnsureGeocercasLoaded();
            return _geocercaUsuarios.FirstOrDefault(gu => gu.Codigo.ToString() == codigo);
        }

        public async Task<DatosCargainicial> ObtenerDatosCargaInicialAsync(string login)
        {
            try
            {
                // Obtener los deviceIDs (desde cache o BD)
                var deviceIds = await GetDeviceIdsAsync(login);

                if (!deviceIds.Any())
                {
                    return new DatosCargainicial
                    {
                        FechaActual = DateTime.Now,
                        DatosDevice = new List<Device>()
                    };
                }

                using var connection = _connectionFactory.GetDefaultConnection();

                // Consulta simplificada usando los IDs en cache
                const string sqlGetDevices = @"SELECT deviceID, lastGPSTimestamp, lastValidLatitude, lastValidLongitude, description, direccion, codgeoact, lastValidHeading, lastValidSpeed, rutaact FROM device WHERE deviceID IN @DeviceIDs";

                var devices = (await connection.QueryAsync<Device>(sqlGetDevices, new { DeviceIDs = deviceIds })).ToList();

                var datosCargaInicial = new DatosCargainicial
                {
                    FechaActual = DateTime.Now,
                    DatosDevice = devices
                };

                // Asignar geocercas
                foreach (var device in devices)
                {
                    device.DatosGeocercausu = ObtenerGeocercausuPorCodigo(device.Codgeoact);
                }

                return datosCargaInicial;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error obteniendo datos carga inicial para {login}: {ex.Message}");
                return new DatosCargainicial
                {
                    FechaActual = DateTime.Now,
                    DatosDevice = new List<Device>()
                };
            }
        }

        public async Task<IEnumerable<SimplifiedDevice>> SimplifiedList(string login)
        {
            try
            {
                // Obtener los deviceIDs (desde cache o BD)
                var deviceIds = await GetDeviceIdsAsync(login);

                if (!deviceIds.Any())
                {
                    return new List<SimplifiedDevice>();
                }

                using var connection = _connectionFactory.GetDefaultConnection();

                // Consulta optimizada usando los IDs en cache
                const string sqlGetSimplifiedDevices = @"SELECT DISTINCT d.deviceID, d.rutaact, d.lastValidSpeed, d.lastValidLatitude, d.lastValidLongitude FROM device d WHERE d.deviceID IN @DeviceIds";

                var listDevices = await connection.QueryAsync<SimplifiedDevice>(
                    sqlGetSimplifiedDevices,
                    new { DeviceIds = deviceIds });

                var result = listDevices.ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<DatosVehiculo> ObtenerDatosVehiculoAsync(string login, string placa)
        {
            try
            {
                var deviceIds = await GetDeviceIdsAsync(login);
                if (!deviceIds.Any())
                {
                    return new DatosVehiculo
                    {
                        FechaActual = DateTime.Now,
                        FechaGPS = null,
                        Vehiculo = null
                    };
                }

                using var connection = _connectionFactory.GetDefaultConnection();

                const string sqlGetVehicle = @"
            SELECT deviceID, lastGPSTimestamp, lastValidLatitude, lastValidLongitude, 
                   lastValidHeading, lastValidSpeed, lastOdometerKM, direccion 
            FROM device 
            WHERE deviceID IN @DeviceIDs AND deviceID = @Placa";

                var vehiculo = await connection.QueryFirstOrDefaultAsync<Device>(
                    sqlGetVehicle,
                    new { DeviceIDs = deviceIds, Placa = placa });

                // ✅ Convertir timestamp del GPS a DateTime si está disponible
                DateTime? fechaGPS = null;

                if (vehiculo != null && vehiculo.lastGPSTimestamp > 0)
                {
                    try
                    {
                        fechaGPS = DateTimeOffset.FromUnixTimeSeconds(vehiculo.lastGPSTimestamp)
                            .DateTime
                            .ToLocalTime(); // Convertir a hora local
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Error convirtiendo timestamp {vehiculo.lastGPSTimestamp}: {ex.Message}");
                        fechaGPS = null;
                    }
                }

                return new DatosVehiculo
                {
                    FechaActual = DateTime.Now,  // ✅ Mantener - para otros sistemas
                    FechaGPS = fechaGPS,         // ✅ NUEVO - hora real del GPS
                    Vehiculo = vehiculo
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error obteniendo datos vehículo {placa}: {ex.Message}");
                return new DatosVehiculo
                {
                    FechaActual = DateTime.Now,
                    FechaGPS = null,
                    Vehiculo = null
                };
            }
        }


        // Método opcional para limpiar el cache si es necesario
        public void ClearDeviceIdsCache()
        {
            _deviceIds = null;
            _currentLogin = null;
        }
    }
}