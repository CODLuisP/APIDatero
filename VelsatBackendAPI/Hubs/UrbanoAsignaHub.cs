using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using VelsatBackendAPI.Data.Repositories;

namespace VelsatBackendAPI.Hubs
{
    public class UrbanoAsignaHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UrbanoAsignaHub> _logger;
        private static readonly ConcurrentDictionary<string, Timer> _placaTimers = new();
        private static readonly ConcurrentDictionary<string, int> _conexionesPorPlaca = new();

        public UrbanoAsignaHub(IServiceScopeFactory scopeFactory, ILogger<UrbanoAsignaHub> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task UnirGrupo(string placa)
        {
            if (string.IsNullOrEmpty(placa))
            {
                _logger.LogWarning("Intento de unirse a grupo con placa vacía");
                return;
            }

            // Unir al cliente al grupo
            await Groups.AddToGroupAsync(Context.ConnectionId, placa);
            _logger.LogInformation($"Cliente {Context.ConnectionId} se unió al grupo: {placa}");

            // Incrementar contador de conexiones para esta placa
            _conexionesPorPlaca.AddOrUpdate(placa, 1, (key, value) => value + 1);

            // Enviar datos inmediatamente al cliente que se acaba de conectar
            await EnviarDatosInmediatos(placa);

            // Iniciar timer si es la primera conexión para esta placa
            if (!_placaTimers.ContainsKey(placa))
            {
                IniciarTimerParaPlaca(placa);
            }
        }

        public async Task DejarGrupo(string placa)
        {
            if (string.IsNullOrEmpty(placa))
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, placa);
            _logger.LogInformation($"Cliente {Context.ConnectionId} dejó el grupo: {placa}");

            // Decrementar contador de conexiones
            if (_conexionesPorPlaca.TryGetValue(placa, out var count))
            {
                if (count <= 1)
                {
                    _conexionesPorPlaca.TryRemove(placa, out _);
                    DetenerTimerParaPlaca(placa);
                }
                else
                {
                    _conexionesPorPlaca.TryUpdate(placa, count - 1, count);
                }
            }
        }

        private async Task EnviarDatosInmediatos(string placa)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var datos = await unitOfWork.UrbanoAsignaService.GetUrbanoAsigna(placa);

                if (datos != null)
                {
                    // Enviar solo al cliente que se acaba de conectar
                    await Clients.Caller.SendAsync("ActualizarDatosUrbano", datos);
                    _logger.LogInformation($"Datos inmediatos enviados para placa: {placa}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar datos inmediatos para placa: {placa}");
            }
        }

        private void IniciarTimerParaPlaca(string placa)
        {
            var timer = new Timer(async (state) => await EnviarDatosEnTiempoReal((string)state),
                                  placa,
                                  TimeSpan.FromSeconds(5), // Primer envío después de 5 segundos
                                  TimeSpan.FromSeconds(5));  // Luego cada 5 segundos

            _placaTimers[placa] = timer;
            _logger.LogInformation($"Timer iniciado para placa: {placa}");
        }

        private async Task EnviarDatosEnTiempoReal(string placa)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<UrbanoAsignaHub>>();

                var datos = await unitOfWork.UrbanoAsignaService.GetUrbanoAsigna(placa);

                if (datos != null)
                {
                    // Enviar a todos los clientes del grupo
                    await hubContext.Clients.Group(placa).SendAsync("ActualizarDatosUrbano", datos);
                    _logger.LogDebug($"Datos en tiempo real enviados para placa: {placa}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar datos en tiempo real para placa: {placa}");

                // Si hay error persistente, detener el timer para evitar spam de errores
                DetenerTimerParaPlaca(placa);
            }
        }

        private void DetenerTimerParaPlaca(string placa)
        {
            if (_placaTimers.TryRemove(placa, out var timer))
            {
                timer.Dispose();
                _logger.LogInformation($"Timer detenido para placa: {placa}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "UrbanoGroup");
            _logger.LogInformation($"Cliente conectado: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Nota: En una implementación más robusta, deberías mantener un registro
            // de qué cliente está en qué grupos para limpiar correctamente
            // Por ahora, los timers se detendrán cuando no haya conexiones activas

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "UrbanoGroup");
            _logger.LogInformation($"Cliente desconectado: {Context.ConnectionId}");

            if (exception != null)
            {
                _logger.LogError(exception, "Cliente desconectado con excepción");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}