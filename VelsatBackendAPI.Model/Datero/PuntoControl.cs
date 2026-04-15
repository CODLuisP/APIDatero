using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDatero.Model.Datero
{
    public class PuntoControl
    {
        public string? Nom_control { get; set; }
        public string? Hora_estimada { get; set; }
        public string? Hora_llegada { get; set; }
        public string? Volado { get; set; }
        public string? Fecha { get; set; }
        public int? IsGPS { get; set; }
    }
}
