using System;
using System.IO;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class LoggingEx
    {
        private MemoryStream _memoryStream = new MemoryStream();

        private const string reportLibrary = "SyncReportLibrary";
        private void CreateReportStorage()
        {
            var settingsStorage = new FoundationSyncStorage();

            if (settingsStorage.SyncSettings() == null) return;
            if (!settingsStorage.SyncSettings().LoggingEx) return;
            try
            {
                var adminWebApplication = new SPAdministrationWebApplication();

                using (SPSite site = adminWebApplication.Sites[0])
                {
                    var web = site.OpenWeb(site.RootWeb.ID);

                    var library = web.Lists[reportLibrary];

                    if (library != null) return;
                    var listTemplates = web.ListTemplates["Document Library"];
                    var documentTemplate = (from SPDocTemplate dt in web.DocTemplates
                        where dt.Type == 100
                        select dt).FirstOrDefault();
                    var listGuid = web.Lists.Add("FoundationSync Reports",
                        "Reporting on FoundationSync activity.", listTemplates,
                        documentTemplate);

                    library = (SPDocumentLibrary)web.Lists[listGuid];
                    library.OnQuickLaunch = true;
                    library.EnableFolderCreation = false;
                    library.Update();
                }
            }
            catch (Exception e)
            {
                FoudationSync.LogMessage(402, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    "Unable to create Report Storage in Central Administration for" +
                    " FoundationSyncSetting object. " + e.StackTrace, null);
            }
        }

        internal void BuildReport(string logMessage, LoggingExType logType)
        {
            switch (logType)
            {
                case LoggingExType.UsersFoundCount:
                case LoggingExType.UsersUpdatedCount:
                case LoggingExType.UsersDeletedCount:
                case LoggingExType.UserProperties:
                    break;

            }


            //MemoryStream object (declare in class)
            //Total Users Found in Site Collection
            //Total Users Deleted from Site Collection
            //Total Users Updated in Site Collection
            //Properties updated for each user
        }

        internal void SaveReport(MemoryStream report)
        {
            //Save to reportLibrary
        }

        public enum LoggingExType
        {
            UsersFoundCount,
            UsersDeletedCount,
            UsersUpdatedCount,
            UserProperties
        }
    }
}
