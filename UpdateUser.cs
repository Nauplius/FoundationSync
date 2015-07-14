using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.DirectoryServices;

namespace Nauplius.SP.UserSync
{
    //Flow: Record user in report on Users sheet (A), record any updated property (B), record overall updated (C)
    public class UpdateUser
    {
        internal static void User(SPUser user, DirectoryEntry directoryEntry, SPListItemCollection listItems, int itemCount, int u)
        {
            try
            {
                var j = 0;
                for (; j < itemCount; j++)
                {
                    var shouldUpdate = false;

                    var item = listItems[j];

                    if (!String.Equals(item["Name"].ToString(), user.LoginName, StringComparison.CurrentCultureIgnoreCase)) continue;

                    var title = (directoryEntry.Properties["displayName"].Value == null)
                                        ? string.Empty
                                        : directoryEntry.Properties["displayName"].Value.ToString();

                    if (item["Title"].ToString() != title)
                    {
                        item["Title"] = title;
                        shouldUpdate = true;
                    }

                    var eMail = (directoryEntry.Properties["mail"].Value == null)
                                        ? string.Empty
                                        : directoryEntry.Properties["mail"].Value.ToString();

                    if (item["EMail"].ToString() != eMail)
                    {
                        item["EMail"] = eMail;
                        shouldUpdate = true;
                    }

                    var jobTitle = (directoryEntry.Properties["title"].Value == null)
                                           ? string.Empty
                                           : directoryEntry.Properties["title"].Value.ToString();

                    if (item["JobTitle"].ToString() != jobTitle)
                    {
                        item["JobTitle"] = jobTitle;
                        shouldUpdate = true;
                    }

                    var mobilePhone = (directoryEntry.Properties["mobile"].Value == null)
                                              ? string.Empty
                                              : directoryEntry.Properties["mobile"].Value.ToString();

                    if (item["MobilePhone"].ToString() != mobilePhone)
                    {
                        item["MobilePhone"] = mobilePhone;
                        shouldUpdate = true;
                    }

                    if (user.SystemUserKey != null)
                    {
                        var uri = ThumbnailHandler.GetThumbnail(user, directoryEntry);

                        if (!string.IsNullOrEmpty(uri))
                        {
                            item["Picture"] = uri;
                        }
                        else if (string.IsNullOrEmpty(uri))
                        {
                            item["Picture"] = string.Empty;
                        }
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
                                var sipAddress = o.Remove(0, 4);

                                if (item["SipAddress"].ToString() != sipAddress)
                                {
                                    item["SipAddress"] = sipAddress;
                                    shouldUpdate = true;
                                }
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        if (directoryEntry.Properties["proxyAddresses"].Value.ToString().Contains("sip:"))
                        {
                            var sipAddress = directoryEntry.Properties["proxyAddresses"].Value.ToString().Remove(0, 4);

                            if (item["SipAddress"].ToString() != sipAddress)
                            {
                                item["SipAddress"] = sipAddress;
                                shouldUpdate = true;
                            }

                        }
                        else
                        {
                            item["SipAddress"] = string.Empty;
                        }
                    }

                    var department = (directoryEntry.Properties["department"].Value == null)
                                             ? string.Empty
                                             : directoryEntry.Properties["department"].Value.ToString();

                    if (item["Department"].ToString() != department)
                    {
                        item["Department"] = department;
                        shouldUpdate = true;
                    }

                    try
                    {
                        var additionalAttributes = FoundationSyncSettings.Local.AdditionalUserAttributes;

                        foreach (var ldapAttribute in additionalAttributes)
                        {
                            var value = (directoryEntry.Properties[ldapAttribute.Value].Value == null)
                                                   ? string.Empty
                                                   : directoryEntry.Properties[ldapAttribute.Value].Value.ToString();

                            if (item[ldapAttribute.Key].ToString() != value)
                            {
                                item[ldapAttribute.Key] = value;
                                shouldUpdate = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //ToDo: Log exception -- e.g., 'should check existing LDAP attrib, UIL key, AdditionalAttributes key:value pair(s)
                    }

                    if (shouldUpdate)
                    {
                        FoudationSync.LogMessage(201, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("Updating user {0} (ID {1}) on Site Collection {2}.", item.DisplayName, item.ID, item.Web.Site.Url), null);
                        item.Update();
                        ++u;
                    }

                    return;
                }
            }
            catch (SPException exception)
            {
                FoudationSync.LogMessage(401, FoudationSync.LogCategories.FoundationSync,
                    TraceSeverity.Unexpected, exception.Message + " " + exception.StackTrace, null);
            }
        }
    }
}
