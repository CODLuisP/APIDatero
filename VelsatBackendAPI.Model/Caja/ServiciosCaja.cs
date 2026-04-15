using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Caja
{
    public class ServiciosCaja
    {
        public int CodServicio { get; set; }
        public string Concepto { get; set; }
        public double Monto { get; set; }
    }
}
