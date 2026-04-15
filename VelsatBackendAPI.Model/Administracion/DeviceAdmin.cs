using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Administracion
{
    public class DeviceAdmin
    {
        public string DeviceID { get; set; }
        public string AccountID { get; set; }
        public string EquipmentType { get; set; }
        public string UniqueID { get; set; }
        public string DeviceCode { get; set; }
        public string SimPhoneNumber { get; set; }
        public string ImeiNumber { get; set; }
        public string? IsActive { get; set; }
    }
}
