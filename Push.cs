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
    public class AttributePush : SPJobDefinition
    {
        private const string tJobName = "Nauplius.SharePoint.FoundationSync";

        public AttributePush()
            : base()
        {
        }

        public AttributePush(String name, SPService service, SPServer server, SPJobLockType lockType)
            : base(name, service, server, lockType)
        {
        }

        public AttributePush(String name, SPService service)
            : base(name, service, null, SPJobLockType.Job)
        {
            Title = tJobName;
        }

        public override void Execute(Guid targetInstanceId)
        {
            var farm = SPFarm.Local;
            var invalidUids = new List<string> { @"NT AUTHORITY\", @"SHAREPOINT\", "c:0(.s|true" };
            var service = farm.Services.GetValue<SPWebService>();
            var userAccounts = new HashSet<SPUser>();
            var groupAccounts = new HashSet<SPUser>();

            foreach (SPWebApplication webApplication in service.WebApplications)
            {
                foreach (SPSite site in webApplication.Sites)
                {
                    foreach (SPUser userPrincipal in from SPUser userPrincipal in site.RootWeb.SiteUsers
                                                     let invalidUser = invalidUids.Any(word => userPrincipal.LoginName.Contains(word))
                                                     where !invalidUser
                                                     where !userPrincipal.IsDomainGroup
                                                     where userPrincipal.LoginName.Contains(@"\")
                                                     select userPrincipal)
                    {
                        userAccounts.Add(userPrincipal);
                    }

                    FoudationSync.LogMessage(100, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                        string.Format("{0} user principals in site {1}", userAccounts.Count, site.Url), null);
                    GetDomains(userAccounts, webApplication, site, false);
                    userAccounts.Clear();

                    foreach (SPUser groupPrincipal in from SPUser groupPrincipal in site.RootWeb.SiteUsers
                                                      let invalidGroup = invalidUids.Any(word => groupPrincipal.LoginName.Contains(word))
                                                      where !invalidGroup
                                                      where groupPrincipal.IsDomainGroup
                                                      select groupPrincipal)
                    {
                        groupAccounts.Add(groupPrincipal);
                    }

                    FoudationSync.LogMessage(101, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                        string.Format("{0} group principals in site {1}", groupAccounts.Count, site.Url), null);
                    GetDomains(groupAccounts, webApplication, site, true);
                    groupAccounts.Clear();

                    site.Dispose();
                }
            }
        }

        private static void GetDomains(HashSet<SPUser> objPrincipals, SPWebApplication webApplication, SPSite site, bool isGroup)
        {
            var domains = webApplication.PeoplePickerSettings.SearchActiveDirectoryDomains;

            if (domains.Count == 0)
            {
                var domain = new SPPeoplePickerSearchActiveDirectoryDomain
                {
                    DomainName = Environment.UserDomainName
                };

                SearchPrincipals(domain, objPrincipals, webApplication, site, isGroup);
            }
            else
            {
                foreach (var domain in domains)
                {
                    SearchPrincipals(domain, objPrincipals, webApplication, site, isGroup);
                }
            }
        }

        private static void SearchPrincipals(SPPeoplePickerSearchActiveDirectoryDomain domain, HashSet<SPUser> objPrincipals,
                                 SPWebApplication webApplication, SPSite site, bool isGroup)
        {
            var chasing = webApplication.PeoplePickerSettings.ReferralChasingOption;

            {
                string ldapPath = null;

                try
                {
                    var objContext = new DirectoryContext(
                        DirectoryContextType.Domain, domain.DomainName);
                    var objDomain = Domain.GetDomain(objContext);
                    ldapPath = objDomain.Name;
                }
                catch (DirectoryServicesCOMException e)
                {
                    FoudationSync.LogMessage(500, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                        "Unexpected exception attempting to retrieve domain name. " + e.StackTrace, null);
                }

                var listItems = site.RootWeb.SiteUserInfoList.Items;
                var itemCount = listItems.Count;

                foreach (SPUser objPrincipal in objPrincipals)
                {
                    var claimProvider = SPClaimProviderManager.Local;
                    string loginName, filter;
                    string[] properties;

                    if (isGroup)
                    {
                        if (claimProvider != null && objPrincipal.LoginName.Contains(@"c:0+.w"))
                        {
                            var sid = claimProvider.DecodeClaim(objPrincipal.LoginName).Value;

                            try
                            {
                                loginName = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
                            }
                            catch (IdentityNotMappedException exception)
                            {
                                FoudationSync.LogMessage(503, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                    exception.Message + exception.StackTrace, null);
                                break;
                            }
                        }
                        else
                        {
                            loginName = objPrincipal.LoginName;
                        }

                        properties = new[]{
                            "sAMAccountName", "mail", "proxyAddresses"
                        };

                        var entry = new DirectoryEntry(@"LDAP://" + ldapPath);
                        var i = loginName.LastIndexOf('\\');
                        var objName = loginName.Remove(0, i + 1);
                        filter = string.Format("(&(objectClass=group)(sAMAccountName={0}))", objName);
                        var searcher = new DirectorySearcher(entry, filter, properties)
                        {
                            ReferralChasing = chasing
                        };

                        try
                        {
                            var result = searcher.FindOne();

                            if (result == null) continue;
                            var directoryEntry = result.GetDirectoryEntry();
                            UpdateUilGroup(objPrincipal, directoryEntry, listItems, itemCount);
                        }
                        catch (DirectoryServicesCOMException exception)
                        {
                            FoudationSync.LogMessage(501, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                exception.Message + exception.StackTrace, null);
                        }
                    }
                    else
                    {
                        if (claimProvider != null && objPrincipal.LoginName.Contains(@"i:0#.w"))
                        {
                            loginName = claimProvider.DecodeClaim(objPrincipal.LoginName).Value;
                        }
                        else
                        {
                            loginName = objPrincipal.LoginName;
                        }

                        properties = new[]
                        {
                            "displayName", "mail", "title", "mobile", "proxyAddresses", "department",
                            "sn", "givenName", "telephoneNumber", "wWWHomePage", "physicalDeliveryOfficeName",
                            "thumbnailPhoto"
                        };

                        var entry = new DirectoryEntry("LDAP://" + ldapPath);
                        var i = loginName.LastIndexOf('\\');
                        var objName = loginName.Remove(0, i + 1);
                        filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", objName);
                        var searcher = new DirectorySearcher(entry, filter, properties)
                        {
                            ReferralChasing = chasing
                        };

                        try
                        {
                            var result = searcher.FindOne();

                            if (result == null) continue;
                            var directoryEntry = result.GetDirectoryEntry();
                            UpdateUilUser(objPrincipal, directoryEntry, listItems, itemCount);
                        }
                        catch (DirectoryServicesCOMException exception)
                        {
                            FoudationSync.LogMessage(502, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                exception.Message + exception.StackTrace, null);
                        }
                    }
                }
            }
        }

        private static void UpdateUilGroup(SPUser group, DirectoryEntry directoryEntry,
            SPListItemCollection listItems, int itemCount)
        {
            try
            {
                var j = 0;
                for (; j < itemCount; j++)
                {
                    var item = listItems[j];

                    if (item["Account"].ToString().ToLower() != group.LoginName.ToLower()) continue;
                    item["EMail"] = (directoryEntry.Properties["mail"].Value == null)
                        ? string.Empty
                        : directoryEntry.Properties["mail"].Value.ToString();

                    try
                    {
                        if (directoryEntry.Properties["proxyAddresses"].Value != null)
                        {
                            var array = (Array)directoryEntry.Properties["proxyAddresses"].Value;

                            foreach (var o in from string o in array
                                              where o.Contains(("sip:"))
                                              select o)
                            {
                                item["SipAddress"] = o.Remove(0, 4);
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        if (directoryEntry.Properties["proxyAddresses"].Value.ToString().Contains("sip:"))
                        {
                            item["SipAddress"] =
                                directoryEntry.Properties["proxyAddresses"].Value.ToString().Remove(0, 4);
                        }
                        else
                        {
                            item["SipAddress"] = string.Empty;
                        }
                    }

                    FoudationSync.LogMessage(200, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                        string.Format("Updating group {0} (ID {1}) on Site Collection {2}.", item.DisplayName, item.ID, item.Web.Site.Url), null);
                    item.Update();
                    return;
                }
            }
            catch (SPException exception)
            {
                FoudationSync.LogMessage(400, FoudationSync.LogCategories.FoundationSync,
                    TraceSeverity.Unexpected, exception.Message + " " + exception.StackTrace, null);
            }
        }
        private static void UpdateUilUser(SPUser user, DirectoryEntry directoryEntry, SPListItemCollection listItems, int itemCount)
        {
            try
            {
                var j = 0;
                for (; j < itemCount; j++)
                {
                    var item = listItems[j];

                    if (item["Name"].ToString().ToLower() != user.LoginName.ToLower()) continue;
                    item["Title"] = (directoryEntry.Properties["displayName"].Value == null)
                                        ? string.Empty
                                        : directoryEntry.Properties["displayName"].Value.ToString();

                    item["EMail"] = (directoryEntry.Properties["mail"].Value == null)
                                        ? string.Empty
                                        : directoryEntry.Properties["mail"].Value.ToString();

                    item["JobTitle"] = (directoryEntry.Properties["title"].Value == null)
                                           ? string.Empty
                                           : directoryEntry.Properties["title"].Value.ToString();

                    item["MobilePhone"] = (directoryEntry.Properties["mobile"].Value == null)
                                              ? string.Empty
                                              : directoryEntry.Properties["mobile"].Value.ToString();

                    if (user.SystemUserKey != null)
                    {
                        item["Picture"] = GetThumbnail(user, directoryEntry);                       
                    }

                    try
                    {
                        if (directoryEntry.Properties["proxyAddresses"].Value != null)
                        {
                            var array = (Array)directoryEntry.Properties["proxyAddresses"].Value;

                            foreach (var o in from string o in array
                                              where o.Contains(("sip:"))
                                              select o)
                            {
                                item["SipAddress"] = o.Remove(0, 4);
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        if (directoryEntry.Properties["proxyAddresses"].Value.ToString().Contains("sip:"))
                        {
                            item["SipAddress"] =
                                directoryEntry.Properties["proxyAddresses"].Value.ToString().Remove(0, 4);
                        }
                        else
                        {
                            item["SipAddress"] = string.Empty;
                        }
                    }

                    item["Department"] = (directoryEntry.Properties["department"].Value == null)
                                             ? string.Empty
                                             : directoryEntry.Properties["department"].Value.ToString();

                    FoudationSync.LogMessage(201, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                        string.Format("Updating user {0} (ID {1}) on Site Collection {2}.", item.DisplayName, item.ID, item.Web.Site.Url), null);
                    item.Update();
                    return;
                }
            }
            catch (SPException exception)
            {
                FoudationSync.LogMessage(401, FoudationSync.LogCategories.FoundationSync,
                    TraceSeverity.Unexpected, exception.Message + " " + exception.StackTrace, null);
            }
        }

        private static string GetThumbnail(SPUser user, DirectoryEntry directoryEntry)
        {
            var farm = SPFarm.Local;
            var siteUri = (string)farm.Properties["pictureStorageUrl"];
            var fileUri = string.Empty;

            //One-way hash of SystemUserKey, typically a SID.
            var sHash = SHA1.Create();
            var encoding = new ASCIIEncoding();
            var userBytes = encoding.GetBytes(user.SystemUserKey);
            var userHash = sHash.ComputeHash(userBytes);
            var userHashString = Convert.ToBase64String(userHash);
            
            //The / is the only illegal character for SharePoint in a Base64 string
            //Replacing it with $, which is not valid in a Base64 string, but works for our purposes

            userHashString = userHashString.Replace("/", "$");

            var fileName = string.Format("{0}{1}", userHashString, ".jpg");

            try
            {
                using (SPSite site = new SPSite(siteUri))
                {
                    using (SPWeb web = site.RootWeb)
                    {
                        var list = web.GetList("UserPhotos");
                        var folder = list.RootFolder;
                        var file = folder.Files[fileName];

                        if (file.Length > 1)
                        {
                            var pictureExpiryDays = 1;

                            if (farm.Properties.ContainsKey("pictureExpiryDays"))
                            {
                                pictureExpiryDays = (int) farm.Properties["pictureExpiryDays"];
                            }

                            if ((file.TimeLastModified - DateTime.Now).TotalDays < pictureExpiryDays)
                            {
                                return (string)file.Item[SPBuiltInFieldId.EncodedAbsUrl];
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //File not found, etc. Discard exception and continue.
            }

            if ((string) farm.Properties["useExchange"] == "true")
            {
                const string size = "HR648x648";
                var uri = new UriBuilder(string.Format("{0}/s/GetUserPhoto?email={1}&{2}", farm.Properties["ewsUrl"], user.Email, size));
                var request = (HttpWebRequest)WebRequest.Create(uri.Uri);
                request.Credentials = CredentialCache.DefaultNetworkCredentials;

                try
                {
                    using (var response = (HttpWebResponse) request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotModified)
                        {
                            if (response.GetResponseStream() != null)
                            {
                                var image = new Bitmap(response.GetResponseStream());
                                fileUri = SaveImage(user, image, siteUri, fileName);
                            }
                        }
                        //else Exchange is not online, incorrect URL, etc.
                    }
                }
                catch (Exception exception)
                {
                    FoudationSync.LogMessage(601, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                        exception.Message + exception.StackTrace, null);
                    return null;
                }
            }
            else
            {
                var byteArray = (byte[])directoryEntry.Properties["thumbnailPhoto"][0];

                if (byteArray.Length > 0)
                {
                    using (var ms = new MemoryStream(byteArray))
                    {
                        var image = new Bitmap(ms);
                        fileUri = SaveImage(user, image, siteUri, fileName);
                    }
                }
            }

            return !string.IsNullOrEmpty(fileUri) ? fileUri : null;
        }

        private static string SaveImage(SPUser user, Bitmap image, string siteUri, string fileName)
        {
            try
            {
                using (SPSite site = new SPSite(siteUri))
                {
                    using (SPWeb web = site.RootWeb)
                    {
                        var library = web.Lists["UserPhotos"];


                        var byteArray = new byte[0];

                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Jpeg);
                            ms.Close();

                            byteArray = ms.ToArray();
                        }

                        if (byteArray.Length > 0)
                        {
                            var file = library.RootFolder.Files.Add(fileName, byteArray, true);

                            return (string) file.Item[SPBuiltInFieldId.EncodedAbsUrl];
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                FoudationSync.LogMessage(701, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    exception.Message + exception.StackTrace, null);
            }

            return null;
        }
    }
}