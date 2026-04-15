using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDatero.Model.Documentacion
{
    public class Docunidad
    {
        public int Id { get; set; }
        public string DeviceID { get; set; }
        public string Tipo_documento { get; set; }
        public string? Archivo_url { get; set; }
        public DateTime? Fecha_vencimiento { get; set; }
        public string? Observaciones { get; set; }

        // Estado calculado - NO se mapea desde la BD
        public string Estado
        {
            get
            {
                if (Fecha_vencimiento == null)
                    return "Sin Fecha";

                var fechaActual = DateTime.UtcNow.AddHours(-5).Date; // Hora peruana
                var diasRestantes = (Fecha_vencimiento.Value.Date - fechaActual).Days;

                if (diasRestantes < 0)
                    return "Vencido";
                else if (diasRestantes <= 15)
                    return "Por Vencer";
                else if (diasRestantes <= 30)
                    return "Próximo a Vencer";
                else
                    return "Vigente";
            }
        }

        public string? Usuario { get; set; }
    }
}
