using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nauplius.SP.UserSync
{
    class ThumbnailHandler
    {
        internal static string GetThumbnail(SPUser user, DirectoryEntry directoryEntry)
        {
            if (FoundationSyncSettings.Local.PictureExpiryDays <= -1) return null;
            var siteUri = FoundationSyncSettings.Local.PictureStorageUrl;
            if (siteUri == null) return null;
            if (string.IsNullOrEmpty(siteUri.AbsoluteUri)) return null;

            var fileUri = string.Empty;

            //One-way hash of SystemUserKey, typically a SID.
            var sHash = SHA1.Create();
            var encoding = new ASCIIEncoding();
            var userBytes = encoding.GetBytes(user.SystemUserKey);
            var userHash = sHash.ComputeHash(userBytes);
            var userHashString = Convert.ToBase64String(userHash);

            //The / is the only illegal character for SharePoint in a Base64 string
            //Replacing it with $, which is not valid in a Base64 string, but works for our purposes

            userHashString = userHashString.Replace("/", "$");

            var fileName = string.Format("{0}{1}", userHashString, ".jpg");

            try
            {
                using (SPSite site = new SPSite(siteUri.AbsoluteUri))
                {
                    var web = site.RootWeb;
                    var list = web.GetList("UserPhotos");
                    var folder = list.RootFolder;
                    var file = folder.Files[fileName];

                    if (file.Length > 1)
                    {
                        var pictureExpiryDays = 1;

                        try
                        {
                            pictureExpiryDays = FoundationSyncSettings.Local.PictureExpiryDays;
                        }
                        catch (InvalidCastException)
                        {
                            FoundationSyncSettings.Local.PictureExpiryDays = 1;
                            pictureExpiryDays = 1;
                        }
                        catch (OverflowException)
                        {
                            FoundationSyncSettings.Local.PictureExpiryDays = 1;
                            pictureExpiryDays = 1;
                        }

                        if ((file.TimeLastModified - DateTime.Now).TotalDays < pictureExpiryDays)
                        {
                            return (string)file.Item[SPBuiltInFieldId.EncodedAbsUrl];
                        }
                    }
                }
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                FoudationSync.LogMessage(1004, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    string.Format("Invalid Site URL specified for Picture Site Collection URL."), null);
                return null;
            }
            catch (Exception)
            {
                FoudationSync.LogMessage(2001, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Verbose,
                    string.Format("Error retrieving picture file from UserPhotos library, continuing to pull new picture."), null);
            }

            if (FoundationSyncSettings.Local.UseExchange)
            {
                var ewsPictureSize = "648x648";

                if (FoundationSyncSettings.Local.EwsPictureSize != null)
                {
                    ewsPictureSize = FoundationSyncSettings.Local.EwsPictureSize;
                }

                var uri = new UriBuilder(string.Format("{0}/s/GetUserPhoto?email={1}&size=HR{2}", FoundationSyncSettings.Local.EwsUrl, user.Email, ewsPictureSize));

                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    var request = (HttpWebRequest)WebRequest.Create(uri.Uri);
                    request.UseDefaultCredentials = true;

                    try
                    {
                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotModified)
                            {
                                if (response.GetResponseStream() != null)
                                {
                                    var image = new Bitmap(response.GetResponseStream());
                                    fileUri = SaveImage(image, siteUri.AbsoluteUri, fileName);
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound ||
                                        response.StatusCode == HttpStatusCode.InternalServerError ||
                                        response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                fileUri = string.Empty;
                            }
                            //else Exchange is not online, incorrect URL, etc.
                        }
                    }
                    catch (Exception exception)
                    {
                        FoudationSync.LogMessage(601, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Medium,
                            exception.Message + exception.StackTrace, null);
                    }

                });
            }
            else
            {
                try
                {
                    var byteArray = (byte[])directoryEntry.Properties["thumbnailPhoto"].Value;

                    if (byteArray.Length > 0)
                    {
                        using (var ms = new MemoryStream(byteArray))
                        {
                            var image = new Bitmap(ms);
                            fileUri = SaveImage(image, siteUri.AbsoluteUri, fileName);
                        }
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            return !string.IsNullOrEmpty(fileUri) ? fileUri : null;
        }

        private static string SaveImage(Bitmap image, string siteUri, string fileName)
        {
            if (siteUri == null) return null;
            try
            {
                using (SPSite site = new SPSite(siteUri))
                {
                    using (SPWeb web = site.RootWeb)
                    {
                        var library = (from SPList list in web.Lists
                                       where list.RootFolder.Name.Equals("UserPhotos")
                                       select list).FirstOrDefault();

                        if (library == null) return null;

                        var ms = new MemoryStream();

                        image.Save(ms, ImageFormat.Jpeg);
                        ms.Close();

                        var byteArray = ms.ToArray();

                        if (byteArray.Length > 0)
                        {
                            var file = library.RootFolder.Files.Add(fileName, byteArray, true);

                            return (string)file.Item[SPBuiltInFieldId.EncodedAbsUrl];
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                FoudationSync.LogMessage(405, FoudationSync.LogCategories.FoundationSync, TraceSeverity.Unexpected,
                    exception.Message + exception.StackTrace, null);
            }

            return null;
        }
    }
}
