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
        private static bool shouldUpdate = false;
        internal static void User(SPUser user, DirectoryEntry directoryEntry, SPListItemCollection listItems, int itemCount)
        {
            try
            {
                var j = 0;
                for (; j < itemCount; j++)
                {
                    shouldUpdate = false;

                    var item = listItems[j];

                    if (!string.Equals(item["Name"].ToString(), user.LoginName, StringComparison.CurrentCultureIgnoreCase)) continue;

                    var title = (directoryEntry.Properties["displayName"].Value == null)
                                        ? string.Empty
                                        : directoryEntry.Properties["displayName"].Value.ToString();

                    try
                    {
                        TryUpdateValue(item, "Title", (string)item["Title"], title);
                    }
                    catch (Exception)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                            string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "Title", item.DisplayName, item.ID, item.Web.Site.Url), null);                        
                    }

                    var eMail = (directoryEntry.Properties["mail"].Value == null)
                                        ? string.Empty
                                        : directoryEntry.Properties["mail"].Value.ToString();

                    try
                    {
                        TryUpdateValue(item, "EMail", (string)item["EMail"], eMail);
                    }
                    catch (Exception)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                            string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "EMail", item.DisplayName, item.ID, item.Web.Site.Url), null);
                    }


                    var jobTitle = (directoryEntry.Properties["title"].Value == null)
                                           ? string.Empty
                                           : directoryEntry.Properties["title"].Value.ToString();

                    try
                    {
                        TryUpdateValue(item, "JobTitle", (string)item["JobTitle"], jobTitle);
                    }
                    catch (Exception)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                           string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "JobTitle", item.DisplayName, item.ID, item.Web.Site.Url), null);                       
                    }


                    var mobilePhone = (directoryEntry.Properties["mobile"].Value == null)
                                              ? string.Empty
                                              : directoryEntry.Properties["mobile"].Value.ToString();
                    try
                    {
                        TryUpdateValue(item, "MobilePhone", (string) item["MobilePhone"], mobilePhone);
                    }
                    catch (Exception)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                           string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "MobilePhone", item.DisplayName, item.ID, item.Web.Site.Url), null);     
                    }

                    if (user.SystemUserKey != null)
                    {
                        var uri = ThumbnailHandler.GetThumbnail(user, directoryEntry);

                        try
                        {
                            if (!string.IsNullOrEmpty(uri))
                            {
                                item["Picture"] = uri;
                            }
                            else if (string.IsNullOrEmpty(uri))
                            {
                                item["Picture"] = string.Empty;
                            }                       
                        }
                        catch (Exception)
                        {
                            FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "Picture", item.DisplayName, item.ID, item.Web.Site.Url), null);                              
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

                                try
                                {
                                    TryUpdateValue(item, "SipAddress", (string) item["SipAddress"],
                                        sipAddress);
                                }
                                catch (Exception)
                                {
                                    FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                        string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "SipAddress", item.DisplayName, item.ID, item.Web.Site.Url), null);                                    
                                }
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        if (directoryEntry.Properties["proxyAddresses"].Value.ToString().Contains("sip:"))
                        {
                            var sipAddress = directoryEntry.Properties["proxyAddresses"].Value.ToString().Remove(0, 4);

                            try
                            {
                                TryUpdateValue(item, "SipAddress", (string)item["SipAddress"],
                                    sipAddress);
                            }
                            catch (Exception)
                            {
                                FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                    string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "SipAddress", item.DisplayName, item.ID, item.Web.Site.Url), null);
                            }

                        }
                        else
                        {
                            try
                            {
                                item["SipAddress"] = string.Empty;
                            }
                            catch (Exception)
                            {
                                FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                    string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "SipAddress", item.DisplayName, item.ID, item.Web.Site.Url), null);
                            }
                        }
                    }

                    var department = (directoryEntry.Properties["department"].Value == null)
                                             ? string.Empty
                                             : directoryEntry.Properties["department"].Value.ToString();

                    try
                    {
                        TryUpdateValue(item, "Department", (string)item["Department"], department);
                    }
                    catch (Exception)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                            string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "Department", item.DisplayName, item.ID, item.Web.Site.Url), null);
                    }

                    try
                    {
                        var additionalAttributes = FoundationSyncSettings.Local.AdditionalUserAttributes;

                        foreach (var ldapAttribute in additionalAttributes)
                        {
                            var value = (directoryEntry.Properties[ldapAttribute.Value].Value == null)
                                                   ? string.Empty
                                                   : directoryEntry.Properties[ldapAttribute.Value].Value.ToString();

                            TryUpdateValue(item, ldapAttribute.Key, (string)item[ldapAttribute.Key], value);
                        }
                    }
                    catch (Exception)
                    {
                        FoundationSync.LogMessage(506, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                            string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "AdditionalAttribues Value", item.DisplayName, item.ID, item.Web.Site.Url), null);
                    }

                    if (shouldUpdate)
                    {
                        FoundationSync.LogMessage(201, FoundationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("Updating user {0} (ID {1}) on Site Collection {2}.", item.DisplayName, item.ID, item.Web.Site.Url), null);
                        item.Update();
                    }

                    return;
                }
            }
            catch (SPException exception)
            {
                FoundationSync.LogMessage(401, FoundationSync.LogCategories.FoundationSync,
                    TraceSeverity.Unexpected, exception.Message + " " + exception.StackTrace, null);
            }
        }

        internal static bool TryUpdateValue(SPListItem item, string itemProperty, string itemValue, string ldapValue)
        {
            if (string.IsNullOrEmpty(itemValue) && string.IsNullOrEmpty(ldapValue)) return false;
            if (itemValue == ldapValue) return false;
            item[itemProperty] = ldapValue;
            shouldUpdate = true;
            return true;
        }
    }
}
