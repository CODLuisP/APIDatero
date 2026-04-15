using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDateroRepository _dateroRepository;
        private readonly ICajaRepository _cajaRepository;
        private readonly IHistoricosRepository _historicosRepository;
        private readonly IDatosCargainicialService _datosCargainicialService;
        private readonly IUserRepository _userRepository;
        private readonly IKilometrosRepository _kilometrosRepository;
        private readonly IServidorRepository _servidorRepository;
        private readonly IUrbanoAsignaService _urbanoAsignaService; // AGREGAR
        private readonly IAdminRepository _adminRepository; // AGREGAR

        // Inyectar los repositories directamente (ya configurados con DI)
        public UnitOfWork(IDateroRepository dateroRepository, ICajaRepository cajaRepository, IHistoricosRepository historicosRepository, IDatosCargainicialService datosCargainicialService, IUserRepository userRepository, IKilometrosRepository kilometrosRepository, IServidorRepository servidorRepository, IUrbanoAsignaService urbanoAsignaService, IAdminRepository adminRepository)
        {
            _dateroRepository = dateroRepository ?? throw new ArgumentNullException(nameof(dateroRepository));
            _cajaRepository = cajaRepository ?? throw new ArgumentNullException(nameof(cajaRepository));
            _historicosRepository = historicosRepository ?? throw new ArgumentNullException(nameof(historicosRepository));
            _datosCargainicialService = datosCargainicialService ?? throw new ArgumentNullException(nameof(datosCargainicialService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _kilometrosRepository = kilometrosRepository ?? throw new ArgumentNullException(nameof(kilometrosRepository));
            _servidorRepository = servidorRepository;
            _urbanoAsignaService = urbanoAsignaService ?? throw new ArgumentNullException(nameof(urbanoAsignaService));
            _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
        }

        public IDateroRepository DateroRepository => _dateroRepository;
        public ICajaRepository CajaRepository => _cajaRepository;
        public IHistoricosRepository HistoricosRepository => _historicosRepository;
        public IDatosCargainicialService DatosCargainicialService => _datosCargainicialService;
        public IUserRepository UserRepository => _userRepository;
        public IKilometrosRepository KilometrosRepository => _kilometrosRepository;
        public IServidorRepository ServidorRepository => _servidorRepository;
        public IUrbanoAsignaService UrbanoAsignaService => _urbanoAsignaService; // AGREGAR
        public IAdminRepository AdminRepository => _adminRepository; // AGREGAR


        // Opcional: Implementar IDisposable si necesitas cleanup
        public void Dispose()
        {
            // Cleanup si es necesario
        }
    }
}