namespace VRC.SDKBase.Editor.Api
{
    public struct VRCApiError
    {
        public VRCApiErrorContent Error { get; set; }
        
        public struct VRCApiErrorContent {
            public string Message { get; set; }
            public int Code { get; set; }
        } 
    }
}