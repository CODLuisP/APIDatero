using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Caja
{
    public class Despachador
    {
        public int CodDespachador {  get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Dni {  get; set; }
        public char IsActivo { get; set; }
    }
}
