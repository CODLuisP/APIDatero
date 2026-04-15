using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IUnitOfWork
    {
        IDateroRepository DateroRepository { get; }

        ICajaRepository CajaRepository { get; }

        IHistoricosRepository HistoricosRepository { get; }

        IDatosCargainicialService DatosCargainicialService { get; }

        IUserRepository UserRepository { get; }

        IKilometrosRepository KilometrosRepository { get; }

        IServidorRepository ServidorRepository { get; }

        IUrbanoAsignaService UrbanoAsignaService { get; }

        IAdminRepository AdminRepository { get; }

    }
}
