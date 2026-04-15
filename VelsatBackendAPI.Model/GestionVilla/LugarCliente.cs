using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.GestionVilla
{
    public class LugarCliente
    {
        public int? Codlugar {  get; set; }
        public string? Codcli {  get; set; }
        public string? Direccion { get; set; }
        public string? Distrito { get; set; }
        public string? Wy {  get; set; }
        public string? Wx { get; set; }
        public char? Estado { get; set; }
        public string? Codcliente { get; set; }
        public string? Referencia { get;set; }
        public string? Zona { get; set; }
    }
}
