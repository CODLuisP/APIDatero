using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace APIDatero.Model
{
    public class DatosVehiculo
    {
        public DateTime FechaActual { get; set; }
        public DateTime? FechaGPS { get; set; }          // ✅ NUEVO - hora real del GPS
        public Device Vehiculo { get; set; }
    }   
}
