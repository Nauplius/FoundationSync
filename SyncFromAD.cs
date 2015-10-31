using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class SyncFromAd : SPJobDefinition
    {
        private const string tJobName = "FoundationSync";

                public SyncFromAd()
            : base()
        {
        }

        public SyncFromAd(SPService service, SPServer server, SPJobLockType lockType)
            : base(tJobName, service, server, lockType) { }

        public SyncFromAd(String name, SPService service, SPServer server, SPJobLockType lockType)
            : base(name, service, server, SPJobLockType.Job)
        {
        }

        public SyncFromAd(String name, SPService service)
            : base(name, service, null, SPJobLockType.Job)
        {
        }

        public override void Execute(Guid targetInstanceId)
        {
            var farm = SPFarm.Local;
            var service = farm.Services.GetValue<SPWebService>();
            var webApplications = FoundationSyncSettings.Local.WebApplicationCollection.Count < 1
                ? (IEnumerable<SPWebApplication>)service.WebApplications
                : FoundationSyncSettings.Local.WebApplicationCollection;

            foreach (SPWebApplication webApplication in webApplications)
            {
                LdapSearcher.SearchPrincipals(webApplication);
            }
        }
    }
}
