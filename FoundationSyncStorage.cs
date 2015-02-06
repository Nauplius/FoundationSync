using System;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class FoundationSyncStorage
    {
        private static readonly Guid pGuid = new Guid("5032BAD9-AC8B-4E2E-85CD-A1DBEFEE19B0");

        internal FoundationSyncSettings SyncSettings()
        {
            try
            {
                var farm = SPFarm.Local;

                if (farm != null)
                {
                    var settingsStorage = (FoundationSyncSettings)farm.GetObject(pGuid);

                    if (settingsStorage != null)
                    {
                        return settingsStorage;
                    }
                }
            }
            catch (Exception e)
            {
                FoudationSync.LogMessage(504, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    "Unable to read FoundationSyncSetting object. " + e.StackTrace, null);
            }

            return null;
        }
    }
}
