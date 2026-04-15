using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<Account>> GetAllUsers(); //Tipo de retorno, Devuelve una tarea. IEnumerable, hace referencia a que trae todos los datos

        Task<Account> GetDetails(int id);

        Task<bool> InsertUser(Account account);

        Task<bool> UpdateUser(Account account);

        Task<bool> DeleteUser(Account account);

        Task<Account> ValidarUser(string accountID, string password);
    }
}
