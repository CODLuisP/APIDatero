using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class StopsReporting
    {
        public int Item { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public string TotalTime { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Address { get; set; }

        [JsonIgnore]
        public int TimeStampIni { get; set; }

        [JsonIgnore]
        public int TimeStampEnd { get; set; }
    }
}
