using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public class ServidorRepository : IServidorRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ServidorRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            Console.WriteLine("[DEBUG] ServidorRepository inicializado con IDbConnectionFactory");
        }

        private IDbConnection CreateConnection() => _connectionFactory.GetDefaultConnection();

        public async Task<Servidor> GetServidor(string accountID)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetServidor para accountID: {accountID}");

            try
            {
                using var connection = CreateConnection();
                Console.WriteLine("[DEBUG] Conexión creada para GetServidor");

                const string sql = "Select servidor from serverprueba where loginusu = @accountID";
                var result = await connection.QueryFirstOrDefaultAsync<Servidor>(sql, new { accountID = accountID });

                if (result != null)
                {
                    Console.WriteLine($"[DEBUG] GetServidor completado: Servidor encontrado para accountID {accountID}");
                }
                else
                {
                    Console.WriteLine($"[WARNING] GetServidor: No se encontró servidor para accountID {accountID}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en GetServidor para accountID {accountID}: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}