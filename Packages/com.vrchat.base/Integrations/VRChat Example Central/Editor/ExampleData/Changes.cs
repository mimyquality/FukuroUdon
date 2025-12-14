using System;

namespace VRC.ExampleCentral.Editor
{
   
    /// <summary>
    /// Stores a description of changes made to an Example
    /// </summary>
    [Serializable]
    public class Changes
    {
        /// <summary>
        /// SemVer-compatible version number (SemVer not currently enforced anywhere)
        /// </summary>
        public string Version;
        /// <summary>
        /// Plain text description of changes
        /// </summary>
        public string Value;

        public Changes(string version, string value)
        {
            Version = version;
            Value = value;
        }
    }
}