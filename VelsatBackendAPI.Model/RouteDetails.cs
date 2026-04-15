using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class RouteDetails
    {
        public string Date { get; set; }

        public string Time{ get; set; }

        public double Speed { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }
}
