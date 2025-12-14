using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using JetBrains.Annotations;

namespace VRC.SDKBase.Editor
{
    public static class VRCSDKSettingsProvider
    {
        public static readonly string customSettingsPath = "VRChat/SDK";

        [SettingsProvider]
        public static SettingsProvider CreateVRCSDKSettingsProvider()
        {
            var provider = new SettingsProvider(customSettingsPath, SettingsScope.Project)
            {
                label = "SDK",
                
                // This method will draw the UI for the settings
                activateHandler = (searchContext, rootElement) =>
                {
                    var styleSheet = Resources.Load<StyleSheet>("VRCSDKSettingsStyle");
                    rootElement.styleSheets.Add(styleSheet);

                    var container = new VisualElement()
                    {
                        name = "container",
                    };
                    rootElement.Add(container);

                    var titleLabel = new Label("VRChat SDK Settings")
                    {
                        name = "title-label"
                    };
                    
                    container.Add(titleLabel);

                    var androidAppNameField = new UnityEngine.UIElements.TextField("Android App Package Name")
                    {
                        value = VRCSettings.AndroidPackageName,
                        name = "android-app-package-name-field"
                    };
                    androidAppNameField.RegisterValueChangedCallback(evt =>
                    {
                        VRCSettings.AndroidPackageName = evt.newValue;
                    });
                    container.Add(androidAppNameField);
                    
                    var questAppNameField = new UnityEngine.UIElements.TextField("Quest App Package Name")
                    {
                        value = VRCSettings.QuestPackageName,
                        name = "quest-app-package-name-field"
                    };
                    questAppNameField.RegisterValueChangedCallback(evt =>
                    {
                        VRCSettings.QuestPackageName = evt.newValue;
                    });
                    container.Add(questAppNameField);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting
                keywords = new[] { "vrchat", "sdk", "mobile", "android" }
            };

            return provider;
        }
        
        public static void OpenSettings()
        {
            // Open the Project Settings window with these settings selected
            SettingsService.OpenProjectSettings(customSettingsPath);
        }
    }
}