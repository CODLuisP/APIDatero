using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.GestionVilla
{
    public class Conductor
    {
        public int Codigo { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? Login { get; set; }
        public string? Clave { get; set; }
        public string? Telefono { get; set; }
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? Brevete { get; set; }
        public string? Sctr { get; set; }
        public string? Direccion { get; set; }
        public string? Imagen { get; set; }
        public string? CatBrevete { get; set; }
        public string? FecValidBrevete { get; set; }
        public string? EstBrevete { get; set; }
        public string? Sexo { get; set; }
        public string? UnidadActual { get; set; }
        public string? Habilitado { get; set; }
    }
}
