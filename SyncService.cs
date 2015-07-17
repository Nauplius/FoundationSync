using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    public class SyncService : SPService
    {
        public static string FoundationSync = "Foundation Synchronization Service";

        public override string DisplayName
        {
            get { return FoundationSync; }
        }

        public override string TypeName
        {
            get { return "Foundation Synchronization Service Type"; }
        }

        public SyncService()
        {
        }

        public SyncService(SPFarm farm) : base(FoundationSync, farm){}
    }
}
