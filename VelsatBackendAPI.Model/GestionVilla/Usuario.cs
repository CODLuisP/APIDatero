using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.GestionVilla;

namespace VelsatBackendAPI.Model.GestionVilla
{
    public class Usuario
    {
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Codlan { get; set; }
        public string? Apepate { get; set; }
        public string? Login { get; set; }
        public string? Clave { get; set; }
        public string? Sexo { get; set; }
        public string? Dni { get; set; }
        public string? Telefono { get; set; }
        public string? Empresa { get; set; }
        public LugarCliente? Lugar { get; set; }
    }
}
