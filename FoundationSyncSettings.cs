using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        [Persisted] private string m_ewsUrl = string.Empty;

        [Persisted] private List<string> m_ignoredUsers = new List<string>()
        {
            @"NT AUTHORITY\",
            @"SHAREPOINT\",
            "c:0(.s|true"
        };

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

        public bool LoggingExVerbose
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

        private bool UseExchange
        {
            get { return m_useExchange; }
        }

        private Uri PictureStorageUrl
        {
            get { return m_pictureStorageUrl; }
        }

        private string EwsUrl
        {
            get { return m_ewsUrl; }
        }

        internal List<string> IgnoredUsers
        {
            get { return m_ignoredUsers; }
        }
    }
}
