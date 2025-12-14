using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VRC.Core;

namespace VRC.SDKBase.Editor.Api
{
    public struct VRCWorld: IVRCContent
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PreviewYoutubeId { get; set; }
        public List<string> Tags { get; set; }
        public List<string> UdonProducts { get; set; }
        
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        
        public string ImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        
        public string ReleaseStatus { get; set; }
        
        public int Capacity { get; set; }
        public int RecommendedCapacity { get; set; }
        public int Favorites { get; set; }
        public int Visits { get; set; }
        // public int Popularity { get; set; } // this field doesn't deserialize correctly for VERY popular worlds, ignoring in SDK for now
        public int Heat { get; set; }
        public int Occupants { get; set; }
        public int PublicOccupants { get; set; }
        public int PrivateOccupants { get; set; }
        
        public List<Instance> Instances { get; set; }
        
        public struct Instance
        {
            public string ID { get; set; }
            public int Occupants { get; set; }
        }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        public DateTime PublicationDate { get; set; }
        public DateTime LabsPublicationDate { get; set; }
        
        public int Version { get; set; }
        public List<VRCUnityPackage> UnityPackages { get; set; }
        
        public string Organization { get; set; }
        public bool Featured { get; set; }
        
        public string GetLatestAssetUrlForPlatform(string platform)
        {
            string assetUrl = null;
            var preferredUnityVersion = new UnityVersion();
            if (this.UnityPackages == null) return null;
            foreach (var unityPackage in this.UnityPackages)
            {
                if (UnityVersion.Parse(unityPackage.UnityVersion).CompareTo(preferredUnityVersion) < 0) continue;
                if (unityPackage.Platform != platform) continue;
                assetUrl = unityPackage.AssetUrl;
                preferredUnityVersion = UnityVersion.Parse(unityPackage.UnityVersion);
            }

            return assetUrl;
        }
    }
    
    // Only a subset of fields is allowed to be changed through the SDK
    public struct VRCWorldChanges {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public int RecommendedCapacity { get; set; }
        public string PreviewYoutubeId { get; set; }
        public List<string> Tags { get; set; }
    }
}

