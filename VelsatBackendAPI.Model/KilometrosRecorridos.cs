using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class KilometrosRecorridos
    {
        public int Item { get; set; }

        public string DeviceId {  get; set; }

        public double Maximo { get; set; }

        public double Minimo { get; set; }

        public double Kilometros { get; set; }
    }
}
