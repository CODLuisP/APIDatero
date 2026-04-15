using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Administracion
{
    public class Usuarioadmin
    {
        public string AccountID { get; set; }
        public string Password { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Description { get; set; }
        public int? CreationTime { get; set; }
        public bool? IsActive { get; set; }
        public string? Ruc { get; set; }
    }
}