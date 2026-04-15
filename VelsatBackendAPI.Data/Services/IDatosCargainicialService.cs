using APIDatero.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Services
{
    public interface IDatosCargainicialService
    {
        Task<DatosCargainicial> ObtenerDatosCargaInicialAsync(string login);
        Task<IEnumerable<SimplifiedDevice>> SimplifiedList(string login);

        Task<DatosVehiculo> ObtenerDatosVehiculoAsync(string login, string placa);

    }
}