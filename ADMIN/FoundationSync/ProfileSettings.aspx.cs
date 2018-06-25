using System;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace Nauplius.SP.UserSync.ADMIN.FoundationSync
{
    public partial class ProfileSettings : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSettings();   
            }
        }

        protected void btnSave_OnClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tBox1.Text))
            {
                ValidateSiteCollection();
            }
            else
            {
                FoundationSyncSettings.Local.PictureStorageUrl = null;
                FoundationSyncSettings.Local.Update();
            }

            if (!string.IsNullOrEmpty(tBox2.Text))
            {
                ValidateExchangeConnection();
            }
            else
            {
                FoundationSyncSettings.Local.UseExchange = false;
                FoundationSyncSettings.Local.EwsUrl = null;
                FoundationSyncSettings.Local.Update();
            }

            if (Page.IsValid)
            {
                SPUtility.Redirect("/applications.aspx", SPRedirectFlags.Default, Context);
            }
        }

        internal void ValidateSiteCollection()
        {
            if (!Uri.IsWellFormedUriString(tBox1.Text, UriKind.Absolute))
            {
                v2.Visible = true;
                v2.IsValid = false;
                return;
            }

            var tbox1Uri = new Uri(tBox1.Text + "/UserPhotos");
            FoundationSyncSettings.Local.PictureStorageUrl = tbox1Uri;
            FoundationSyncSettings.Local.Update();
        }

        internal void ValidateExchangeConnection()
        {

            if (!Uri.IsWellFormedUriString(tBox2.Text, UriKind.Absolute))
            {
                FoundationSyncSettings.Local.UseExchange = false;
                FoundationSyncSettings.Local.EwsUrl = null;
                FoundationSyncSettings.Local.Update();

            }
            else
            {
                try
                {
                    FoundationSyncSettings.Local.UseExchange = true;

                    var tbox2Uri = new Uri(tBox2.Text);
                    FoundationSyncSettings.Local.EwsUrl = tbox2Uri;
                    FoundationSyncSettings.Local.Update();
                }
                catch (Exception ex)
                {
                    UserSync.FoundationSync.LogMessage(1002, UserSync.FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                        string.Format("Unable to set UseExchange or EwsUrl values with error {0}.", ex.InnerException), null);
                }                
            }
        }

        internal void LoadSettings()
        {
            try
            {
                if (FoundationSyncSettings.Local.UseExchange)
                {
                    tBox2.Text = FoundationSyncSettings.Local.EwsUrl.AbsoluteUri;
                }
            }
            catch (Exception)
            {
                UserSync.FoundationSync.LogMessage(1002, UserSync.FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unable to retrieve EwsUrl when loading settings."), null);                
            }

            try
            {
                if (FoundationSyncSettings.Local.PictureStorageUrl != null)
                {
                    var uri = FoundationSyncSettings.Local.PictureStorageUrl.AbsoluteUri;

                    if(uri.EndsWith("/UserPhotos"))
                    {
                        tBox1.Text = uri.Replace("/UserPhotos", string.Empty);
                    }
                }
            }
            catch (Exception)
            {
                UserSync.FoundationSync.LogMessage(1002, UserSync.FoundationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unable to retrieve PictureStorageUrl when loading settings."), null);     
            }
        }
    }
}
