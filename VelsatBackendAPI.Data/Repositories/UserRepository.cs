using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private IDbConnection CreateConnection() => _connectionFactory.GetDefaultConnection();

        public async Task<IEnumerable<Account>> GetAllUsers() //Task es siempre asíncrono
        {

            try
            {
                using var connection = CreateConnection();

                var result = await connection.QueryAsync<Account>("Select accountID, password, description from usuarios", new { });
                var users = result.ToList();

                return users;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Account> GetDetails(int id)
        {
            try
            {
                using var connection = CreateConnection();

                var result = await connection.QueryFirstOrDefaultAsync<Account>(
                    "Select accountID, password, description from user where accountID = @id",
                    new { id });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en GetDetails para ID {id}: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public Task<bool> InsertUser(Account account)
        {
            throw new NotImplementedException("InsertUser no está implementado");
        }

        public Task<bool> UpdateUser(Account account)
        {
            throw new NotImplementedException("UpdateUser no está implementado");
        }

        public Task<bool> DeleteUser(Account account)
        {
            throw new NotImplementedException("DeleteUser no está implementado");
        }

        public async Task<Account> ValidarUser(string login, string clave)
        {
            try
            {
                using var connection = CreateConnection();

                const string sql = "Select accountID, password, description, emailTraccar, passTraccar from usuarios where accountID = @login and password = @clave";
                var result = await connection.QueryFirstOrDefaultAsync<Account>(sql, new { login = login, clave = clave });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en ValidarUser para login {login}: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}