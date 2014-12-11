using System;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync.Features.UserPhotosFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("951b4f0b-5200-415b-86ff-0631da807835")]
    public class UserPhotosFeatureEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            var web = properties.Feature.Parent as SPWeb;

            if (web == null) return;
            try
            {
                web.GetList("UserPhotos");
            }
            catch (Exception)
            {
                CreateList(web);
            }
            finally
            {
                SetPermissions(web);
            }
        }

        internal void CreateList(SPWeb web)
        {
            try
            {

                web.AllowUnsafeUpdates = true;
                web.Lists.Add("UserPhotos",
                    "This library holds User Photos pulled from Active Directory and/or Exchange",
                    SPListTemplateType.PictureLibrary);
                web.AllowUnsafeUpdates = false;
                web.Update();
            }
            catch (Exception ex)
            {
                FoudationSync.LogMessage(1003, FoudationSync.LogCategories.FoundationSync,
                    TraceSeverity.Unexpected,
                    string.Format("Unable to create UserPhotos library. " +
                                  "Please create the UserPhotos library manually. {0}",
                    ex.InnerException), null);
            }      
        }

        internal void SetPermissions(SPWeb web)
        {
            try
            {
                var list = web.GetList("UserPhotos");
                var allUsers = web.EnsureUser("NT AUTHORITY\\authenticated users");
                var roleAssignment = new SPRoleAssignment(allUsers);
                var readerRole = web.RoleDefinitions.GetByType(SPRoleType.Reader);

                roleAssignment.RoleDefinitionBindings.Add(readerRole);

                if (!list.HasUniqueRoleAssignments)
                {
                    list.BreakRoleInheritance(true);
                }

                list.RoleAssignments.Add(roleAssignment);
                list.Update();
            }
            catch (Exception ex)
            {
                FoudationSync.LogMessage(1003, FoudationSync.LogCategories.FoundationSync,
                    TraceSeverity.Unexpected,
                    string.Format("Unable to set permissions on UserPhotos list. " +
                                  "Add Authenticated Users with Read rights manually. {0}",
                    ex.InnerException), null);
            }         
        }
    }
}


