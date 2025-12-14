namespace VRC.ExampleCentral.Types.Algolia
{
    public class UnityPackage
    {
        public string AssetId { get; set; }
        public string AssetRevision { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string UnityPackageFile { get; set; }
        public string ThumbnailImage { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        [Newtonsoft.Json.JsonProperty("_tags")]
        public string[] Tags { get; set; }
        public string DocsLink { get; set; }
    }
}