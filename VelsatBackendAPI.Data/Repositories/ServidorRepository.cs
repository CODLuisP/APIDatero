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
        }

        private IDbConnection CreateConnection() => _connectionFactory.GetDefaultConnection();

        public async Task<Servidor> GetServidor(string accountID)
        {

            try
            {
                using var connection = CreateConnection();

                const string sql = "Select servidor from serverprueba where loginusu = @accountID";
                var result = await connection.QueryFirstOrDefaultAsync<Servidor>(sql, new { accountID = accountID });

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}