using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.GestionVilla
{
    public class Gps
    {
        public string? Numreg { get; set; }
        public string? Numequipo { get; set; }
        public double? Posx { get; set; }
        public double? Posy { get; set; }
        public double? Velocidad { get; set; }
        public string? Io { get; set; }
        public string? Fecha { get; set; }
        public double? Rm { get; set; }
        public string? Evento { get; set; }
        public string? Direccion { get; set; }
        //public Ubicacion? Ubicacion { get; set; }
        public double? Distancia { get; set; }
        public double? Tiempo { get; set; }
        public string? Fecform { get; set; }
        public string? Horform { get; set; }
        public string? Nomevento { get; set; }
        public string? Placaalarma { get; set; }
        public string? Numubicaciones { get; set; }
        public double? Disorigen { get; set; }
        public double? Diferencia { get; set; }
        public string? Millas { get; set; }
        public string? Timedesconect { get; set; }
        public DateTime? Fecgt { get; set; }
        public double? Time { get; set; }
        public double? Millasmin { get; set; }
        public double? Millasmax { get; set; }
        public double? Velpromedio { get; set; }
        public double? Velmaxima { get; set; }
        //public Geocerca? Geocerca { get; set; }
        //public Geocerca? GeoSalida { get; set; }
        public string? Uniprox { get; set; }
        public string? Alarmacontrol { get; set; }
        public string? Origen { get; set; }
        public string? Destino { get; set; }
        public string? Rutaact { get; set; }
        public string? Feciniruta { get; set; }
        public string? Ultimocontrol { get; set; }
        public double? Cantidadreg { get; set; }
        public Usuario? Conductor { get; set; }
        //public Servicio? Ultservicio { get; set; }
        public int? Totalservicios { get; set; }
        //public Geocerca? GeocercaDesvio { get; set; }
        public string? Timerutaout { get; set; }
        public double? Odometercontrol { get; set; }
        public string? Codmantecontrol { get; set; }
        public string? Botonpanico { get; set; }
        public string? Timereg { get; set; }
        public string? Timegps { get; set; }
        public string? Lastdespacho { get; set; }
    }
}
