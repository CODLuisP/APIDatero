using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IHistoricosRepository
    {
        Task<DatosReporting> GetDataReporting(string fechaini, string fechafin, string deviceID, string accountID);

        Task<List<StopsReporting>> GetStopData (string fechaini, string fechafin, string deviceID, string accountID);

        Task<List<RouteDetails>> GetRouteDetails(string fechaini, string fechafin, string deviceID, string accountID);

        Task<List<SpeedReporting>> GetSpeedData(string fechaini, string fechafin, string deviceID, double speedKPH, string accountID);

        string UserName(string deviceID);

    }
}
