using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nauplius.SP.UserSync
{
    [Guid("CA9D049C-D23F-4C1C-A1D5-5CD43EA87D03")]
    public class SyncJob : SPJobDefinition
    {
        private const string tJobName = "Nauplius.SharePoint.FoundationSync";
        private static int j; //RemoveUsers method
        private static int u; //Users updated
        private readonly bool _loggingEx = FoundationSyncSettings.Local.LoggingEx;

        public SyncJob()
            : base()
        {
        }

        public SyncJob(String name, SPService service, SPServer server, SPJobLockType lockType)
            : base(name, service, server, SPJobLockType.Job)
        {
        }

        public SyncJob(String name, SPService service)
            : base(name, service, null, SPJobLockType.Job)
        {
            Title = tJobName;
        }

        public override void Execute(Guid targetInstanceId)
        {
            LoggingEx.CreateReportStorage();

            try
            {
                var farm = SPFarm.Local;
                var ignoredUsers = FoundationSyncSettings.Local.IgnoredUsers;
                var service = farm.Services.GetValue<SPWebService>();
                var userAccounts = new HashSet<SPUser>();
                var groupAccounts = new HashSet<SPUser>();
                var webApplications = FoundationSyncSettings.Local.WebApplicationCollection.Count < 1
                    ? (IEnumerable<SPWebApplication>)service.WebApplications
                    : FoundationSyncSettings.Local.WebApplicationCollection;

                foreach (SPWebApplication webApplication in webApplications)
                {
                    var siteCollections = FoundationSyncSettings.Local.SPSiteCollection.Count < 1
                        ? (IEnumerable<SPSite>)webApplication.Sites
                        : FoundationSyncSettings.Local.SPSiteCollection;

                    foreach (SPSite site in siteCollections)
                    {
                        foreach (SPUser userPrincipal in from SPUser userPrincipal in site.RootWeb.SiteUsers
                                                         let invalidUser = ignoredUsers.Any(word => userPrincipal.LoginName.Contains(word))
                                                         where !invalidUser
                                                         where !userPrincipal.IsDomainGroup
                                                         where userPrincipal.LoginName.Contains(@"\")
                                                         select userPrincipal)
                        {
                            userAccounts.Add(userPrincipal);
                        }

                        if (_loggingEx)
                            LoggingExData(string.Format("{0} user principals in site {1}",
                                userAccounts.Count, site.Url), LoggingEx.LoggingExType.UsersFoundCount);

                        FoudationSync.LogMessage(100, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("{0} user principals in site {1}", userAccounts.Count, site.Url), null);

                        PrincipalHandler.SearchPrincipals(userAccounts, webApplication, site, false, j, u);

                        userAccounts.Clear();

                        foreach (SPUser groupPrincipal in from SPUser groupPrincipal in site.RootWeb.SiteUsers
                                                          let invalidGroup = ignoredUsers.Any(word => groupPrincipal.LoginName.Contains(word))
                                                          where !invalidGroup
                                                          where groupPrincipal.IsDomainGroup
                                                          select groupPrincipal)
                        {
                            groupAccounts.Add(groupPrincipal);
                        }

                        if (_loggingEx)
                            LoggingExData(string.Format("{0} group principals in site {1}",
                                groupAccounts.Count, site.Url), LoggingEx.LoggingExType.UsersFoundCount);

                        FoudationSync.LogMessage(101, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("{0} group principals in site {1}", groupAccounts.Count, site.Url), null);

                        PrincipalHandler.SearchPrincipals(userAccounts, webApplication, site, false, j, u);

                        groupAccounts.Clear();

                        site.Dispose();
                    }
                }

                if (_loggingEx)
                    LoggingExData(string.Format("{0} user principals deleted",
                        j), LoggingEx.LoggingExType.UsersDeletedCount);

                if (_loggingEx)
                    LoggingExData(string.Format("{0} users and groups updated",
                        u), LoggingEx.LoggingExType.UsersUpdatedCount);

                LoggingEx.SaveReport();
            }
            catch (IndexOutOfRangeException)
            {
                FoudationSync.LogMessage(102, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Medium,
                   string.Format("Index was out of range."), null);
            }
        }

        internal static void LoggingExData(string logMessage, LoggingEx.LoggingExType logType)
        {
            LoggingEx.BuildReport(logMessage, logType);
        }
    }
}