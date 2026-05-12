using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace VelsatBackendAPI.Data.Repositories
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _defaultConnectionString;
        private readonly string _secondConnectionString;
        private readonly string _gtsConnectionString;


        public DbConnectionFactory(IConfiguration configuration)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");

            _secondConnectionString = configuration.GetConnectionString("SecondConnection")
                ?? _defaultConnectionString; // Si no existe SecondConnection, usa la misma que Default

            _gtsConnectionString = configuration.GetConnectionString("GtsConnection")
                ?? throw new ArgumentNullException("Connection string 'GtsConnection' not found.");

        }

        public IDbConnection CreateConnection()
        {
            try
            {
                var connection = new MySqlConnection(_defaultConnectionString);
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error creando conexión default: {ex.Message}");
                throw;
            }
        }

        public IDbConnection GetDefaultConnection()
        {
            try
            {
                var connection = new MySqlConnection(_defaultConnectionString);
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error creando conexión default: {ex.Message}");
                throw;
            }
        }

        public IDbConnection GetSecondConnection()
        {
            try
            {
                var connection = new MySqlConnection(_secondConnectionString);
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error creando conexión second: {ex.Message}");
                throw;
            }
        }

        public IDbConnection GetGtsConnection()
        {
            try
            {
                var connection = new MySqlConnection(_gtsConnectionString);
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error creando conexión GTS: {ex.Message}");
                throw;
            }
        }
    }
}