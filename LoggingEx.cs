using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync
{
    class LoggingEx
    {
        private static MemoryStream _usersFoundMemoryStream = new MemoryStream();
        private static MemoryStream _usersUpdatedMemoryStream = new MemoryStream();
        private static MemoryStream _usersDeletedMemoryStream = new MemoryStream();
        private static MemoryStream _userPropertiesMemoryStream = new MemoryStream();
        private const string reportLibrary = "SyncReportLibrary";

        internal static void CreateReportStorage()
        {
            var settingsStorage = new FoundationSyncStorage();

            if (settingsStorage.SyncSettings() == null) return;
            if (!settingsStorage.SyncSettings().LoggingEx) return;
            try
            {
                var adminWebApplication = SPAdministrationWebApplication.Local;

                using (SPSite site = adminWebApplication.Sites[0])
                {
                    var web = site.OpenWeb(site.RootWeb.ID);

                    var library = (from SPList list in web.Lists
                                   where list.RootFolder.Name.Equals(reportLibrary)
                                   select list).FirstOrDefault();

                    if (library == null)
                    {
                        var listTemplates = web.ListTemplates["Document Library"];
                        var documentTemplate = (from SPDocTemplate dt in web.DocTemplates
                            where dt.Type == 100
                            select dt).FirstOrDefault();
                        var listGuid = web.Lists.Add(reportLibrary,
                            "Reporting on FoundationSync activity.", listTemplates,
                            documentTemplate);

                        library = (SPDocumentLibrary) web.Lists[listGuid];
                        library.OnQuickLaunch = true;
                        library.EnableFolderCreation = false;
                        library.Update();
                        settingsStorage.SyncSettings().LoggingExLibrary = (SPDocumentLibrary) library;
                    }
                    else
                    {
                        settingsStorage.SyncSettings().LoggingExLibrary = (SPDocumentLibrary) library;
                    }
                }
            }
            catch (Exception e)
            {
                FoundationSync.LogMessage(402, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    "Unable to create Report Storage in Central Administration for" +
                    " FoundationSyncSetting object. " + e.StackTrace, null);
            }
        }

        internal static void BuildReport(string logMessage, LoggingExType logType)
        {
            var bytes = Encoding.UTF8.GetBytes(string.Format("{0}\r\n", logMessage));

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

        internal static void SaveReport()
        {
            var settingsStorage = new FoundationSyncStorage();

            if (settingsStorage.SyncSettings() == null) return;
            if (!settingsStorage.SyncSettings().LoggingEx) return;
            if (settingsStorage.SyncSettings().LoggingExLibrary == null) return;

            var dt = DateTime.Now.ToString("MMdyyyy-HHmm");

            var fileName = string.Format("{0}-FoundationSync.log", dt);

            try
            {
                var adminWebApplication = SPAdministrationWebApplication.Local;

                using (SPSite site = adminWebApplication.Sites[0])
                {
                    var web = site.OpenWeb(site.RootWeb.ID);

                    var library = (from SPList list in web.Lists
                        where list.RootFolder.Name.Equals(reportLibrary)
                        select list).FirstOrDefault();

                    var folder = library.RootFolder;
                    var tempBytes = Encoding.UTF8.GetBytes("Abc123");
                    var tempStream = new MemoryStream(tempBytes);

                    folder.Files.Add(fileName, tempStream, true);
                    folder.Update();

                    var url = string.Format("{0}/{1}", folder.Url, fileName);
                    var file = web.GetFile(url);
                    var stream = file.OpenBinaryStream();

                    stream.Position = 0;

                    var separatorMessage = Encoding.UTF8.GetBytes("\r\n-------------------\r\n");

                    var userFoundBytes = _usersFoundMemoryStream.GetBuffer();
                    if (userFoundBytes.Length == 0)
                    {
                        var logMessage = Encoding.UTF8.GetBytes("No User or Groups Found.\r\n");
                        stream.Write(logMessage, 0, logMessage.Length);
                    }
                    {
                        stream.Write(userFoundBytes, 0, userFoundBytes.Length);  
                    }

                    stream.Write(separatorMessage, 0, separatorMessage.Length);

                    var userUpdatedBytes = _usersUpdatedMemoryStream.GetBuffer();

                    if (userUpdatedBytes.Length == 0)
                    {
                        var logMessage = Encoding.UTF8.GetBytes("No User or Groups Updated.\r\n");
                        stream.Write(logMessage, 0, logMessage.Length);
                    }
                    else
                    {
                        stream.Write(userUpdatedBytes, 0, userUpdatedBytes.Length);
                    }

                    stream.Write(separatorMessage, 0, separatorMessage.Length);

                    var userDeletedBytes = _usersDeletedMemoryStream.GetBuffer();

                    if (userDeletedBytes.Length == 0)
                    {
                        var logMessage = Encoding.UTF8.GetBytes("No Users Deleted.\r\n");
                        stream.Write(logMessage, 0, logMessage.Length);
                    }
                    else
                    {
                        stream.Write(userDeletedBytes, 0, userDeletedBytes.Length);
                        stream.Flush();                        
                    }

                    _userPropertiesMemoryStream.Clear();
                    _usersDeletedMemoryStream.Clear();
                    _usersFoundMemoryStream.Clear();
                    _usersUpdatedMemoryStream.Clear();

                    file.SaveBinary(stream);
                    file.Update();
                }
            }
            catch (Exception)
            {
                //ToDo: Log to ULS
            }
        }

        public enum LoggingExType
        {
            UsersFoundCount,
            UsersDeletedCount,
            UsersUpdatedCount,
            UserProperties,
            UserName,
            GroupName,
            Properties,
            IsActive,
            IsDeleted,
            RemovedFromSite
        }
    }
    public static class ExtensionMethod
    {
        public static void Clear(this MemoryStream source)
        {
            var buffer = source.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            source.Position = 0;
            source.SetLength(0);
        }
    }
}
