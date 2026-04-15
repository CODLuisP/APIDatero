using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Datero
{
    public class UrbanoAsignaRaw
    {
        public int Codigo { get; set; }

        public string DeviceID {  get; set; }

        public int Fecreg { get; set; }

        public int Fechaini { get; set; }

        public int? Fechafin { get; set; }

        public string? Codconductor { get; set; }

        public string Codruta {  get; set; }

        public string Isruta {  get; set; }
    }
}
