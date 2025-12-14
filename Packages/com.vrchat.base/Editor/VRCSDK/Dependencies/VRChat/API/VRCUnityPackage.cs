using System;
using Newtonsoft.Json;

namespace VRC.SDKBase.Editor.Api
{
    public struct VRCUnityPackage
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string AssetUrl { get; set; }
        public string UnityVersion { get; set; }
        public long UnitySortNumber { get; set; }
        public int AssetVersion { get; set; }
        public string Platform { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        public string Variant { get; set; }
    }
}

