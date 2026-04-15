using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Caja
{
    public class RegistroTicket
    {
        public int Num { get; set; }
        public int IdTicket { get; set; } 

        public int Numero { get; set; }
        public string? Unidad { get; set; }
        public string? Fecha { get; set; }
        public string? Salida1 {  get; set; }
        public string? Salida2 { get; set; }
        public string? Salida3 { get; set; }
        public string? Salida4 { get; set; }
        public string? Conductor { get; set; }
        public int Codtaxi { get; set; }
        public string? Despachador { get; set; }
        public int CodDespachador { get; set; }
        public string? Usuario { get; set; }
        public int CodUsuario { get; set; }
        public List<DetalleTicket>? Pagos { get; set; }
    }
}
