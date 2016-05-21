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

                    if (item["EMail"] != eMail)
                    {
                        item["Email"] = eMail;
                        shouldUpdate = true;
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

                                if (item["SipAddress"] != sipAddress)
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

                            if (item["SipAddress"] != sipAddress)
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
    }
}
