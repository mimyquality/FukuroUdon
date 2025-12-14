using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRC.Core;

namespace VRC.SDKBase.Editor.Api
{
    public struct VRCAvatar: IVRCContent
    {
        public enum AvatarVariant
        {
            Standard,
            Impostor
        }

        public struct AvatarStyles
        {
            public string Primary { get; set; }
            public string Secondary { get; set; }
        }
        
        [JsonProperty("id")]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        
        public string ImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        
        public string ReleaseStatus { get; set; }
        
        public AvatarStyles Styles { get; set; }
        public bool Lock { get; set; }
        public string ActiveAssetReviewId { get; set; }
        
        public bool PendingUpload { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        public int Version { get; set; }
        public List<VRCUnityPackage> UnityPackages { get; set; }
        
        public bool Featured { get; set; }
        public string UnityPackageUrl { get; set; }
        public Dictionary<string, string> UnityPackageUrlObject { get; set; }

        public string GetLatestAssetUrlForPlatform(string platform)
        {
            string assetUrl = null;
            var preferredUnityVersion = new UnityVersion();
            if (this.UnityPackages == null) return null;
            
            foreach (var unityPackage in this.UnityPackages)
            {
                if (UnityVersion.Parse(unityPackage.UnityVersion).CompareTo(preferredUnityVersion) < 0) continue;

                if (unityPackage.Variant != null)
                {
                    AvatarVariant variant = (AvatarVariant)Enum.Parse(typeof(AvatarVariant), unityPackage.Variant, true);
                    if (variant == AvatarVariant.Impostor) continue;
                }

                if (unityPackage.Platform != platform) continue;
                assetUrl = unityPackage.AssetUrl;
                preferredUnityVersion = UnityVersion.Parse(unityPackage.UnityVersion);
            }

            return assetUrl;
        }
        
        public string GetLatestAssetUrlForCurrentPlatform()
        {
            return GetLatestAssetUrlForPlatform(Tools.Platform);
        }

        /// <summary>
        /// Checks if user-editable data is equal between avatar records
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ContentInfoEqual(VRCAvatar target)
        {
            return target.Name.Equals(Name) &&
                target.Description.Equals(Description) &&
                target.Tags.SequenceEqual(Tags) &&
                target.ReleaseStatus.Equals(ReleaseStatus);
        }
    }
    
    // Only a subset of fields is allowed to be changed through the SDK
    public struct VRCAvatarChanges {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string ReleaseStatus { get; set; }
        
        // Styles should be submitted via style ID and not the style Name
        public string PrimaryStyle { get; set; }
        public string SecondaryStyle { get; set; }
    }
}