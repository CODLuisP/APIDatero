using APIDatero.Model.Documentacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.Caja;
using VelsatBackendAPI.Model.GestionVilla;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface ICajaRepository
    {
        //MÉTODOS PARA GESTIÓN DE ACTIVOS
        Task<List<Usuario>> GetConductores(string codusuario);

        Task<List<Carro>> GetUnidades(string username);

        //FIN CAJA


        //MÉTODOS GESTION VILLA
        Task<List<RutaUrbano>> ListaRutas(string usuario);

        Task<List<ConductorDisp>> ListaConducDisp(string usuario);

        Task<List<Carro>> ListUnidDisp(string usuario);

        Task<Usuario> GetDetalleConductor(string codtaxi);

        Task<List<DespachoVilla>> ListDespachoIniciado(string codruta);

        Task<string> AsignarDespacho(DespachoVilla despacho);

        Task<string> EliminarDespacho(DespachoVilla despacho);

        Task<List<Conductor>> GetConductoresxUsuario(string usuario);

        Task<List<Carro>> GetUnidadesxUsuario(string usuario);

        Task<List<Conductor>> GetCobradoresxUsuario(string usuario);

        Task<List<Carro>> ReporteVueltas(string fecha, string ruta);

        Task<int> NuevoConductorAsync(Conductor conductor, string usuario);

        Task<int> GuardarCobradorAsync(Conductor cobrador, string usuario);

        Task<int> ModificarConductorAsync(Conductor conductor);

        Task<int> ModificarCobradorAsync(Conductor cobrador);

        Task<int> HabilitarConductorAsync(int codigoConductor);

        Task<int> HabilitarCobradorAsync(int codigoCobrador);

        Task<int> DeshabilitarConductorAsync(int codigoConductor);

        Task<int> DeshabilitarCobradorAsync(int codigoCobrador);

        Task<int> LiberarConductorAsync(int codigoConductor);

        Task<int> EliminarConductorAsync(int codigoConductor);

        Task<int> EliminarCobradorAsync(int codigoCobrador);

        Task<int> HabilitarUnidadAsync(string placa);

        Task<int> DeshabilitarUnidadAsync(string placa);

        Task<int> LiberarUnidadAsync(string placa);

        Task<int> LiberarTotalUnidadesAsync(string username);

        //APIS PARA CONTROL GPS
        Task<string> EnviarHoraSolicitud(string codruta);

        Task<string> GetHoraSolicitud(string codruta);

        Task<string> ObtenerUltimoCodasig(string placa);


        //DOCUMENTACIÓN
        //----------------------------------UNIDAD--------------------------------------------------//
        Task<List<Docunidad>> GetByDeviceID(string deviceID);
        Task<int> Create(Docunidad docunidad);
        Task<bool> DeleteUnidad(int id);
        Task<List<Docunidad>> GetDocumentosUnidadProximosVencer(string usuario);


        //----------------------------------CONDUCTOR--------------------------------------------------//
        Task<List<Docconductor>> GetByCodtaxi(int codtaxi);
        Task<int> Create(Docconductor docconductor);
        Task<bool> DeleteConductor(int id);
        Task<List<Docconductor>> GetDocumentosConductorProximosVencer(string usuario);


        //----------------------------------COBRADOR--------------------------------------------------//
        Task<List<Docconductor>> GetByCobrador(int codtaxi);
        Task<int> CreateCobrador(Docconductor doccobrador);
        Task<bool> DeleteCobrador(int id);
        Task<List<Docconductor>> GetDocumentosCobradorProximosVencer(string usuario);
        Task<Usuario> GetDetalleCobrador(string codtaxi);

    }
}