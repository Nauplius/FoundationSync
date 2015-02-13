using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class LoggingEx
    {
        private MemoryStream _usersFoundMemoryStream = new MemoryStream();
        private MemoryStream _usersUpdatedMemoryStream = new MemoryStream();
        private MemoryStream _usersDeletedMemoryStream = new MemoryStream();
        private MemoryStream _userPropertiesMemoryStream = new MemoryStream();
        private const string reportLibrary = "SyncReportLibrary";

        internal void CreateReportStorage()
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
                    var listGuid = web.Lists.Add(reportLibrary,
                        "Reporting on FoundationSync activity.", listTemplates,
                        documentTemplate);

                    library = (SPDocumentLibrary)web.Lists[listGuid];
                    library.OnQuickLaunch = true;
                    library.EnableFolderCreation = false;
                    library.Update();
                    settingsStorage.SyncSettings().LoggingExLibrary = (SPDocumentLibrary) library;
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
            var bytes = Encoding.UTF8.GetBytes(logMessage);

            switch (logType)
            {
                case LoggingExType.UsersFoundCount:
                    _usersFoundMemoryStream.WriteAsync(bytes, 0, bytes.Length);
                    _usersFoundMemoryStream.FlushAsync();
                    break;
                case LoggingExType.UsersUpdatedCount:
                    _usersUpdatedMemoryStream.WriteAsync(bytes, 0, bytes.Length);
                    _usersUpdatedMemoryStream.FlushAsync();
                    break;
                case LoggingExType.UsersDeletedCount:
                    _usersDeletedMemoryStream.WriteAsync(bytes, 0, bytes.Length);
                    _usersDeletedMemoryStream.FlushAsync();
                    break;
                case LoggingExType.UserProperties:
                    _userPropertiesMemoryStream.WriteAsync(bytes, 0, bytes.Length);
                    _userPropertiesMemoryStream.FlushAsync();
                    break;
            }

            bytes = null;
        }

        internal void SaveReport()
        {
            FileStream fileStream = null;
            
            _usersFoundMemoryStream.CopyTo(fileStream);
            _usersUpdatedMemoryStream.CopyTo(fileStream);
            _usersDeletedMemoryStream.CopyTo(fileStream);
            _usersUpdatedMemoryStream.CopyTo(fileStream);
            fileStream.Flush();

            const string format = "MMdyyyy-HHmm-FoundationSync.log";

            var fileName = string.Format("{0}", DateTime.Now.ToString(format));

            try
            {
                var syncSettings = new FoundationSyncSettings();
                var library = syncSettings.LoggingExLibrary;
                
                using(SPSite site = new SPSite(library.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(library.ParentWeb.ID))
                    {
                        var folder = library.RootFolder;
                        folder.Files.Add(fileName, fileStream, false);
                        folder.Update();
                    }
                }
            }
            catch (Exception)
            {
                //ToDo: Log to ULS
                throw;
            }
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
