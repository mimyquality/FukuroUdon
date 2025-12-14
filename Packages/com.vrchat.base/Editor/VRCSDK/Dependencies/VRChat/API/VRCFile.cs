using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VRC.SDKBase.Editor.Api
{
    public struct VRCFile
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public string MimeType { get; set; }
        public string Extension { get; set; }
        public List<VersionEntry> Versions { get; set; }
        
        public class VersionEntry
        {
            public int Version { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool Deleted { get; set; }
            public FileDescriptor File { get; set; }
            public FileDescriptor Signature { get; set; }
            public FileDescriptor Delta { get; set; }

            public class FileDescriptor
            {
                public string Status { get; set; }
                public string URL { get; set; }
                public string MD5 { get; set; }
                public string Category { get; set; }
                public int SizeInBytes { get; set; }
                public string FileName { get; set; }
                public string UploadId { get; set; }
                [JsonProperty("cdns")]
                public List<string> CDNs { get; set; }
            }
        }

        public int GetLatestVersion()
        {
            return (this.Versions?.Count ?? 0) - 1;
        }

        /// <summary>
        /// Returns true if there is a valid version that is not deleted.
        /// </summary>
        public bool HasExistingOrPendingVersion()
        {
            var latestVersion = GetLatestVersion();
            if (latestVersion > 0)
            {
                latestVersion -= Versions.Count(v => v == null || v.Deleted);
            }

            return latestVersion > 0;
        }

        public bool IsLatestVersionWaiting()
        {
            if (!HasExistingOrPendingVersion()) return false;
            var version = Versions[GetLatestVersion()];
            if (version.Status == "waiting") return true;
            return ((version.File?.Status == "waiting") ||
                    (version.Signature?.Status == "waiting"));
        }

        public bool IsLatestVersionQueued()
        {
            if (!HasExistingOrPendingVersion()) return false;
            var version = Versions[GetLatestVersion()];
            if (version.Status == "queued") return true;
            return ((version.File?.Status == "queued") ||
                    (version.Signature?.Status == "queued"));
        }
        
        public bool IsLatestVersionErrored()
        {
            if (!HasExistingOrPendingVersion()) return false;
            var version = Versions[GetLatestVersion()];
            if (version.Status == "error") return true;
            return ((version.File?.Status == "error") ||
                    (version.Signature?.Status == "error"));
        }

        public bool HasQueuedOperation()
        {
            if (IsLatestVersionWaiting()) return false;

            return HasExistingOrPendingVersion() && IsLatestVersionQueued();
        }
    }

}

