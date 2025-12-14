using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using JetBrains.Annotations;

namespace VRC.ExampleCentral.Window
{
    public static class ExampleCentralSettings
    {
        public static readonly string customSettingsPath = "VRChat/Example Central";

        [SettingsProvider]
        public static SettingsProvider CreateExampleCentralSettingsProvider()
        {
            var provider = new SettingsProvider(customSettingsPath, SettingsScope.Project)
            {
                label = "Example Central",
                
                // This method will draw the UI for the settings
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = Data.Instance;

                    var styleSheet = Resources.Load<StyleSheet>("ExampleCentralSettingsStyle");
                    rootElement.styleSheets.Add(styleSheet);

                    var container = new VisualElement()
                    {
                        name = "container",
                    };
                    rootElement.Add(container);

                    var titleLabel = new Label("Example Central Settings")
                    {
                        name = "title-label"
                    };
                    
                    container.Add(titleLabel);

                    var showCEField = new Toggle("Show Creator Economy Examples")
                    {
                        value = settings.ShowEconomyPackages,
                        name = "show-economy-packages-field"
                    };
                    showCEField.RegisterValueChangedCallback(evt =>
                    {
                        settings.ShowEconomyPackages = evt.newValue;
                        settings.Save();

                        var existingExampleCentralWindows = Resources.FindObjectsOfTypeAll<ExampleDownloaderPanel>();
                        if (existingExampleCentralWindows.Length > 0)
                        {
                            var target = existingExampleCentralWindows[0];
                            target.Refresh();
                        }
                    });
                    container.Add(showCEField);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting
                keywords = new[] { "example", "central", "vrchat", "show creator economy examples" }
            };

            return provider;
        }
        
        public static void OpenSettings()
        {
            // Open the Project Settings window with these settings selected
            SettingsService.OpenProjectSettings(customSettingsPath);
        }
        
        // Uses simple embedded Data class which saves and loads from EditorPrefs
        public class Data
        {
            public bool ShowEconomyPackages;

            private static Data _data;

            public static Data Instance
            {
                get
                {
                    // Create if needed
                    if(_data == null)
                    {
                        _data = new Data();
                        
                        // Set properties from EditorPrefs
                        _data.ShowEconomyPackages = EditorPrefs.GetBool(nameof(ShowEconomyPackages));
                    }
                    return _data;
                }
            }

            public void Save()
            {
                EditorPrefs.SetBool(nameof(ShowEconomyPackages), ShowEconomyPackages);
            }
        }
    }
}