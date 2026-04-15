using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class KilometrosReporting
    {
        public List<KilometrosRecorridos> ListaKilometros { get; set; } = new List<KilometrosRecorridos>();

        public string Mensaje { get; set; }
    }
}
