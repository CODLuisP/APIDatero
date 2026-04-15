using APIDatero.Model.Datero;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;
using VelsatBackendAPI.Model.Datero;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IDateroRepository
    {

        Task<UrbanoAsigna> GetUrbanoAsigna(string placa);

        Task<IEnumerable<LogUrbano>> GetLogUrbano(string codasig);

        Task<string> EnvioControl(LogUrbano log);

        Task<IEnumerable<UnidadesDatero>> GetDevices(string accountID);

        Task<IEnumerable<DeviceOrden>> GetDevicesOrden(string rutaact);

        Task<string> EndRuta(string deviceID);

        Task<string> AsignarID(string placa, string nuevoAndroidID);

        Task<IEnumerable<DespachoAgrupado>> ControlDespachoAgrupado(string fecha, string codruta, string username);

        Task<IEnumerable<DespachoAgrupado>> ControlDespachoAgrupadoEdu(string fecha, string codruta, string username);

        Task<bool> ActualizarRutaact(string placa);

        Task<bool> ActualizarRutaactG(string placa); 

        Task<bool> ActualizarRutaactFR(string placa);

        //APIS PARA REGISTROS DE GPS VEHICULAR
        Task<IEnumerable<DeviceDespachadas>> DevicesDespacho(string fecha, string ruta);

        Task<IEnumerable<DeviceDespachadas>> DevicesDespachoEdu(string fecha, string ruta);

        //Task<IEnumerable<DespachoAgrupado>> ControlDespachoGPSE(string fecha, string codruta);


        Task<string> EnvioControlGPS(ControlGPS[] logs, string user); 

        Task<string> EnvioControlB(ControlGPS[] logs, string user);
    }
}