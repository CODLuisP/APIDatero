using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class TablasReporting
    {
        [JsonIgnore]
        public string DeviceId { get; set; }

        [JsonIgnore]
        public int Timestamp { get; set; }

        [JsonIgnore]
        public DateTime TimestampConvert => DateTimeOffset.FromUnixTimeSeconds(Timestamp).DateTime;

        public int Item { get; set; }

        public string Fecha => TimeZoneInfo.ConvertTimeFromUtc(TimestampConvert, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time")).ToString("dd/MM/yyyy");
        public string Hora => TimeZoneInfo.ConvertTimeFromUtc(TimestampConvert, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time")).ToString("HH:mm:ss").ToUpper();

        public double SpeedKPH { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double OdometerKM { get; set; }
        public string Address { get; set; }
    }
}