using Newtonsoft.Json;

namespace VRC.SDKBase.Editor.Api
{
    public struct VRCAgreement
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string AgreementCode { get; set; }
        public string ContentId { get; set; }
        public string AgreementFulltext { get; set; }
        public int Version { get; set; }
        public string[] Tags { get; set; }
    }

    public struct VRCAgreementCheckRequest
    {
        public string UserId { get; set; }
        public string AgreementCode { get; set; }
        public string ContentId { get; set; }
        public int Version { get; set; }
    }
    
    public struct VRCAgreementCheckResponse
    {
        public bool Agreed { get; set; }
        public string UserId { get; set; }
        public string AgreementCode { get; set; }
        public string ContentId { get; set; }
        public int Version { get; set; }
    }
}