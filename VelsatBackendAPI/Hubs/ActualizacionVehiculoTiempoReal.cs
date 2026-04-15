using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI;
using VelsatBackendAPI.Data.Repositories;

namespace APIDatero.Hubs
{
    public class ActualizacionVehiculoTiempoReal : Hub
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHubContext<ActualizacionVehiculoTiempoReal> _hubContext;
        private static readonly Dictionary<string, Timer> _vehicleTimers = new Dictionary<string, Timer>();
        private static readonly object _lockObject = new object();

        public ActualizacionVehiculoTiempoReal(
            IServiceScopeFactory serviceScopeFactory,
            IHubContext<ActualizacionVehiculoTiempoReal> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _hubContext = hubContext;
        }

        public async Task UnirGrupoVehiculo()
        {
            var username = GetUsernameFromRoute();
            var placa = GetPlacaFromRoute();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(placa))
            {
                await Clients.Caller.SendAsync("Error", "Username o placa no encontrados en la ruta");
                return;
            }

            try
            {
                // Crear identificador único para el grupo: "username_placa"
                var groupKey = $"{username}_{placa}";

                await Groups.AddToGroupAsync(Context.ConnectionId, groupKey);
                IniciarTimerVehiculo(username, placa);
                await Clients.Caller.SendAsync("ConectadoExitosamente", new { username, placa });

                Console.WriteLine($"[DEBUG] Usuario {username} se unió al tracking del vehículo {placa}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error uniendo al grupo del vehículo: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Error al unirse al grupo: {ex.Message}");
            }
        }

        public async Task DejarGrupoVehiculo()
        {
            var username = GetUsernameFromRoute();
            var placa = GetPlacaFromRoute();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(placa))
                return;

            try
            {
                var groupKey = $"{username}_{placa}";

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupKey);
                DetenerTimerVehiculo(username, placa);

                Console.WriteLine($"[DEBUG] Usuario {username} dejó el tracking del vehículo {placa}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error dejando el grupo del vehículo: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var username = GetUsernameFromRoute();
            var placa = GetPlacaFromRoute();

            Console.WriteLine($"[DEBUG] Cliente conectado: {Context.ConnectionId}, Username: {username}, Placa: {placa}");

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(placa))
            {
                await UnirGrupoVehiculo();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = GetUsernameFromRoute();
            var placa = GetPlacaFromRoute();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(placa))
            {
                DetenerTimerVehiculo(username, placa);
                Console.WriteLine($"[DEBUG] Usuario {username} desconectado del vehículo {placa}, timer detenido");
            }

            await base.OnDisconnectedAsync(exception);
        }

        private string GetUsernameFromRoute()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext?.Request.RouteValues.TryGetValue("username", out var usernameObj) == true)
                {
                    return usernameObj?.ToString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error obteniendo username de la ruta: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetPlacaFromRoute()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext?.Request.RouteValues.TryGetValue("placa", out var placaObj) == true)
                {
                    return placaObj?.ToString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error obteniendo placa de la ruta: {ex.Message}");
                return string.Empty;
            }
        }

        // ============= MÉTODOS DEL TIMER PARA VEHÍCULO =============

        private void IniciarTimerVehiculo(string username, string placa)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(placa))
                return;

            var timerKey = $"{username}_{placa}";

            lock (_lockObject)
            {
                // Si ya existe un timer para este vehículo, lo detenemos primero
                if (_vehicleTimers.ContainsKey(timerKey))
                {
                    _vehicleTimers[timerKey].Dispose();
                    _vehicleTimers.Remove(timerKey);
                }

                // Crear nuevo timer
                var timer = new Timer(async _ => await EnviarDatosVehiculo(username, placa),
                                    null,
                                    TimeSpan.FromSeconds(1),
                                    TimeSpan.FromSeconds(5));

                _vehicleTimers[timerKey] = timer;
            }

            Console.WriteLine($"[DEBUG] Timer iniciado para vehículo: {placa} del usuario: {username}");
        }

        private void DetenerTimerVehiculo(string username, string placa)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(placa))
                return;

            var timerKey = $"{username}_{placa}";

            lock (_lockObject)
            {
                if (_vehicleTimers.ContainsKey(timerKey))
                {
                    _vehicleTimers[timerKey].Dispose();
                    _vehicleTimers.Remove(timerKey);
                    Console.WriteLine($"[DEBUG] Timer detenido para vehículo: {placa}");
                }
            }
        }

        private async Task EnviarDatosVehiculo(string username, string placa)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    Console.WriteLine($"[DEBUG] Obteniendo datos del vehículo {placa} para usuario: {username}");

                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var datosVehiculo = await unitOfWork.DatosCargainicialService
                        .ObtenerDatosVehiculoAsync(username, placa);

                    datosVehiculo.FechaActual = DateTime.Now;

                    Console.WriteLine($"[DEBUG] Datos obtenidos del vehículo {placa}");

                    // Enviar datos al grupo específico username_placa
                    var groupKey = $"{username}_{placa}";
                    await _hubContext.Clients.Group(groupKey).SendAsync("ActualizarDatosVehiculo", datosVehiculo);

                    Console.WriteLine($"[DEBUG] Datos enviados exitosamente para vehículo: {placa}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error enviando datos del vehículo {placa}: {ex.Message}");

                    // Si hay error, detener el timer para evitar spam de errores
                    if (ex.Message.Contains("disposed") || ex.Message.Contains("ObjectDisposed"))
                    {
                        Console.WriteLine($"[WARNING] Deteniendo timer para vehículo {placa} debido a disposed objects");
                        DetenerTimerVehiculo(username, placa);
                    }
                }
            }
        }
    }
}