using System.Data;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IDbConnectionFactory
    {
        // Método original para compatibilidad con otros repositorios
        IDbConnection CreateConnection();

        // Nuevos métodos para manejar múltiples conexiones
        IDbConnection GetDefaultConnection();
        IDbConnection GetSecondConnection();
        IDbConnection GetGtsConnection();

    }
}