using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("VRC.SDK3A.Editor")]
[assembly:InternalsVisibleTo("VRC.SDK3.Editor")]
namespace VRC.SDKBase
{
    [Serializable]
    internal struct VPMProjectManifest
    {
        [Serializable]
        internal struct VPMProjectDependency
        {
            public string version { get; set; }
        }
        
        [Serializable]
        internal struct VPMProjectLockedDependency
        {
            public string version { get; set; }
            public Dictionary<string, string> dependencies { get; set; }
        }
        
        public Dictionary<string, VPMProjectDependency> dependencies { get; set; }
        public Dictionary<string, VPMProjectLockedDependency> locked { get; set; }
        
    }
}