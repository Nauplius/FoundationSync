using System;
using System.Net;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
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
            var farm = SPFarm.Local;

            if (!string.IsNullOrEmpty(tBox1.Text))
            {
                ValidateSiteCollection();
            }
            else
            {
                if (farm.Properties.ContainsKey("pictureStorageUrl"))
                {
                    farm.Properties.Remove("pictureStorageUrl");
                }
            }

            if (!string.IsNullOrEmpty(tBox2.Text))
            {
                ValidateExchangeConnection();
            }
            else
            {
                if (farm.Properties.ContainsKey("useExchange"))
                {
                    farm.Properties.Remove("useExchange");
                }

                if (farm.Properties.ContainsKey("ewsUrl"))
                {
                    farm.Properties.Remove("ewsUrl");
                }
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

            try
            {
                var farm = SPFarm.Local;
                var url = tBox1.Text + "/UserPhotos";

                if (farm.Properties.ContainsKey("pictureStorageUrl"))
                {
                    farm.Properties["pictureStorageUrl"] = url;
                }
                else
                {
                    farm.Properties.Add("pictureStorageUrl", url);
                }

                farm.Update(true);
            }
            catch (Exception ex)
            {
                FoudationSync.LogMessage(1002, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unable to set pictureStorageUrl with error {0}.", ex.InnerException), null);
            }
        }

        internal void ValidateExchangeConnection()
        {
            var uri = new UriBuilder(tBox2.Text);

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                try
                {
                    var farm = SPFarm.Local;

                    if (farm.Properties.ContainsKey("useExchange"))
                    {
                        farm.Properties["useExchange"] = "True";
                    }
                    else
                    {
                        farm.Properties.Add("useExchange", "True");
                    }

                    if (farm.Properties.ContainsKey("ewsUrl"))
                    {
                        farm.Properties["ewsUrl"] = uri.Uri.ToString();
                    }
                    else
                    {
                        farm.Properties.Add("ewsUrl", uri.Uri.ToString());
                    }

                    farm.Update(true);
                }
                catch (Exception ex)
                {
                    FoudationSync.LogMessage(1002, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                        string.Format("Unable to set useExchange or ewsUrl values with error {0}.", ex.InnerException), null);
                }                
            });
        }

        internal void LoadSettings()
        {
            var farm = SPFarm.Local;

            try
            {
                if (farm.Properties.ContainsKey("useExchange") && (string) farm.Properties["useExchange"] == "True")
                {
                    if (!string.IsNullOrEmpty(farm.Properties["ewsUrl"].ToString()))
                    {
                        tBox2.Text = farm.Properties["ewsUrl"].ToString();
                    }
                    else
                    {
                        farm.Properties["useExchange"] = "False";
                        farm.Update();
                    }   
                }
            }
            catch (Exception)
            {
                FoudationSync.LogMessage(1001, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unable to retrieve useExchange or ewsUrl when loading settings. " +
                                  "Try setting them manually on the SPFarm object."), null);
            }

            try
            {
                if (!farm.Properties.ContainsKey("pictureStorageUrl") ||
                    string.IsNullOrEmpty(farm.Properties["pictureStorageUrl"].ToString())) return;
                


                var url = farm.Properties["pictureStorageUrl"].ToString();
                var index = url.LastIndexOf("/", StringComparison.Ordinal);

                if (index > 0)
                    url = url.Substring(0, index);
                tBox1.Text = url;
            }
            catch (Exception)
            {
                FoudationSync.LogMessage(1002, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Unable to retrieve pictureStorageUrl when loading settings. " +
                                  "Try setting it manually on the SPFarm object."), null);
            }
        }
    }
}
