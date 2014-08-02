using System;
using System.Net;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;

namespace Nauplius.SP.UserSync.ADMIN.FoundationSync
{
    public partial class ProfileSettings : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadSettings();
            }
        }

        protected void btnSave_OnClick(object sender, EventArgs e)
        {
            bool site, exchange = false;

            if (!string.IsNullOrEmpty(tBox1.Text))
            {
                site = ValidateSiteCollection();
            }

            if (!string.IsNullOrEmpty(tBox2.Text))
            {
                exchange = ValidateExchangeConnection();
            }
        }

        internal bool ValidateSiteCollection()
        {
            if (!Uri.IsWellFormedUriString(tBox1.Text, UriKind.Absolute)) return false;

            var uri = new UriBuilder(tBox1.Text + "/_api/lists/getbytitle('UserPhotos')");
            var request = (HttpWebRequest)WebRequest.Create(uri.Uri);
            request.Credentials = CredentialCache.DefaultNetworkCredentials;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var farm = SPFarm.Local;
                farm.Properties["pictureStorageUrl"] = tBox1.Text + "/UserPhotos";
                farm.Update();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                //not a valid location or access denied
                throw;
            }

            return false;
        }

        internal bool ValidateExchangeConnection()
        {
            var credentials = new CredentialCache();
            if (!Uri.IsWellFormedUriString(tBox2.Text, UriKind.Absolute)) return false;

            var uri = new UriBuilder(tBox2.Text);
            var request = (HttpWebRequest)WebRequest.Create(uri.Uri);
            request.Credentials = CredentialCache.DefaultNetworkCredentials;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var farm = SPFarm.Local;
                farm.Properties["useExchange"] = "true";
                farm.Properties["ewsUrl"] = uri.Uri.ToString();
                farm.Update();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }

        internal void LoadSettings()
        {

        }
    }
}
