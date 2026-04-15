using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class DatosReporting
    {
        public List<TablasReporting> ListaTablas { get; set; } = new List<TablasReporting>();//No tenemos certeza en qué tabla estará

        public string Mensaje { get; set; }

        public int TotalPages { get; set; }
    }
}
