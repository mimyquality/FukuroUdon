using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRC.Core;

namespace VRC.SDKBase.Editor
{
    public class VRCAnalyticsTools
    {
        internal static List<AnalyticsSDK.PackageEntry> GetPackageList()
        {
            var packageList = new List<AnalyticsSDK.PackageEntry>();
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "vpm-manifest.json");
            try
            {
                if (File.Exists(manifestPath))
                {
                    var manifestJson = File.ReadAllText(manifestPath);
                    var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<VPMProjectManifest>(manifestJson);

                    if (manifest.locked != null)
                    {
                        foreach (var locked in manifest.locked)
                        {
                            packageList.Add(new AnalyticsSDK.PackageEntry
                            {
                                packageId = locked.Key,
                                packageVersion = locked.Value.version
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to get package list from vpm-manifest.json");
                Debug.LogException(e);
            }
            
            return packageList;
        }
    }
}