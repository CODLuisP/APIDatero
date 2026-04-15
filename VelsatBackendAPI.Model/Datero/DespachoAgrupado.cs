using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDatero.Model.Datero
{
    public class DespachoAgrupado
    {
        public string? Codasig { get; set; }
        public string? Deviceid { get; set; }
        public string? Codconductor { get; set; }
        public string? Codruta { get; set; }
        public string? NombreConductor { get; set; }
        public string? Hora_registro { get; set; }
        public string? Hora_inicio { get; set; }

        public List<PuntoControl> Controles { get; set; } = new();
    }
}
