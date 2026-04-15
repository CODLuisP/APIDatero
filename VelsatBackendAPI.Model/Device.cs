using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class Device
    {
        public string DeviceId { get; set; }
        public int lastGPSTimestamp {  get; set; }
        public double lastValidLatitude { get; set; }
        public double lastValidLongitude { get; set; }
        public double lastValidHeading { get; set; }      
        public double lastValidSpeed { get; set; }
        public string Descripcion { get; set; }
        public string Direccion { get; set; }
        public string Codgeoact { get; set; }
        public string rutaact { get; set; }
        public Geocercausu DatosGeocercausu { get; set; }

    }
}
