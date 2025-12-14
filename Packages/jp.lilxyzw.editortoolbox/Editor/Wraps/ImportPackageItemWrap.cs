using System;
using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal class ImportPackageItemWrap : WrapBase
    {
        internal static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.ImportPackageItem");
        private static readonly (Delegate g, Delegate s) FI_existingAssetPath = GetFieldIns(type, "existingAssetPath", typeof(string));
        private static readonly (Delegate g, Delegate s) FI_destinationAssetPath = GetFieldIns(type, "destinationAssetPath", typeof(string));
        private static readonly (Delegate g, Delegate s) FI_isFolder = GetFieldIns(type, "isFolder", typeof(bool));
        private static readonly (Delegate g, Delegate s) FI_assetChanged = GetFieldIns(type, "assetChanged", typeof(bool));
        private static readonly (Delegate g, Delegate s) FI_guid = GetFieldIns(type, "guid", typeof(string));

        public object instance;
        public ImportPackageItemWrap(object i) => instance = i;

        public string existingAssetPath => FI_existingAssetPath.g.DynamicInvoke(instance) as string;
        public bool isFolder => (bool)FI_isFolder.g.DynamicInvoke(instance);
        public string guid => FI_guid.g.DynamicInvoke(instance) as string;
        public string destinationAssetPath
        {
            get => FI_destinationAssetPath.g.DynamicInvoke(instance) as string;
            set => FI_destinationAssetPath.s.DynamicInvoke(instance, value);
        }
        public bool assetChanged
        {
            get => (bool)FI_assetChanged.g.DynamicInvoke(instance);
            set => FI_assetChanged.s.DynamicInvoke(instance, value);
        }

    }
}
