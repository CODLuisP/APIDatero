using Microsoft.AspNetCore.SignalR;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Hubs
{
    public class ActualizacionTiempoReal : Hub
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHubContext<ActualizacionTiempoReal> _hubContext;
        private static readonly Dictionary<string, Timer> _userTimers = new Dictionary<string, Timer>();
        private static readonly Dictionary<string, bool> _userExecuting = new Dictionary<string, bool>();
        private static readonly object _lockObject = new object();

        public ActualizacionTiempoReal(IServiceScopeFactory serviceScopeFactory, IHubContext<ActualizacionTiempoReal> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error dejando el grupo: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var username = GetUsernameFromRoute();

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

        private void IniciarTimer(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;

            lock (_lockObject)
            {
                if (_userTimers.ContainsKey(username))
                {
                    _userTimers[username].Dispose();
                    _userTimers.Remove(username);
                }
                _userExecuting.Remove(username);

                var timer = new Timer(_ =>
                {
                    lock (_lockObject)
                    {
                        if (_userExecuting.GetValueOrDefault(username)) return;
                        _userExecuting[username] = true;
                    }
                    EnviarDatosDirectamente(username).ContinueWith(t =>
                    {
                        lock (_lockObject) { _userExecuting[username] = false; }
                        if (t.IsFaulted)
                            Console.WriteLine($"[ERROR] Timer usuario {username}: {t.Exception?.GetBaseException().Message}");
                    });
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));

                _userTimers[username] = timer;
            }
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
                }
                _userExecuting.Remove(username);
            }
        }

        private async Task EnviarDatosDirectamente(string username)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            try
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var datosCargaActualizados = await unitOfWork.DatosCargainicialService.ObtenerDatosCargaInicialAsync(username);
                datosCargaActualizados.FechaActual = DateTime.Now;

                await _hubContext.Clients.Group(username).SendAsync("ActualizarDatos", datosCargaActualizados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error enviando datos para {username}: {ex.Message}");
                DetenerTimer(username);
            }
        }
    }
}
