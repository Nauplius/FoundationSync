using System;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class FoundationSyncStorage
    {
        internal FoundationSyncSettings SyncSettings()
        {

            try
            {
                var foundationSyncSettings = FoundationSyncSettings.Local;

                if (foundationSyncSettings != null) return foundationSyncSettings;
            }
            catch (Exception e)
            {
                FoundationSync.LogMessage(504, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    "Unable to read FoundationSyncSetting object. " + e.StackTrace, null);
            }

            return null;
        }
    }
}
