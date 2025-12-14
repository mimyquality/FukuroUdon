using Newtonsoft.Json;

namespace VRC.SDKBase.Editor.Api
{
    public struct VRCAvatarStyle
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        
        public string StyleName { get; set; }
    }
}