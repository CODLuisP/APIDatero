using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Data
{
    public class MySqlConfiguration
    {
        public MySqlConfiguration(string defaultConnection, string secondConnection, string gtsConnectionString)
        {
            DefaultConnection = defaultConnection;
            SecondConnection = secondConnection;
            GtsConnectionString = gtsConnectionString;
        }

        public string DefaultConnection { get; set; }
        public string SecondConnection { get; set; }
        public string GtsConnectionString { get; set; }

    }
}
