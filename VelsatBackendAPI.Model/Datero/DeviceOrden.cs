using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Datero
{
    public class DeviceOrden
    {
        public string DeviceID {  get; set; }
        public double LastValidLatitude { get; set; }
        public double LastValidLongitude { get; set; }
        public double LastValidSpeed { get; set; }
        public string Direccion { get; set; }
    }
}
