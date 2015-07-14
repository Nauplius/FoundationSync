using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.SharePoint.Administration;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;

namespace Nauplius.SP.UserSync
{
    [Guid("5032BAD9-AC8B-4E2E-85CD-A1DBEFEE19B0")]
    internal class FoundationSyncSettings : SPPersistedObject
    {
        private const string name = "FoundationSyncSettings";
        [Persisted] private bool m_deleteUsers;
        [Persisted] private bool m_deleteDisabledUsers;
        [Persisted] private bool m_loggingEx;
        [Persisted] private bool m_loggingExVerbose;
        [Persisted] private SPDocumentLibrary m_loggingExLibrary;
        [Persisted] private Collection<SPWebApplication> m_webApplicationCollection;
        [Persisted] private Collection<SPSite> m_spSiteCollection;
        [Persisted] private bool m_useExchange = false;
        [Persisted] private Uri m_pictureStorageUrl = null;
        [Persisted] private Uri m_ewsUrl = null;
        [Persisted] private int m_pictureExpiryDays = 1;
        [Persisted] private string m_ewsPictureSize = "648x648";
        [Persisted] private List<string> m_ignoredUsers = new List<string>()
        {
            @"NT AUTHORITY\",
            @"SHAREPOINT\",
            @"c:0(.s|true",
            @"c:0!.s|windows"
        };
        [Persisted] private Dictionary<string, string> m_additionalUserAttributes = new Dictionary<string, string>(); //UIL Property (Key), LDAP Attribute (Value)

        public FoundationSyncSettings()
        { }

        public FoundationSyncSettings(SPPersistedObject parent) : base(name, parent)
        { }

        public FoundationSyncSettings(string name, SPPersistedObject parent) : base(name, parent)
        { }

        public FoundationSyncSettings(string name, SPPersistedObject parent, Guid guid) : base(name, parent, guid)
        { }

        public static FoundationSyncSettings Local
        {
            get
            {
                var parent = SPFarm.Local;
                var obj = parent.GetChild<FoundationSyncSettings>(name);

                if (obj != null) return obj;
                obj = new FoundationSyncSettings(name, parent, new Guid("7A2A3CFF-383F-42E1-A019-384C8B6FA3E3"));
                obj.Update();

                return obj;
            }
        }

        public bool DeleteUsers
        {
            get { return m_deleteUsers; }
            set { m_deleteUsers = value; }
        }

        public bool DeleteDisabledUsers
        {
            get { return m_deleteDisabledUsers; }
            set { m_deleteDisabledUsers = value; }
        }

        public bool LoggingEx
        {
            get { return m_loggingEx; }
            set { m_loggingEx = value; }
        }

        internal bool LoggingExVerbose
        {
            get { return m_loggingExVerbose; }
            set { m_loggingExVerbose = value; }
        }

        internal SPDocumentLibrary LoggingExLibrary
        {
            get { return m_loggingExLibrary; }
            set { m_loggingExLibrary = value; }
        }

        public Collection<SPWebApplication> WebApplicationCollection
        {
            get
            {
                var webApplications = m_webApplicationCollection;
                if (webApplications != null) return m_webApplicationCollection;

                webApplications = new Collection<SPWebApplication>();
                m_webApplicationCollection = webApplications;

                return m_webApplicationCollection;
            }
        }

        public Collection<SPSite> SPSiteCollection
        {
            get
            {
                var siteCollections = m_spSiteCollection;
                if (siteCollections != null) return m_spSiteCollection;

                siteCollections = new Collection<SPSite>();
                m_spSiteCollection = siteCollections;

                return m_spSiteCollection;

            }
        }

        public bool UseExchange
        {
            get { return m_useExchange; }
            set { m_useExchange = value; }
        }

        public Uri PictureStorageUrl
        {
            get { return m_pictureStorageUrl; }
            set { m_pictureStorageUrl = value; }
        }

        public Uri EwsUrl
        {
            get { return m_ewsUrl; }
            set { m_ewsUrl = value; }
        }

        public int PictureExpiryDays
        {
            get { return m_pictureExpiryDays; }
            set { m_pictureExpiryDays = value; }
        }

        public string EwsPictureSize
        {
            get { return m_ewsPictureSize; }
            set { m_ewsPictureSize = value; }
        }

        internal List<string> IgnoredUsers
        {
            get { return m_ignoredUsers; }
            set
            {
                m_ignoredUsers = value;
            }
        }

        public Dictionary<string, string> AdditionalUserAttributes
        {
            get { return m_additionalUserAttributes; }
            set { m_additionalUserAttributes = value; }
        }
    }
}
