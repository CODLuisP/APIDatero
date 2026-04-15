using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class SimplifiedDevice
    {
        public string DeviceId { get; set; }
        public string rutaact { get; set; }
        public double lastValidSpeed {get; set; }
        public double lastValidLatitude { get; set; }
        public double lastValidLongitude { get; set; }
    }
}
