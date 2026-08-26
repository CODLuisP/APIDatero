using System;

namespace VelsatBackendAPI.Model.Administracion
{
    public class AuditoriaGeneral
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Entidad { get; set; }
        public string Detalle { get; set; }
        public DateTime Fecharegistro { get; set; }
    }
}
