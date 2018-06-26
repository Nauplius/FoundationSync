using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;

namespace Nauplius.SP.UserSync
{
    public class PrincipalHandler
    {
        internal static void SearchPrincipals(HashSet<SPUser> objPrincipals,
                         SPWebApplication webApplication, SPSite site, bool isGroup)
        {
            var chasing = webApplication.PeoplePickerSettings.ReferralChasingOption;

            {
                var listItems = site.RootWeb.SiteUserInfoList.Items;
                var itemCount = listItems.Count;

                foreach (SPUser objPrincipal in objPrincipals)
                {
                    var claimProvider = SPClaimProviderManager.Local;
                    string loginName, filter;
                    List<string> userProperties = new List<string> { "displayName", "mail", "title", "mobile", "proxyAddresses", "department",
                            "sn", "givenName", "telephoneNumber", "wWWHomePage", "physicalDeliveryOfficeName", "thumbnailPhoto" };
                    List<string> groupProperties = new List<string> { "sAMAccountName", "mail", "proxyAddresses" };

                    if (isGroup)
                    {
                        if (claimProvider != null && objPrincipal.LoginName.Contains(@"c:0+.w"))
                        {
                            var sid = claimProvider.DecodeClaim(objPrincipal.LoginName).Value;
                            FoundationSync.LogMessage(202, FoundationSync.LogCategories.FoundationSync, TraceSeverity.VerboseEx, 
                                string.Format("IsGroup:{0}. RootWeb: {1}. {2}{3}", objPrincipal.LoginName, site.RootWeb.Url), null);

                            try
                            {
                                loginName = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();

                            }
                            catch (Exception exception)
                            {
                                FoundationSync.LogMessage(503, FoundationSync.LogCategories.FoundationSync,
                                    TraceSeverity.High,
                                    exception.Message + exception.StackTrace, null);
                                continue;
                            }
                        }
                        else
                        {
                            loginName = objPrincipal.LoginName;
                        }

                        var ldapPath = GetDomain(loginName.Split('\\')[0]);

                        var entry = new DirectoryEntry(@"LDAP://" + ldapPath);
                        var i = loginName.LastIndexOf('\\');
                        var objName = loginName.Remove(0, i + 1);
                        filter = string.Format("(&(objectClass=group)(sAMAccountName={0}))", objName);

                        var searcher = new DirectorySearcher(entry, filter, groupProperties.ToArray())
                        {
                            ReferralChasing = chasing
                        };

                        try
                        {
                            var result = searcher.FindOne();
                            var directoryEntry = result.GetDirectoryEntry();
                            UpdateGroup.Group(objPrincipal, directoryEntry, listItems, itemCount);
                        }
                        catch (DirectoryServicesCOMException exception)
                        {
                            FoundationSync.LogMessage(403, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected, 
                                string.Format("IsGroup:{0}. RootWeb: {1}. {2}{3}", objPrincipal.LoginName, 
                                site.RootWeb.Url, exception.Message, exception.StackTrace), null);
                        }
                    }
                    else
                    {
                        FoundationSync.LogMessage(203, FoundationSync.LogCategories.FoundationSync, TraceSeverity.VerboseEx,
                            string.Format("IsUser:{0}. RootWeb: {1}. {2}{3}", objPrincipal.LoginName, site.RootWeb.Url), null);

                        if (claimProvider != null && objPrincipal.LoginName.Contains(@"i:0#.w"))
                        {
                            loginName = claimProvider.DecodeClaim(objPrincipal.LoginName).Value;
                        }
                        else
                        {
                            loginName = objPrincipal.LoginName;
                        }

                        try
                        {
                            foreach (var attribute in FoundationSyncSettings.Local.AdditionalUserAttributes)
                            {
                                userProperties.Add(attribute.Value);
                            }
                        }
                        catch(Exception)
                        { }

                        var ldapPath = GetDomain(loginName.Split('\\')[0]);

                        if (string.IsNullOrEmpty(ldapPath))
                            continue;

                        var entry = new DirectoryEntry("LDAP://" + ldapPath);

                        filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", loginName.Split('\\')[1]);
                        var searcher = new DirectorySearcher(entry, filter, userProperties.ToArray())
                        {
                            ReferralChasing = chasing
                        };

                        try
                        {
                            var result = searcher.FindOne();

                            if (result == null)
                            {
                                if (FoundationSyncSettings.Local.DeleteUsers)
                                {
                                    RemoveUsers(objPrincipal, site.Url);
  
                                }
                                continue;
                            }   

                            if (!IsActive(result.GetDirectoryEntry()))
                            {
                                if (FoundationSyncSettings.Local.DeleteDisabledUsers)
                                {
                                    RemoveUsers(objPrincipal, site.Url);
                                }
                                continue;
                            }

                            var directoryEntry = result.GetDirectoryEntry();
                            UpdateUser.User(objPrincipal, directoryEntry, listItems, itemCount);
                        }
                        catch (DirectoryServicesCOMException exception)
                        {
                            FoundationSync.LogMessage(404, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                string.Format("IsUser:{0}. RootWeb: {1}. {2}{3}", objPrincipal.LoginName, 
                                site.RootWeb.Url, exception.Message, exception.StackTrace), null);
                        }
                        catch (Exception exception)
                        {
                            FoundationSync.LogMessage(405, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected, 
                                string.Format("Unknown error: {0}{1}", exception.Message, exception.StackTrace), null);
                        }
                    }
                }
            }
        }

        private static string GetDomain(string domainName)
        {
            string ldapPath = null;

            try
            {
                var objContext = new DirectoryContext(
                    DirectoryContextType.Domain, domainName);
                var objDomain = Domain.GetDomain(objContext);
                ldapPath = objDomain.Name;
            }
            catch (Exception e)
            {
                FoundationSync.LogMessage(410, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unexpected exception attempting to retrieve domain name: {0}. {1}", domainName, e.StackTrace), null);
                return null;
            }

            return ldapPath;
        }

        private static bool IsActive(DirectoryEntry de)
        {
            if (de == null) return false;
            if (de.NativeGuid == null) return false;
            var status = true;

            try
            {
                var flags = (int)de.Properties["userAccountControl"].Value;
                status = !Convert.ToBoolean(flags & 0x0002);
            }
            catch (Exception e)
            {
                FoundationSync.LogMessage(505, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unexpected exception attempting to determine if user is active: User: {0}. Status value: {1}. {2}", de.Username, status, e.StackTrace), null);
            }

            return status;
        }

        private static void RemoveUsers(SPUser objPrincipal, string siteUrl)
        {
            using (SPSite site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    try
                    {
                        var user = web.SiteUsers[objPrincipal.LoginName];
                        if (user.IsSiteAdmin) return;

                        web.SiteUsers.Remove(user.LoginName);
                    }
                    catch (Exception e)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                            string.Format("Unexpected exception attempting to remove user: User: {0} (ID: {1}). Url: {2}. {3}", objPrincipal.LoginName, objPrincipal.ID, siteUrl, e.StackTrace), null);
                    }
                }
            }
        }
    }
}
