using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Settings",
        "Settings related to lilEditorToolbox. You can open it from `Edit/Preference/lilEditorToolbox` on the menu bar, where you can change the language and enable various functions."
    )]
    internal class EditorToolboxSettings : ScriptableObject
    {
        [Header("Language")]
        [Tooltip("The language setting for lilEditorToolbox. The language file exists in `jp.lilxyzw.editortoolbox/Editor/Localization`, and you can support other languages by creating a language file.")]
        public string language = CultureInfo.CurrentCulture.Name;

        [L10nHeader("Asset Import")]
        [Tooltip("When importing assets via D&D, if there is a file with the same name at the same level, it will be overwritten by the import.")]
        [ToggleLeft] public bool dragAndDropOverwrite = false;
        [Tooltip("Prevents unitypackage from overwriting assets under Packages.")]
        [ToggleLeft] public bool cancelUnitypackageOverwriteInPackages = false;
        [Tooltip("Do not add \"Variant\" to the end of the name when creating a Prefab Variant.")]
        [ToggleLeft] public bool doNotAddVariantToTheEndOfPrefabName = false;
        [Tooltip("Add a button to change the import directory in the unitypackage import window.")]
        [ToggleLeft] public bool addUnitypackageDirectorySelectionMenu = false;
        [Tooltip("Add a warning if the unitypackage contains scripts.")]
        [ToggleLeft] public bool addUnitypackageContainsScriptWarning = false;

        [L10nHeader("Texture Import")]
        [Tooltip("Automatically turn off Crunch Compression when importing textures to speed up imports.")]
        [ToggleLeft] public bool turnOffCrunchCompression = false;
        [Tooltip("Automatically turn on `Streaming Mipmaps` when importing textures.")]
        [ToggleLeft] public bool turnOnStreamingMipmaps = false;
        [Tooltip("Automatically change to `Kaiser` when importing texture.")]
        [ToggleLeft] public bool changeToKaiserMipmaps = false;

        [L10nHeader("Model Import")]
        [Tooltip("Automatically turn on `Readable` when importing a model.")]
        [ToggleLeft] public bool turnOnReadable = false;
        [Tooltip("Turn off `Legacy Blend Shape Normals` when importing a model to turn off automatic recalculation of BlendShape normals.")]
        [ToggleLeft] public bool fixBlendshapes = false;
        [Tooltip("When importing a model, if a bone that does not contain `jaw` (case insensitive) in the bone name is assigned to the Humanoid Jaw, it will be automatically unassigned.")]
        [ToggleLeft] public bool removeJaw = false;

        [L10nHeader("Animator Controller Editor")]
        [Tooltip("Change the default Layer Weight value to 1 when creating a new layer.")]
        [ToggleLeft] public bool defaultLayerWeight1 = false;
        [Tooltip("Change the default value of Write Defaults when creating a new state to off.")]
        [ToggleLeft] public bool defaultWriteDefaultsOff = false;
        [Tooltip("Change the default value of Has Exit Time when creating a new transition to off.")]
        [ToggleLeft] public bool defaultHasExitTimeOff = false;
        [Tooltip("Change the default value of Exit Time when creating a new transition to 1.")]
        [ToggleLeft] public bool defaultExitTime1 = false;
        [Tooltip("Change the default value of Duration when creating a new transition to 0.")]
        [ToggleLeft] public bool defaultDuration0 = false;
        [Tooltip("Change the default value of Can Transition Self when creating a new transition to off.")]
        [ToggleLeft] public bool defaultCanTransitionSelfOff = false;

        [Tooltip("The Animator Controller parameter type has been expanded to allow it to be changed, and a decimal point will be displayed if the type is Float, making it easier to distinguish from Int types.")]
        [ToggleLeft] public bool extendAnimatorControllerParameterGUI = false;
        [Tooltip("Add copy and paste to the right-click menu of the Animator Controller layer.")]
        [ToggleLeft] public bool addCopyAndPasteLayerMenu = false;
        [Tooltip("Add copy and paste settings to the transition right-click menu.")]
        [ToggleLeft] public bool addCopyTransitionSettingsMenu = false;
        [Tooltip("Add a bulk selection menu for entering and exiting transitions to the state right-click menu.")]
        [ToggleLeft] public bool addSelectInOutTransitionsMenu = false;
        [Tooltip("Add a menu to the right-click menu of parameters that displays the referenced transition state.")]
        [ToggleLeft] public bool addParameterReferencesMenu = false;
        [Tooltip("Make transition when you double-click a state.")]
        [ToggleLeft] public bool makeTransitionWithDoubleClick = false;
        [Tooltip("Fix a Unity bug where transition Interruption settings were not copied.")]
        [ToggleLeft] public bool fixCopyInterruptionSettings = false;
        [Tooltip("When changing a parameter in the transition condition settings, if the destination is an Int type, the default value will be Equals.")]
        [ToggleLeft] public bool fixTransitionConditionGUI = false;
        [Tooltip("Fix a Unity bug that caused some properties to be overwritten when editing multiple states at the same time.")]
        [ToggleLeft] public bool fixStateMultipleEdit = false;

        [L10nHeader("Hierarchy Extension", "You can display objects, components, tags, layers, etc. on the Hierarchy. You can also add your own extensions by implementing `IHierarchyExtensionConponent`. Please refer to the scripts under `Editor/HierarchyExtension/Components` for how to write them.")]
        [Tooltip("The width of the margin to avoid interfering with other Hierarchy extensions.")]
        public int hierarchySpacerWidth = 0;
        [Tooltip("This is the timing to insert margins so as not to interfere with other Hierarchy extensions.")]
        public int hierarchySpacerPriority = 0;
        [Tooltip("This is the mouse button that the hierarchy window extension that supports this property will respond to. If you make many erroneous operations, please change the button.")]
        public MouseButton hierarchyMouseButton = MouseButton.Left;
        [Tooltip("Displays Layer and Tag side by side.")]
        public bool hierarchyLayerAndTagSideBySide = false;
        [DocsGetStrings(typeof(HierarchyExtension), "GetNameAndTooltips")]
        public string[] hierarchyComponents = new string[]{};

        [L10nHeader("Project Extension", "You can display extensions and prefab information on the project. You can also add your own extensions by implementing `IProjectExtensionConponent`. Please refer to the script under `Editor/ProjectExtension/Components` for how to write it.")]
        [DocsGetStrings(typeof(ProjectExtension), "GetNameAndTooltips")]
        public string[] projectComponents = new string[]{};

        [L10nHeader("Toolbar Extension", "You can display an assembly lock button or an extension inspector tab on the Toolbar. You can also add your own extension by implementing `IToolbarExtensionComponent`. Please refer to the script under `Editor/ToolbarExtension/Components` for how to write it.")]
        [DocsGetStrings(typeof(ToolbarExtension), "GetNameAndTooltips")]
        public string[] toolbarComponents = new string[]{};

        [L10nHeader("Menu Directory Replaces", "You can customize the menu bar by changing or deleting the menu hierarchy. You can also edit multiple menus at once by specifying `Tools/*`.")]
        [L10nHelpBox("Some menu items may not be compatible with replacement.", MessageType.Warning)]
        [Tooltip("When this check box is selected, changing and deleting the menu hierarchy is enabled.")] [ToggleLeft] public bool enableMenuDirectoryReplaces = false;
        [Tooltip("Add the menu to be changed here. If To is empty, the menu will be deleted, if it is not empty, it will be moved to that hierarchy.")] public MenuReplace[] menuDirectoryReplaces = new MenuReplace[]{};

        [HideInInspector] public bool enableMSAA = false;

        internal readonly Color backgroundColor = new Color(0.5f,0.5f,0.5f,0.05f);
        internal readonly Color lineColor = new Color(0.5f,0.5f,0.5f,0.5f);
        internal readonly Color backgroundHilightColor = new Color(1.0f,0.95f,0.5f,0.2f);

        private static EditorToolboxSettings s_Instance;
        internal static EditorToolboxSettings instance
        {
            get
            {
                if (!s_Instance)
                {
                    InternalEditorUtility.LoadSerializedFileAndForget(GetFilePath());
                    if (!s_Instance)
                    {
                        s_Instance = CreateInstance<EditorToolboxSettings>();
                        s_Instance.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                    }
                }
                return s_Instance;
            }
        }

        internal static void SetReload()
        {
            if(File.Exists(GetFilePath())) s_Instance = null;
        }

        internal static void Save()
        {
            if (!s_Instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            var filePath = GetFilePath();
            var directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            InternalEditorUtility.SaveToSerializedFileAndForget(new[]{ s_Instance }, filePath, true);
        }

        private EditorToolboxSettings()
        {
            if (!s_Instance) s_Instance = this;
        }

        private static string GetFilePath()
        {
            if(string.IsNullOrEmpty(EditorToolboxSettingsProject.instance.settingPreset))
                return InternalEditorUtility.unityPreferencesFolder + "/jp.lilxyzw/editortoolbox.asset";
            return InternalEditorUtility.unityPreferencesFolder + "/jp.lilxyzw/editortoolbox_" + EditorToolboxSettingsProject.instance.settingPreset + ".asset";
        }
    }

    [Flags]
    internal enum MouseButton
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Middle = 1 << 2,
    }

    [CustomEditor(typeof(EditorToolboxSettings))]
    public class EditorToolboxSettingsEditor : Editor
    {
        public static Action update;
        private static SerializedObject serializedObjectProject;
        private static new SerializedObject serializedObject;
        private static string[] settingPresets;

        private class SettingPresetWIndow : PopupWindowContent
        {
            private string settingNewPreset;

            public override Vector2 GetWindowSize()
            {
                return new Vector2(400, 20);
            }

            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.BeginHorizontal();
                settingNewPreset = L10n.TextField("New Preset", settingNewPreset);
                if (L10n.ButtonLimited("Add"))
                {
                    settingPresets = null;
                    var invalidChars = Path.GetInvalidFileNameChars();
                    serializedObjectProject.FindProperty("settingPreset").stringValue = string.Concat(settingNewPreset.Where(c => !invalidChars.Contains(c)));
                    serializedObjectProject.ApplyModifiedProperties();
                    EditorToolboxSettingsProject.Save();
                    EditorToolboxSettings.SetReload();
                    EditorToolboxSettings.Save();
                    serializedObject = null;
                    Reload();
                    editorWindow.Close();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            // プロジェクトごとの設定
            EditorGUILayout.Space();
            L10n.LabelField("Project Settings", EditorStyles.boldLabel);
            if (serializedObjectProject == null) serializedObjectProject = new(EditorToolboxSettingsProject.instance);
            serializedObjectProject.UpdateIfRequiredOrScript();
            var iteratorProject = serializedObjectProject.GetIterator();
            iteratorProject.NextVisible(true); // m_Script
            while (iteratorProject.NextVisible(false))
            {
                if (iteratorProject.name == "settingPreset")
                {
                    EditorGUI.BeginChangeCheck();
                    settingPresets ??= new[] { "(Default)" }.Union(Directory.GetFiles(InternalEditorUtility.unityPreferencesFolder + "/jp.lilxyzw")
                        .Select(p => Path.GetFileNameWithoutExtension(p))
                        .Where(f => f.StartsWith("editortoolbox_"))
                        .Select(f => f.Substring("editortoolbox_".Length))).Union(new[] { L10n.L("New Preset") }).ToArray();
                    var id = Array.IndexOf(settingPresets, iteratorProject.stringValue);
                    if (string.IsNullOrEmpty(iteratorProject.stringValue)) id = 0;
                    id = EditorGUILayout.Popup(L10n.G(iteratorProject.displayName, iteratorProject.tooltip), id, settingPresets);
                    if (EditorGUI.EndChangeCheck() && id != -1)
                    {
                        if (id == settingPresets.Length - 1) PopupWindow.Show(GUILayoutUtility.GetLastRect(), new SettingPresetWIndow());
                        else iteratorProject.stringValue = id == 0 ? "" : settingPresets[id];
                    }

                }
                else
                {
                    L10n.PropertyField(iteratorProject);
                }
            }

            if (serializedObjectProject.ApplyModifiedProperties())
            {
                EditorToolboxSettingsProject.Save();
                EditorToolboxSettings.SetReload();
                serializedObject = null;
                Reload();
            }

            if (L10n.ButtonLimited("Open preference folder"))
            {
                System.Diagnostics.Process.Start(InternalEditorUtility.unityPreferencesFolder + "/jp.lilxyzw");
            }

            // 共通設定
            if (serializedObject == null) serializedObject = new(EditorToolboxSettings.instance);
            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // m_Script
            void StringListAsToggle((string[] key, string fullname)[] names)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var vals = Enumerable.Range(0, iterator.arraySize).Select(i => iterator.GetArrayElementAtIndex(i).stringValue).ToList();
                foreach (var name in names)
                {
                    var contains = vals.Contains(name.fullname);
                    string label = L10n.L(name.key[0]);
                    if (!name.fullname.StartsWith("jp.lilxyzw.editortoolbox")) label = $"{label} ({name.fullname})";
                    var toggle = EditorGUILayout.ToggleLeft(L10n.G(label, name.key[1]), contains);
                    if (contains != toggle)
                    {
                        if (toggle)
                        {
                            iterator.InsertArrayElementAtIndex(iterator.arraySize);
                            iterator.GetArrayElementAtIndex(iterator.arraySize - 1).stringValue = name.fullname;
                        }
                        else
                        {
                            iterator.DeleteArrayElementAtIndex(vals.IndexOf(name.fullname));
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }

            while (iterator.NextVisible(false))
            {
                if (iterator.name == "hierarchyComponents")
                {
                    EditorGUILayout.Space();
                    StringListAsToggle(HierarchyExtension.names);
                }
                else if (iterator.name == "projectComponents")
                {
                    EditorGUILayout.Space();
                    L10n.LabelField("Project Extension", EditorStyles.boldLabel);
                    StringListAsToggle(ProjectExtension.names);
                }
                else if (iterator.name == "toolbarComponents")
                {
                    EditorGUILayout.Space();
                    L10n.LabelField("Toolbar Extension", EditorStyles.boldLabel);
                    StringListAsToggle(ToolbarExtension.names);
                }
                else if (iterator.name == "language")
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);

                    var langs = L10n.GetLanguages();
                    var names = L10n.GetLanguageNames();
                    EditorGUI.BeginChangeCheck();
                    var ind = EditorGUILayout.Popup("Language", Array.IndexOf(langs, iterator.stringValue), names);
                    if (EditorGUI.EndChangeCheck()) iterator.stringValue = langs[ind];
                }
                else
                {
                    L10n.PropertyField(iterator);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                L10n.Load();
                EditorToolboxSettings.Save();
                Reload();
            }
        }

        private static void Reload()
        {
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();
            update.Invoke();
        }
    }

    internal class EditorToolboxSettingsProvider : EasySettingProvider
    {
        public EditorToolboxSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords){}
        public override ScriptableObject SO => EditorToolboxSettings.instance;
        [SettingsProvider] public static SettingsProvider Create() => Create("lilEditorToolbox");
    }
}
