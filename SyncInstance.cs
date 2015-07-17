using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class SyncInstance : SPServiceInstance
    {
        public static string FoundationSyncInstance = "Foundation Synchronization Service Instance";
        private SPActionLink _manageLink;
        private SPActionLink _provisionLink;
        private SPActionLink _unprovisionLink;
        private ICollection<string> _roles;


        public override SPActionLink ManageLink
        {
            get { return _manageLink ?? (_manageLink = new SPActionLink(SPActionLinkType.None)); }
        }

        public override SPActionLink ProvisionLink
        {
            get
            {
                return _provisionLink ?? (_provisionLink = new SPActionLink(SPActionLinkType.ObjectModel));
            }
        }

        public override SPActionLink UnprovisionLink
        {
            get { return _unprovisionLink ?? (_unprovisionLink = new SPActionLink(SPActionLinkType.ObjectModel)); }
        }

        public override ICollection<string> Roles
        {
            get { return _roles ?? (_roles = new string[1] {"Custom"}); }
        }

        public SyncInstance() : base()
        {
        }

        public SyncInstance(SPServer server, SyncService service) : base(FoundationSyncInstance, server, service)
        {
        }
    }
}
