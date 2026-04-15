using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class SpeedReporting
    {
        public int Item { get; set; }
        public double SpeedKPH { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }
}
