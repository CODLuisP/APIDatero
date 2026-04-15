using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Administracion
{
    public class ConexDevice
    {
        public string DeviceID { get; set; }
        public string AccountID { get; set; }
        public double LastValidSpeed { get; set; }
        public int LastGPSTimestamp { get; set; }
        public string DeviceCode { get; set; }
        public string ImeiNumber { get; set; }
        public string LastValidLatitude { get; set; }
        public string LastValidLongitude { get; set; }
    }
}
