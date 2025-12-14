using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using UnityEditor.UIElements;
using VRC.Core;
using AlgoliaPackage = VRC.ExampleCentral.Types.Algolia.UnityPackage;
using Button = UnityEngine.UIElements.Button;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace VRC.ExampleCentral.Window
{

    public class ExampleDownloaderPanel : EditorWindow
    {

        [SerializeField] private VisualTreeAsset VisualTree;

        // Visual Elements, cached after creation
        private VisualElement goToLoginView;
        private VisualElement mainView;
        private VisualElement previewThumbnail;
        private Label previewTitle;
        private Label previewTags;
        private Label previewVersion;
        private Label previewDescription;
        private Button previewDownloadButton;
        private Button previewDocsButtons;
        private ToolbarSearchField searchField;
        private VisualElement examplesScrollviewContainer;
        private Button settingsButton;

        // Searching
        private CancellationTokenSource typingCancellation;
        private int searchDelayMs = 500;

        private AlgoliaPackage selectedPackage;
        private PackageButton selectedButton;

        // Algolia Info
        public const string AlgoliaSearchKey = "60b185254097bf630e913ceaf103822a";
        public const string AlgoliaAppKey = "P787M8AJR8";
        public const string AlgoliaIndexName = "unity-packages";

        // Analytics rate-limiting
        private const int EVENT_LIMIT_EXAMPLE_PREVIEWED = 10;
        private const int TIME_LIMIT_EXAMPLE_PREVIEWED = 60;
        private int eventCounterExamplePreviewed;
        private DateTime lastResetTimeExamplePreviewed = DateTime.Now;
        
        #region Setup and Basics


        [MenuItem("VRChat SDK/🏠 Example Central", false, 980)]
        public static void ShowWindow()
        {
            ExampleDownloaderPanel window = GetWindow<ExampleDownloaderPanel>();
            window.titleContent = new GUIContent("VRChat Example Central");
            window.Show();
            
            AnalyticsSDK.ExampleCentralOpened();
        }

        private void CreateGUI()
        {
            // Set up base layout
            VisualTree.CloneTree(rootVisualElement);

            // Get top-level UI containers
            goToLoginView = rootVisualElement.Query<VisualElement>("go-to-login");
            mainView = rootVisualElement.Query<VisualElement>("split-main");
            
            // Get Auth View elements
            Button goToLoginButton = goToLoginView.Query<Button>("go-to-login-btn");

            // If the user is already logged in, enable main view
            if (APIUser.IsLoggedIn)
                OnUserLoggedIn(null, null);
            
            // Get Listing elements
            searchField = rootVisualElement.Query<ToolbarSearchField>("examples-search-field");
            searchField.RegisterValueChangedCallback(CheckSearch);

            // Get Example elements
            examplesScrollviewContainer = rootVisualElement.Query<VisualElement>("examples-scrollview-container");
            examplesScrollviewContainer.Clear();
            
            // Get Preview elements
            previewThumbnail = rootVisualElement.Query<VisualElement>("preview-thumbnail");
            previewTitle = rootVisualElement.Query<Label>("preview-title");
            previewTags = rootVisualElement.Query<Label>("preview-tags");
            previewVersion = rootVisualElement.Query<Label>("preview-version");
            previewDescription = rootVisualElement.Query<Label>("preview-description");
            previewDownloadButton = rootVisualElement.Query<Button>("preview-download");
            previewDocsButtons = rootVisualElement.Query<Button>("preview-documentation");

            previewDownloadButton.clicked += DownloadSelectedPackage;
            previewDocsButtons.clicked += OpenSelectedPackageDocs;
            goToLoginButton.clicked += OpenControlPanel;
            
            VRCSdkControlPanel.OnPanelLoggedIn += OnUserLoggedIn;
            VRCSdkControlPanel.OnPanelLoggedOut += OnUserLoggedOut;
            
            settingsButton = rootVisualElement.Query<Button>("settings-button");
            var icon = Resources.Load<Texture2D>("gear");
            settingsButton.style.backgroundImage = new StyleBackground(icon);
            settingsButton.clicked += ExampleCentralSettings.OpenSettings;

            // Set the default preview.
            DemoPreview();

            // Fetch and display UnityPackages
            UpdatePackagesAsync();
        }

        private void OnDestroy()
        {
            previewDownloadButton.clicked -= DownloadSelectedPackage;
            previewDocsButtons.clicked -= OpenSelectedPackageDocs;
            
            VRCSdkControlPanel.OnPanelLoggedIn -= OnUserLoggedIn;
            VRCSdkControlPanel.OnPanelLoggedOut -= OnUserLoggedOut;
        }

        private static void OpenControlPanel() => VRCSdkControlPanel.ShowControlPanel();
        private void OnUserLoggedIn(object _, APIUser __) => ToggleView(true);
        private void OnUserLoggedOut(object _, EventArgs __) => ToggleView(false);
        private void ToggleView(bool isUserLoggedIn)
        {
            goToLoginView.style.display = isUserLoggedIn ? DisplayStyle.None : DisplayStyle.Flex;
            mainView.style.display = isUserLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        private void OpenSelectedPackageDocs()
        {
            if (selectedPackage == null) return;
            Application.OpenURL($"{selectedPackage.DocsLink}");
        }

        private void DemoPreview()
        {
            previewTitle.text = "Select a package on the left to view and import.";
            previewVersion.text = "";
            previewTags.text = "";
            previewDescription.text = "";
            previewThumbnail.style.backgroundImage = new StyleBackground();
        }

        private void AddPackageToList(AlgoliaPackage unityPackage)
        {
            PackageButton packageButton = new PackageButton();
            packageButton.name = unityPackage.Title;
            packageButton.PackageLabel.text = unityPackage.Title;
            packageButton.clicked += () =>
            {
                if (selectedButton != null) selectedButton.Select(false);
                selectedButton = packageButton;
                selectedButton.Select(true);
                
                SelectExample(unityPackage);
            };
            
            examplesScrollviewContainer.Add(packageButton);
        }

        private void SelectExample(AlgoliaPackage unityPackage)
        {
            if (selectedPackage == unityPackage)
            {
                return;
            }
            
            selectedPackage = unityPackage;

            previewTitle.text = unityPackage.Title;
            previewVersion.text = $"Version: {unityPackage.Version}";
            previewTags.text = $"Tags: {string.Join(',', unityPackage.Tags)}";
            previewDescription.text = unityPackage.Description;

            string thumbnailPath = DownloadThumbnail(unityPackage.ThumbnailImage);
            previewThumbnail.style.backgroundImage = new StyleBackground(LoadTextureFromDisk(thumbnailPath));
            
            // rate-limit event tracking for previewing examples to at most 10 events per minute
            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - lastResetTimeExamplePreviewed;
            if (elapsedTime.TotalSeconds > TIME_LIMIT_EXAMPLE_PREVIEWED)
            {
                eventCounterExamplePreviewed = 0;
                lastResetTimeExamplePreviewed = currentTime;
            }

            if (eventCounterExamplePreviewed < EVENT_LIMIT_EXAMPLE_PREVIEWED)
            {
                eventCounterExamplePreviewed++;
                AnalyticsSDK.ExamplePreviewed(selectedPackage.Title, selectedPackage.Version);
            }
        }

        #endregion

        #region Search the Index for Packages

        private void UpdatePackagesAsync(string query = "")
        {
            ShowPackagesForQuery(query);
        }

        public void Refresh()
        {
            ShowPackagesForQuery(searchField.value);
        }

        private void ToggleSearchedButtons(string searchKey)
        {
            if (string.IsNullOrEmpty(searchKey) || string.IsNullOrWhiteSpace(searchKey))
            {
                foreach (VisualElement child in examplesScrollviewContainer.Children())
                {
                    child.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                foreach (VisualElement child in examplesScrollviewContainer.Children())
                {
                    bool shown = child.name.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase);
                    child.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        private void CheckSearch(ChangeEvent<string> evt)
        {
            ToggleSearchedButtons(evt.newValue);

            if (typingCancellation != null)
            {
                typingCancellation.Cancel();
                typingCancellation.Dispose();
            }

            typingCancellation = new CancellationTokenSource();
            DebounceSearch(typingCancellation.Token, evt.newValue);
        }

        private async Task DebounceSearch(CancellationToken token, string finalText)
        {
            try
            {
                await Task.Delay(searchDelayMs, cancellationToken: token);
                if (!token.IsCancellationRequested)
                {
                    // User has finished typing
                    UpdatePackagesAsync(finalText);
                }
            }
            catch (OperationCanceledException)
            {
                // This block is executed if the delay is cancelled
            }
        }

        private async Task<List<AlgoliaPackage>> FetchPackagesAlgolia(string query = "")
        {
            SearchClient algolia = new SearchClient(AlgoliaAppKey, AlgoliaSearchKey);
            SearchIndex index = algolia.InitIndex(AlgoliaIndexName);
            
            // Construct the embedded list format needed for tag filters
            var tagFilters = new List<List<string>>();
            
            if (!ExampleCentralSettings.Data.Instance.ShowEconomyPackages)
            {
                tagFilters.Add(new List<string>(){"-ce"});
            }
            
            // Conduct the search, including tag filters from settings
            List<AlgoliaPackage> hits = (await index.SearchAsync<ExampleCentral.Types.Algolia.UnityPackage>(
                new Query(query)
                {
                    TagFilters = tagFilters
                }
            )).Hits;
            
            return hits;
        }

        private async Task ShowPackagesForQuery(string query = "")
        {
            List<AlgoliaPackage> packages = await FetchPackagesAlgolia(query);
            // Clear the existing list
            examplesScrollviewContainer.Clear();

            foreach (AlgoliaPackage unityPackage in packages)
            {
                // Add each package to the list
                AddPackageToList(unityPackage);
            }

            ToggleSearchedButtons(query);
        }

        #endregion

        #region Downloads

        private void DownloadSelectedPackage()
        {
            if (selectedPackage == null) return;
            AnalyticsSDK.ExampleDownloaded(selectedPackage.Title, selectedPackage.Version);
            DownloadUnityPackage(selectedPackage);
        }

        private string DownloadThumbnail(string url)
        {
            // get or create path to store thumbnails, use Local App Data
            string thumbnailDir =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VRChat",
                    "Example Central", "Thumbnails");
            Directory.CreateDirectory(thumbnailDir);
            string thumbnailPath = Path.Combine(thumbnailDir, Path.GetFileName(url));
            // download thumbnail if it doesn't exist
            if (!File.Exists(thumbnailPath))
            {
                WebClient client = new System.Net.WebClient();
                client.DownloadFile(url, thumbnailPath);
                return thumbnailPath;
            }
            else
            {
                return thumbnailPath;
            }
        }

        private void DownloadUnityPackage(AlgoliaPackage unityPackage)
        {
            string packageDownloadDir =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VRChat",
                    "Example Central", "Packages");
            Directory.CreateDirectory(packageDownloadDir);
            string packageDownloadPath = Path.Combine(packageDownloadDir,
                $"{unityPackage.Title}-{unityPackage.Version}.unitypackage");
            // download file if it doesn't exist
            if (!File.Exists(packageDownloadPath))
            {
                WebClient client = new System.Net.WebClient();
                client.DownloadFile(unityPackage.UnityPackageFile, packageDownloadPath);
            }
            
            // import the package
            AssetDatabase.ImportPackage(packageDownloadPath, true);
        }

        private Texture2D LoadTextureFromDisk(string path)
        {
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(path);
            texture.LoadImage(fileData);
            return texture;
        }

        #endregion
    }
}