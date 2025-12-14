using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    [FilePath("ProjectSettings/jp.lilxyzw.editortoolbox.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class EditorToolboxSettingsProject : ScriptableSingleton<EditorToolboxSettingsProject>
    {
        public string settingPreset;
        internal static void Save() => instance.Save(true);
    }
}
