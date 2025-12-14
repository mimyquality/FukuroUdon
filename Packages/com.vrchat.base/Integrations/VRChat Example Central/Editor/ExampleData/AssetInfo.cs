using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRC.ExampleCentral.Editor
{
    /// <summary>
    /// Stores the GUID and Hash for each Asset in an Example
    /// </summary>
    [Serializable]
    public class AssetInfo
    {
        public string GUID { get; private set; }
        public string Hash { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetInfo"/> class with the specified GUID and hash.
        /// Used for deserialization, after the hash is computed.
        /// </summary>
        /// <param name="guid">The GUID of the asset.</param>
        /// <param name="hash">The hash of the asset.</param>
        public AssetInfo(string guid, string hash)
        {
            GUID = guid;
            Hash = hash;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetInfo"/> class with the specified GUID.
        /// The hash is computed based on the asset's content.
        /// </summary>
        /// <param name="guid">The GUID of the asset.</param>
        public AssetInfo(string guid)
        {
            GUID = guid;
            Hash = ComputeAssetHash(guid);
        }

        /// <summary>
        /// Computes the hash of the asset based on its content.
        /// </summary>
        /// <param name="guid">The GUID of the asset.</param>
        /// <returns>The computed hash of the asset.</returns>
        private string ComputeAssetHash(string guid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            if (!File.Exists(fullPath))
                return "File Not Found";

            try
            {
                using FileStream stream = File.OpenRead(fullPath);
                using SHA256 sha = SHA256.Create();
                byte[] hashBytes = sha.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error computing hash for {assetPath}: {ex.Message}");
                return "Error";
            }
        }

        /// <summary>
        /// Serializes the asset to a string, using a pipe character as a separator.
        /// </summary>
        /// <returns>A string representation of the asset's guid and hash.</returns>
        public string Serialize()
        {
            return $"{GUID}|{Hash}";
        }

        /// <summary>
        /// Deserializes the asset information from a string.
        /// </summary>
        /// <param name="serializedData">The serialized data string.</param>
        /// <returns>An instance of <see cref="AssetInfo"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the serialized data format is invalid.</exception>
        public static AssetInfo Deserialize(string serializedData)
        {
            var parts = serializedData.Split('|');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid serialized data format");

            return new AssetInfo(parts[0], parts[1]);
        }
    }
}