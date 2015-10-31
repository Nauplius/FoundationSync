using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauplius.SP.UserSync
{
    class SqlQueries
    {
        private readonly string _iCmd = "INSERT INTO";
        private readonly string _uCmd = "";

        /*Properties:
         *              "displayName",
                        "mail",
                        "title",
                        "mobile",
                        "proxyAddresses",
                        "department",
                        "sn",
                        "givenName",
                        "telephoneNumber",
                        "wWWHomePage",
                        "physicalDeliveryOfficeName",
                        "thumbnailPhoto"
         */

        public void InsertUser(SearchResultCollection results)
        {
            using (var cs = GetConnectionString())
            {
                using (var cmd = new SqlCommand(_iCmd, cs))
                {
                    try
                    {
                        cs.Open();
                        
                    }
                }
            }

        }

        public void UpdateUser(SearchResultCollection results)
        {
            
        }

        public void RemoveUser()
        {
            //periodically sweep the database
        }

        internal SqlConnection GetConnectionString()
        {
            return new SqlConnection(FoundationSyncSettings.Local.ConnectionString);
        }
    }
}
