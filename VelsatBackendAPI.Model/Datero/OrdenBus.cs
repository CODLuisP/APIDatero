using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Datero
{
    public class OrdenBus
    {
        public int Feciniruta { get; set; }
        public double LastValidLatitude { get; set; }
        public double LastValidLongitude { get; set; }
        public double LastValidSpeed {  get; set; }
        public string DeviceID { get; set; }
        public string Ultimocontrol { get; set; }
        public string Direccion {  get; set; }
        public int RowNum { get; set; }

    }
}
