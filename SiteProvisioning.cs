using Microsoft.SharePoint;

namespace Nauplius.SP.UserSync
{
    class SiteProvisioning : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWeb web = null;
            SPSite site = null;
            var parent = properties.Feature.Parent;

            if (parent is SPWeb)
            {
                web = (SPWeb) parent;
                site = web.Site;
            }

            if (parent is SPSite)
            {
                site = (SPSite) parent;
                web = site.RootWeb;
            }

            if (web != null)
                web.Update();
        }
    }
}
