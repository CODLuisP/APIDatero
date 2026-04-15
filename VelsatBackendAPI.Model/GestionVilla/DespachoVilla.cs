using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model.GestionVilla;

namespace VelsatBackendAPI.Model.GestionVilla
{
    public class DespachoVilla
    {
        public string? Codigo { get; set; }
        public Carro? Carro { get; set; }
        public RutaUrbano? Ruta { get; set; }
        public string? Fecprog { get; set; }
        public string? Fecini { get; set; }
        public string? Fecfin { get; set; }
        public Usuario? Conductor { get; set; }
        public string? Ultimocontrol { get; set; }

        //public List<Control>? ListaControl { get; set; }

        public string? Motivoelim { get; set; }
        public Usuario? Cobrador { get; set; }
        public string? Fecelim { get; set; }
        public string? Estado { get; set; }

        //public List<AsignacionBoleto>? ListaBoletos { get; set; }

        public string? Boletos { get; set; }
        public string? Fecreg { get; set; }

    }
}
