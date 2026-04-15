using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class DatosCargainicial
    {
        public DateTime FechaActual { get; set; }
        public List<Device> DatosDevice { get; set; } = new List<Device>();
    }
}
