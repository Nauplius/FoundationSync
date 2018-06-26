using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nauplius.SP.UserSync
{
    [Guid("CA9D049C-D23F-4C1C-A1D5-5CD43EA87D03")]
    public class SyncJob : SPJobDefinition
    {
        private const string tJobName = "Nauplius.SharePoint.FoundationSync";

        public SyncJob()
            : base()
        {
        }

        public SyncJob(string name, SPService service, SPServer server, SPJobLockType lockType)
            : base(name, service, server, SPJobLockType.Job)
        {
        }

        public SyncJob(string name, SPService service)
            : base(name, service, null, SPJobLockType.Job)
        {
            Title = tJobName;
        }

        public override void Execute(Guid targetInstanceId)
        {
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

                        FoundationSync.LogMessage(100, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("{0} user principals in site {1}", userAccounts.Count, site.Url), null);

                        PrincipalHandler.SearchPrincipals(userAccounts, webApplication, site, false);

                        userAccounts.Clear();

                        foreach (SPUser groupPrincipal in from SPUser groupPrincipal in site.RootWeb.SiteUsers
                                                          let invalidGroup = ignoredUsers.Any(word => groupPrincipal.LoginName.Contains(word))
                                                          where !invalidGroup
                                                          where groupPrincipal.IsDomainGroup
                                                          select groupPrincipal)
                        {
                            groupAccounts.Add(groupPrincipal);
                        }

                        FoundationSync.LogMessage(101, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("{0} group principals in site {1}", groupAccounts.Count, site.Url), null);

                        PrincipalHandler.SearchPrincipals(groupAccounts, webApplication, site, true);
                        groupAccounts.Clear();

                        site.Dispose();
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                FoundationSync.LogMessage(102, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Medium,
                   null, null);
            }
        }
    }
}