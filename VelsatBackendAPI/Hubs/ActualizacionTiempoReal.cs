using Microsoft.AspNetCore.SignalR;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Hubs
{
    public class ActualizacionTiempoReal : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ActualizacionTiempoReal> _hubContext;
        private static readonly Dictionary<string, Timer> _userTimers = new Dictionary<string, Timer>();
        private static readonly object _lockObject = new object();

        public ActualizacionTiempoReal(IUnitOfWork unitOfWork, IHubContext<ActualizacionTiempoReal> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task UnirGrupo()
        {
            var username = GetUsernameFromRoute();

            if (string.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("Error", "Username no encontrado en la ruta");
                return;
            }

            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
                IniciarTimer(username);
                await Clients.Caller.SendAsync("ConectadoExitosamente", username);

                Console.WriteLine($"[DEBUG] Usuario {username} se unió al grupo desde la ruta");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error uniendo al grupo: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Error al unirse al grupo: {ex.Message}");
            }
        }

        public async Task DejarGrupo()
        {
            var username = GetUsernameFromRoute();

            if (string.IsNullOrEmpty(username))
                return;

            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
                DetenerTimer(username);

                Console.WriteLine($"[DEBUG] Usuario {username} dejó el grupo");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error dejando el grupo: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var username = GetUsernameFromRoute();
            Console.WriteLine($"[DEBUG] Cliente conectado: {Context.ConnectionId}, Username: {username}");

            if (!string.IsNullOrEmpty(username))
            {
                await UnirGrupo();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = GetUsernameFromRoute();

            if (!string.IsNullOrEmpty(username))
            {
                DetenerTimer(username);
                Console.WriteLine($"[DEBUG] Usuario {username} desconectado, timer detenido");
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

        // ============= MÉTODOS DEL TIMER SIMPLIFICADOS =============

        private void IniciarTimer(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;

            lock (_lockObject)
            {
                // Si ya existe un timer para este usuario, lo detenemos primero
                if (_userTimers.ContainsKey(username))
                {
                    _userTimers[username].Dispose();
                    _userTimers.Remove(username);
                }

                // Crear nuevo timer - USANDO EL CONTEXTO ACTUAL DEL HUB
                var timer = new Timer(async _ => await EnviarDatosDirectamente(username),
                                    null,
                                    TimeSpan.FromSeconds(1),
                                    TimeSpan.FromSeconds(5));

                _userTimers[username] = timer;
            }

            Console.WriteLine($"[DEBUG] Timer iniciado para: {username}");
        }

        private void DetenerTimer(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;

            lock (_lockObject)
            {
                if (_userTimers.ContainsKey(username))
                {
                    _userTimers[username].Dispose();
                    _userTimers.Remove(username);
                    Console.WriteLine($"[DEBUG] Timer detenido para: {username}");
                }
            }
        }

        // ✅ MÉTODO SIMPLE - Sin ServiceProvider, usando las dependencias inyectadas directamente
        private async Task EnviarDatosDirectamente(string username)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Obteniendo datos para: {username}");

                // Usar directamente las dependencias inyectadas en el constructor
                var datosCargaActualizados = await _unitOfWork.DatosCargainicialService.ObtenerDatosCargaInicialAsync(username);
                datosCargaActualizados.FechaActual = DateTime.Now;

                Console.WriteLine($"[DEBUG] Datos obtenidos para {username}: {datosCargaActualizados.DatosDevice?.Count ?? 0} dispositivos");

                // Enviar datos usando el HubContext inyectado
                await _hubContext.Clients.Group(username).SendAsync("ActualizarDatos", datosCargaActualizados);

                Console.WriteLine($"[DEBUG] Datos enviados exitosamente para: {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error enviando datos para {username}: {ex.Message}");

                // Si hay error, detener el timer para evitar spam de errores
                if (ex.Message.Contains("disposed") || ex.Message.Contains("ObjectDisposed"))
                {
                    Console.WriteLine($"[WARNING] Deteniendo timer para {username} debido a disposed objects");
                    DetenerTimer(username);
                }
            }
        }
    }
}