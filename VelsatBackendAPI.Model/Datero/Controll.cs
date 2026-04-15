using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDatero.Model.Datero
{
    public class Controll
    {
        public string? Codasig { get; set; }

        public string? Deviceid { get; set; }

        public string? Codconductor { get; set; }

        public string? Codruta { get; set; }

        public string? Nom_control { get; set; }

        public string? Hora_registro { get; set; }

        public string? Hora_inicio { get; set; }

        public string? Hora_estimada { get; set; }

        public string? Hora_llegada { get; set; }

        public string? Volado { get; set; }

        public string? Fecha { get; set; }

        public int? IsGPS { get; set; }

        // Campo adicional para el nombre del conductor
        public string? NombreConductor { get; set; }
    }
}
