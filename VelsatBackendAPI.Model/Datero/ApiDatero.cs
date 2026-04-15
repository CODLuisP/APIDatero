using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Datero
{
    public class ApiDatero
    {
        [Required(ErrorMessage = "El campo 'accountID' es obligatorio.")]
        public string accountID { get; set; }

        [Required(ErrorMessage = "El campo 'deviceID' es obligatorio.")]
        public string deviceID { get; set; }

        [Required(ErrorMessage = "El campo 'timestamp' es obligatorio.")]
        public int timestamp { get; set; }

        [Required(ErrorMessage = "El campo 'statusCode' es obligatorio.")]
        public int statusCode { get; set; }

        [Required(ErrorMessage = "El campo 'latitude' es obligatorio.")]
        public double latitude { get; set; }

        [Required(ErrorMessage = "El campo 'longitude' es obligatorio.")]
        public double longitude { get; set; }

        [Required(ErrorMessage = "El campo 'speedKPH' es obligatorio.")]
        public double speedKPH { get; set; }

        [Required(ErrorMessage = "El campo 'heading' es obligatorio.")]
        public double heading { get; set; }

        [Required(ErrorMessage = "El campo 'address' es obligatorio.")]
        public string address { get; set; }

    }
}
