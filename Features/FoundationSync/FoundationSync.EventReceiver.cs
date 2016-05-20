using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.SP.UserSync.Features.UserSync
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("3c120b48-5259-411f-ab34-9ab9cfbad598")]
    public class UserSyncEventReceiver : SPFeatureReceiver
    {
        private const string tJobName = "Nauplius.SharePoint.FoundationSync";

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            var farm = SPFarm.Local;

<<<<<<< HEAD
            SyncServiceApplication syncService = null;

            foreach (var service in farm.Services)
            {
                if (String.Compare(service.Name, SyncServiceApplication.FoundationSync, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    syncService = (service as SyncServiceApplication);
                    break;
                }

                if (syncService != null) continue;
                syncService = new SyncServiceApplication(farm);
                syncService.Update();
                
                SyncServiceInstance syncInstance;

                foreach (var server in farm.Servers)
                {
                    syncInstance = new SyncServiceInstance(server, syncService);
                    syncInstance.Update();
                }
=======
            var services = from s in farm.Services
                           where s.Name == "SPTimerV4"
                           select s;

            var service = services.First();

            foreach (var job in service.JobDefinitions.Where(job => job.Name == tJobName))
            {
                job.Delete();
            }
>>>>>>> parent of d3145bf... 2013: Add Service Instance features. Remove PropertyBag functionality for targeting a specific SPServer.

            var schedule = new SPDailySchedule {BeginHour = 0, EndHour = 4};

<<<<<<< HEAD
                try
                {
                    var timerJob = new SyncJob(syncService, null, SPJobLockType.None) {Schedule = schedule};
                    timerJob.Update();
                }
                catch (NullReferenceException)
=======
            try
            {
                if (!string.IsNullOrEmpty(farm.Properties.ContainsKey("FoundationSyncPreferredServer").ToString()))
>>>>>>> parent of d3145bf... 2013: Add Service Instance features. Remove PropertyBag functionality for targeting a specific SPServer.
                {
                    var server = farm.Servers[farm.Properties["FoundationSyncPreferredServer"].ToString()];
                    var timerJob = new SyncJob(tJobName, service, server, SPJobLockType.Job) { Schedule = schedule };
                    timerJob.Update();
                }
            }
            catch (NullReferenceException)
            {
                var timerJob = new SyncJob(tJobName, service) { Schedule = schedule };
                timerJob.Update();     
            }

            RegisterLogging(properties, true);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            DeleteJob();

            RegisterLogging(properties, false);
        }

        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
            try
            {
                var persistedObject = FoundationSyncSettings.Local;
                persistedObject.Provision();
                persistedObject.Update();
            }
            catch (Exception)
            {
            }
        }

        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            DeleteJob();

            RegisterLogging(properties, false);

            try
            {
                var persistedObject = FoundationSyncSettings.Local;
                persistedObject.Unprovision();
            }
            catch (Exception)
            {
            }
        }

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}

        private static void DeleteJob()
        {
            var local = SPFarm.Local;

            var services = from s in local.Services
                           where s.Name == "SPTimerV4"
                           select s;

            var service = services.First();

            foreach (SPJobDefinition job in service.JobDefinitions.Where(job => job.Name == tJobName))
            {
                job.Delete();
            }
        }

        private static void RegisterLogging(SPFeatureReceiverProperties properties, bool register)
        {
            var farm = properties.Definition.Farm;

            if (farm == null) return;
            var log = FoudationSync.Local;

            if (register)
            {
                if (log != null) return;
                log = new FoudationSync();
                log.Update();

                if (log.Status != SPObjectStatus.Offline)
                {
                    log.Status = SPObjectStatus.Offline;
                }

                if (log.Status != SPObjectStatus.Online)
                {
                    log.Provision();
                }
            }
            else
            {
                if (log == null) return;
                try
                {
                    log.Unprovision();
                }
                catch
                {
                }
                finally
                {
                    log.Delete();
                }
            }
        }
    }
}
