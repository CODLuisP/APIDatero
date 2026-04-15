using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class Geocercausu
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public double Latitud { get; set;}
        public double Longitud { get; set;}
        public string Radio { get; set;}
    }
}
