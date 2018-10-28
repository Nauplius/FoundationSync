using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    public class FoundationSync : SPDiagnosticsServiceBase
    {
        public static string NaupliusDiagnosticArea = "Nauplius";
        public FoundationSync()
            : base(DefaultName, SPFarm.Local)
        {
        }

        public static FoundationSync Local
        {
            get
            {
                return SPFarm.Local.Services.GetValue<FoundationSync>(DefaultName);
            }
        }

        public static class LogCategories
        {
            public static string FoundationSync = "FoundationSync";
        }

        public static string DefaultName
        {
            get
            {
                return NaupliusDiagnosticArea;
            }
        }

        public static string AreaName
        {
            get
            {
                return NaupliusDiagnosticArea;
            }
        }

        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {

            var areas = new List<SPDiagnosticsArea>
            {
                new SPDiagnosticsArea(NaupliusDiagnosticArea, 0, 0, false, new List<SPDiagnosticsCategory>
                    {
                        new SPDiagnosticsCategory(LogCategories.FoundationSync, null, TraceSeverity.Medium, EventSeverity.Information, 0, 0, false, true),
                    })
            };
            return areas;
        }

        internal static void LogMessage(ushort id, string LogCategory, TraceSeverity traceSeverity, string message, object[] data)
        {
            try
            {
                var log = Local;

                if (log == null) return;
                var category = log.Areas[NaupliusDiagnosticArea].Categories[LogCategory];
                log.WriteTrace(id, category, traceSeverity, message, data);
            }
            catch (Exception)
            {
            }
        }
    }
}

// Error List
/*
 * 100 (Verbose, Informational):
 *  100: {0} user principals in site {1}
 *  101: {0} group principals in site {1}
 *  102: Index was out of range. //Medium, incorrect
 * 200 (Verbose, Success):
 *  200: Updating group {0} (ID {1}) on Site Collection {2}.
 *  201: Updating user {0} (ID {1}) on Site Collection {2}.
 *  202: Group LoginName {0} on Site Collection {1}.
 *  203: User LoginName {0} on Site Collection {1}.
 * 400 (Unexpected, Failure):
 *  400, 401, 402, 403, 404, 405 (StackTrace only)
 *  410: Unexpected exception attempting to retrieve domain name.
 * 500 (High, Failure):
 *  503 (StackTrace only)
 *  504: Unable to read FoundationSyncSetting object.
 *  505: IsActive returned an error
 *  506: Unable to remove user due to an error
 * 600 (Medium, Failure)
 * 601: (StackTrace only)
 * UI Errors:
 * 1000 (Unexpected, Failure):
 *  1001: Unable to retrieve useExchange or ewsUrl when loading settings. Try setting them manually on the SPFarm object.
 *  1002: Unable to set pictureStorageUrl with error {0}.
 *  1002: Unable to set useExchange or ewsUrl values with error {0}.
 *  1002: Unable to retrieve pictureStorageUrl when loading settings. Try setting it manually on the SPFarm object.
 *  1003: Unable to create UserPhotos library. Please create the UserPhotos library manually. {0}
 *  1003: Unable to set permissions on UserPhotos list. Add Authenticated Users with Read rights manually. {0}
 *  1004: Invalid Site URL specified for Picture Site Collection URL
 * 2000 (Verbose, Failure):
 *  2001: Error retrieving picture file from UserPhotos library, continuing to pull new picture.
*/
