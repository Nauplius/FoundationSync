using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    public class FoudationSync : SPDiagnosticsServiceBase
    {
        public static string NaupliusDiagnosticArea = "Nauplius";
        public FoudationSync()
            : base(DefaultName, SPFarm.Local)
        {
        }

        public static FoudationSync Local
        {
            get
            {
                return SPFarm.Local.Services.GetValue<FoudationSync>(DefaultName);
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
 * 400 (Unexpected, Failure):
 *  400, 401, 402 (StackTrace only)
 * 500 (High, Failure):
 *  500: Unexpected exception attempting to retrieve domain name. //Unexpected, incorrect, ToDo: should be 400
 *  501, 502 (StackTrace only) //Unexpected, incorrect, ToDo: should be 400
 *  503 (StackTrace only)
 *  504: Unable to read FoundationSyncSetting object.
 *  505: IsActive returned an error (?)
 * 601 (?? ToDo: Merge with new Medium category)
 * 701 (?? Unexpected, ToDo: move to 400)
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
 *  2001: Error retriving file, continuing to pull new file. //ToDo: Fix Spelling
*/