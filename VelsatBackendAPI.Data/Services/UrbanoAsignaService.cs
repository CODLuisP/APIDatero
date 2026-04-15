using APIDatero.Model.Datero;
using Dapper;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Services
{
    public interface IUrbanoAsignaService
    {
        Task<RecentUrbano> GetUrbanoAsigna(string placa);
    }

    public class UrbanoAsignaService : IUrbanoAsignaService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UrbanoAsignaService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            Console.WriteLine("[DEBUG] UrbanoAsignaService inicializado con IDbConnectionFactory");
        }

        public async Task<RecentUrbano> GetUrbanoAsigna(string placa)
        {
            const string consulta = @"SELECT deviceID, fechaini, isruta, codruta FROM recent_urbano WHERE deviceID = @Placa";

            using var connection = _connectionFactory.GetDefaultConnection();
            var raw = await connection.QueryFirstOrDefaultAsync<UrbanoAsignaRaw>(consulta, new { Placa = placa });
            if (raw == null) return null;

            if (raw.Isruta == "0")
            {
                return new RecentUrbano
                {
                    DeviceID = null,
                    Fechaini = null,
                    Isruta = "0",
                    Codruta = "0",
                };
            }

            return new RecentUrbano
            {
                DeviceID = raw.DeviceID,
                Fechaini = UnixToHHmm(raw.Fechaini),
                Isruta = raw.Isruta,
                Codruta = raw.Codruta
            };
        }

        private string UnixToHHmm(int unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().ToString("HH:mm");
        }
    }

    // Modelo raw para mapear la consulta
    public class UrbanoAsignaRaw
    {
        public string DeviceID { get; set; } = string.Empty;
        public int Fechaini { get; set; }
        public string Isruta { get; set; } = string.Empty;
        public string Codruta { get; set; }
    }
}