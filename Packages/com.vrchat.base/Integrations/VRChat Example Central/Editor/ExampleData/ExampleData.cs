using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRC.ExampleCentral.Editor
{
    public enum ExampleStatus
    {
        Local,
        Draft,
        Published
    }
    
    [Serializable]
    public class ExampleData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        public string author;
        
        [SerializeField]
        public string title;
        
        [SerializeField]
        public Texture2D thumbnail;
        
        [SerializeField]
        public string description;

        [SerializeField]
        public string version;
        
        [SerializeField]
        public string documentationLink;
        
        [SerializeField]
        public string vrcWorldId;
        
        [SerializeField]
        public List<string> tags = new();
        
        [SerializeField]
        public SceneAsset exampleScene;
        
        [SerializeField]
        private List<string> serializedAssets;
        public List<AssetInfo> assets = new();
        
        [SerializeField]
        public string sanityId = $"drafts.{Guid.NewGuid().ToString()}";

        [SerializeField] 
        public ExampleStatus ExampleStatus = ExampleStatus.Local;
        
        [SerializeField]
        public List<Changes> changes = new();

        // Serialize the AssetInfo list to a list of strings
        public void OnBeforeSerialize()
        {
            serializedAssets = assets.ConvertAll(asset => asset.Serialize());
        }

        // Deserialize the List<string> back to the AssetInfo list
        public void OnAfterDeserialize()
        {
            assets = serializedAssets.ConvertAll(AssetInfo.Deserialize);
        }
    }
}