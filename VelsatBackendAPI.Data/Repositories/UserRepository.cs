using Dapper;
using MySql.Data.MySqlClient;
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
            Console.WriteLine("[DEBUG] UserRepository inicializado con IDbConnectionFactory");
        }

        private IDbConnection CreateConnection() => _connectionFactory.GetDefaultConnection();

        public async Task<IEnumerable<Account>> GetAllUsers() //Task es siempre asíncrono
        {
            Console.WriteLine("[DEBUG] Iniciando GetAllUsers");

            try
            {
                using var connection = CreateConnection();
                Console.WriteLine("[DEBUG] Conexión creada para GetAllUsers");

                var result = await connection.QueryAsync<Account>("Select accountID, password, description from usuarios", new { });
                var users = result.ToList();

                Console.WriteLine($"[DEBUG] GetAllUsers completado: {users.Count} usuarios encontrados");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en GetAllUsers: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Account> GetDetails(int id)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetDetails para ID: {id}");

            try
            {
                using var connection = CreateConnection();
                Console.WriteLine("[DEBUG] Conexión creada para GetDetails");

                var result = await connection.QueryFirstOrDefaultAsync<Account>(
                    "Select accountID, password, description from user where accountID = @id",
                    new { id });

                if (result != null)
                {
                    Console.WriteLine($"[DEBUG] GetDetails completado: Usuario encontrado para ID {id}");
                }
                else
                {
                    Console.WriteLine($"[WARNING] GetDetails: No se encontró usuario para ID {id}");
                }

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
            Console.WriteLine("[DEBUG] InsertUser llamado - No implementado");
            throw new NotImplementedException("InsertUser no está implementado");
        }

        public Task<bool> UpdateUser(Account account)
        {
            Console.WriteLine("[DEBUG] UpdateUser llamado - No implementado");
            throw new NotImplementedException("UpdateUser no está implementado");
        }

        public Task<bool> DeleteUser(Account account)
        {
            Console.WriteLine("[DEBUG] DeleteUser llamado - No implementado");
            throw new NotImplementedException("DeleteUser no está implementado");
        }

        public async Task<Account> ValidarUser(string login, string clave)
        {
            Console.WriteLine($"[DEBUG] Iniciando ValidarUser para login: {login}");

            try
            {
                using var connection = CreateConnection();
                Console.WriteLine("[DEBUG] Conexión creada para ValidarUser");

                const string sql = "Select accountID, password from usuarios where accountID = @login and password = @clave";
                var result = await connection.QueryFirstOrDefaultAsync<Account>(sql, new { login = login, clave = clave });

                if (result != null)
                {
                    Console.WriteLine($"[DEBUG] ValidarUser exitoso: Usuario {login} validado correctamente");
                }
                else
                {
                    Console.WriteLine($"[WARNING] ValidarUser fallido: Credenciales incorrectas para {login}");
                }

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