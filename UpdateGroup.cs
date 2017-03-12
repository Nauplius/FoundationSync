using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauplius.SP.UserSync
{
    public class UpdateGroup
    {
        private static bool shouldUpdate = false;
        //Flow: Record group in report on Groups sheet (A), record any updated property (B), record overall updated (C)
        internal static void Group(SPUser group, DirectoryEntry directoryEntry,
            SPListItemCollection listItems, int itemCount, int u)
        {
            try
            {
                var j = 0;
                for (; j < itemCount; j++)
                {
                    var shouldUpdate = false;

                    var item = listItems[j];

                    if (item["Name"].ToString().ToLower() != group.LoginName.ToLower()) continue;

                    var eMail = (directoryEntry.Properties["mail"].Value == null)
                        ? string.Empty
                        : directoryEntry.Properties["mail"].Value.ToString();

                    try
                    {
                        TryUpdateValue(item, "EMail", (string)item["EMail"], eMail);
                    }
                    catch (Exception)
                    {
                        FoudationSync.LogMessage(506, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                            string.Format("Unable to update {0} for group {1} (ID {2}) on Site Collection {3}.", "EMail", item.DisplayName, item.ID, item.Web.Site.Url), null);
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
                                    TryUpdateValue(item, "SipAddress", (string)item["SipAddress"],
                                        sipAddress);
                                }
                                catch (Exception)
                                {
                                    FoudationSync.LogMessage(506, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                        string.Format("Unable to update {0} for group {1} (ID {2}) on Site Collection {3}.", "SipAddress", item.DisplayName, item.ID, item.Web.Site.Url), null);
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
                                FoudationSync.LogMessage(506, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                                    string.Format("Unable to update {0} for user {1} (ID {2}) on Site Collection {3}.", "SipAddress", item.DisplayName, item.ID, item.Web.Site.Url), null);
                            }    
                        }
                        else
                        {
                            item["SipAddress"] = string.Empty;
                        }
                    }

                    if (shouldUpdate)
                    {
                        FoudationSync.LogMessage(200, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                            string.Format("Updating group {0} (ID {1}) on Site Collection {2}.", item.DisplayName, item.ID, item.Web.Site.Url), null);
                        item.Update();
                        ++u;
                    }

                    return;
                }
            }
            catch (SPException exception)
            {
                FoudationSync.LogMessage(400, FoudationSync.LogCategories.FoundationSync,
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
