using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Administration;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;

namespace Nauplius.SP.UserSync
{
    [Guid("5032BAD9-AC8B-4E2E-85CD-A1DBEFEE19B0")]
    public class FoundationSyncSettings : SPPersistedObject
    {
        [Persisted] private bool m_deleteUsers = false;
        [Persisted] private bool m_deleteDisabledUsers = false;
        [Persisted] private bool m_loggingEx = false;
        [Persisted] private SPWebApplicationCollection m_webApplicationCollection = null;
        [Persisted] private SPSiteCollection m_spSiteCollection = null;
        [Persisted] private bool m_useExchange = false;
        [Persisted] private string m_pictureStorageUrl = string.Empty;
        [Persisted] private string m_ewsUrl = string.Empty;

        [Persisted] private List<string> m_ignoredUsers = new List<string>()
        {
            @"NT AUTHORITY\",
            @"SHAREPOINT\",
            "c:0(.s|true"
        };

        public FoundationSyncSettings()
        {
        }

        public FoundationSyncSettings(string name, SPPersistedObject parent) : base(name, parent)
        {
        }

        public FoundationSyncSettings(string name, SPPersistedObject parent, Guid guid) : base(name, parent, guid)
        {
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

        public SPWebApplicationCollection WebApplicationCollection
        {
            get { return m_webApplicationCollection; }
            set { m_webApplicationCollection = value; }
        }

        public SPSiteCollection SPSiteCollection
        {
            get { return m_spSiteCollection; }
            set { m_spSiteCollection = value; }
        }

        private bool UseExchange
        {
            get { return m_useExchange; }
        }

        private string PictureStorageUrl
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
