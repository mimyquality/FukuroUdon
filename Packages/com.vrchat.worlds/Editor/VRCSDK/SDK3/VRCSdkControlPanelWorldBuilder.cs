
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.Editor;
using VRC.SDK3.Editor;
using VRC.SDK3.Editor.Elements;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Editor.Elements;
using VRC.SDKBase.Editor.Validation;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Object = UnityEngine.Object;
using VRC.SDK3A.Editor.Elements;
using UnityEngine.Rendering.PostProcessing;
using VRC.Dynamics;
using VRC.SDK3.Editor.Exceptions;
using Button = UnityEngine.UIElements.Button;
using Progress = UnityEditor.Progress;
using Toggle = UnityEngine.UIElements.Toggle;

[assembly: VRCSdkControlPanelBuilder(typeof(VRCSdkControlPanelWorldBuilder))]
namespace VRC.SDK3.Editor
{
    public class VRCSdkControlPanelWorldBuilder : IVRCSdkWorldBuilderApi
    {
        private static Type _postProcessVolumeType; 
        protected VRCSdkControlPanel _builder;
        protected VRC_SceneDescriptor[] _scenes;

#if UNITY_ANDROID || UNITY_IOS
        private readonly List<GameObject> _rootGameObjectsBuffer = new List<GameObject>();
#endif

        private const string COMMUNITY_LABS_HELP_URL =
            "https://creators.vrchat.com/worlds/submitting-a-world-to-be-made-public/#submitting-to-community-labs";
        private readonly string[] COMMUNITY_LABS_BLOCKED_TAGS = {"admin_approved", "system_labs", "admin_lock_labs", "system_troll"};
        private const int WORLD_MAX_CAPAITY = 80;
        private const float WORLD_MAX_SPAWN_OFFSET = 1000f;
        private const string WORLD_SPAWN_EMPTY_ERROR =
            "You must add at least one valid spawn point to the spawns list in your scene descriptor.";
        private const string WORLD_SPAWN_OFFSET_WARNING =
            "The spawn position is too far from the World Origin: {0}. This can cause issues with Menus and PhysBones.";
        private const string WORLD_MULTI_SPAWN_OFFSET_WARNING =
            "One of the spawn positions is too far from the World Origin: {0}. This can cause issues with Menus and PhysBones.";
        private const string WORLD_SPAWN_BELOW_RESPAWN_PLANE_WARNING =
            "The spawn position at {0} is below the respawn height ({1}). The spawn position won't work properly.";
        private const string WORLD_MULTI_SPAWN_BELOW_RESPAWN_PLANE_WARNING =
            "A spawn position at {0} is below the respawn height ({1}). The spawn position won't work properly.";

        #region Main Interface Methods
        
        private bool _initialized;
        // Performs any first-time initialization tasks
        // This is called when the builder is mounted to the SDK panel
        public virtual void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

            if (VRCMultiPlatformBuild.ShouldContinueMPB(out var isMPBFinished))
            {
                // React to content loading to start a build
                ContentInfoLoaded += StartMultiPlatformBuild;
            }

            if (!isMPBFinished) return;
            
            // Show a nice notification when we're all done
            var worldId = _scenes?[0]?.gameObject?.GetComponent<PipelineManager>()?.blueprintId ?? "";
            _builder.ShowBuilderNotification(
                "Multi-Platform Upload Finished",
                new WorldUploadSuccessNotification(worldId),
                "green"
            ).ConfigureAwait(false);
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            _pipelineManagers = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            var blueprintId = "";
            if ((_pipelineManagers?.Length ?? 0) > 0)
            {
                blueprintId = _pipelineManagers[0].blueprintId;
            }
            
            // Invoke existing blueprint change logic to immediately adjust the UI
            if (!string.IsNullOrWhiteSpace(blueprintId))
            {
                CheckBlueprintChanges();
                return;
            }
			// Revalidate
            OnShouldRevalidate?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Gets called by "you must add a scene descriptor" SDK Panel code
        public virtual void SelectAllComponents()
        {
            List<Object> show = new List<Object>(Selection.objects);
            foreach (VRC_SceneDescriptor s in _scenes)
                show.Add(s.gameObject);
            Selection.objects = show.ToArray();
        }

        public virtual void ShowSettingsOptions()
        {
            // Draw GUI inside the Settings tab
        }

        private Texture2D _headerImage;

        public Texture2D GetHeaderImage()
        {
            if (_headerImage != null)
            {
                return _headerImage;
            }
            _headerImage = Resources.Load<Texture2D>("SDK_Banner_CreateWorld");
            return _headerImage;
        }

        public virtual bool IsValidBuilder(out string message)
        {
            if (VRC.SDKBase.Editor.V3.V3SdkUI.V3Enabled())
            {
                message = "Multiple pipelines are present. V3 pipeline will take priority";
                return false;
            }
            FindScenes();
            message = null;
            _pipelineManagers = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (_pipelineManagers.Length > 1)
            {
                message = "Multiple Pipeline Managers found in scene. Please remove all but one.";
                return false;
            } 
            if (_scenes != null && _scenes.Length > 0) return true;
            message = "A VRCSceneDescriptor or VRCAvatarDescriptor\nis required to build VRChat SDK Content";
            return false;
        }

        protected void FindScenes()
        {
            VRC_SceneDescriptor[] newScenes = Tools.FindSceneObjectsOfTypeAll<VRC_SceneDescriptor>();

            if (_scenes != null)
            {
                foreach (VRC_SceneDescriptor s in newScenes)
                    if (_scenes.Contains(s) == false)
                        _builder.CheckedForIssues = false;
            }

            _scenes = newScenes;
        }

        public void CreateValidationsGUI(VisualElement root)
        {
            // If we're building - skip performing extra validations
            if (BuildPipeline.isBuildingPlayer)
            {
                root.Clear();
                var message = new Label("Building – Please Wait ...");
                message.AddToClassList("m-4");
                return;
            }

            // Attempt to find the scenes once
            if (_scenes[0] == null)
            {
                FindScenes();
            }
            
            if (_scenes[0] == null)
            {
                root.Clear();
                return;
            }
            
            var failedBehaviours = ShouldShowPrimitivesWarning();
            if (failedBehaviours.Count > 0)
            {
                _builder.OnGUIWarning(null,
                    "Udon Objects reference builtin Unity mesh assets, this won't work. Consider making a copy of the mesh to use instead.",
                    () =>
                    {
                        Selection.objects = failedBehaviours.Select(s => s.gameObject).Cast<Object>().ToArray();
                    }, FixPrimitivesWarning);
            }
            
            if (_postProcessVolumeType != null)
            {
                if (Camera.main != null && Camera.main.GetComponentInChildren(_postProcessVolumeType))
                {
                    _builder.OnGUIWarning(null,
                        "Scene has a PostProcessVolume on the Reference Camera (Main Camera). This Camera is disabled at runtime. Please move the PostProcessVolume to another GameObject.",
                        () => { Selection.activeGameObject = Camera.main.gameObject; },
                        TryMovePostProcessVolumeAwayFromMainCamera
                    );
                }
            }
            
            root.Clear();
            
            if (_scenes.Length > 1)
            {
                _scenes = _scenes.Where(s => s != null).ToArray();
                var gos = new Object[_scenes.Length];
                for (int i = 0; i < _scenes.Length; ++i)
                { 
                    gos[i] = _scenes[i].gameObject;
                }
                _builder.OnGUIError(null,
                    "A Unity scene containing a VRChat Scene Descriptor should only contain one Scene Descriptor.",
                    () => { Selection.objects = gos; }, null);
                    
                root.Add(_builder.CreateIssuesGUI());
                return;
            }

            if (_scenes.Length != 1) return;
            
            if (CreateSceneSetupGUI(out var setupElement))
            {
                root.Add(setupElement);
                root.Add(_builder.CreateFixIssuesToBuildOrTestGUI());
                return;
            }

            _builder.ResetIssues();
            VRC_EditorTools.GetCheckProjectSetupMethod()?.Invoke(_builder, new object[] {});
            OnGUISceneCheck(_scenes[0]);
            _builder.CheckedForIssues = true;
                        
            if (_builder.NoGuiErrorsOrIssuesForItem(_scenes[0]) &&
                _builder.NoGuiErrorsOrIssuesForItem(_builder))
            {
                _builder.OnGUIInformation(_scenes[0], "Everything looks good");
            }
            
            // Show general issues
            root.Add(_builder.CreateIssuesGUI());
            // Show scene-related issues
            root.Add(_builder.CreateIssuesGUI(_scenes[0]));
        }
        
        public EventHandler OnContentChanged { get; set; }
        public EventHandler OnShouldRevalidate { get; set; }

        public virtual void RegisterBuilder(VRCSdkControlPanel baseBuilder)
        {
            _builder = baseBuilder;
        }
        
        #endregion

        #region World Validations (IMGUI)
        
        private void TryMovePostProcessVolumeAwayFromMainCamera()
        {
            if (Camera.main == null)
                return;
            if (_postProcessVolumeType == null)
                return;
            Component oldVolume = Camera.main.GetComponentInChildren(_postProcessVolumeType);
            if (!oldVolume)
                return;
            GameObject oldObject = oldVolume.gameObject;
            GameObject newObject = Object.Instantiate(oldObject);
            newObject.name = "Post Processing Volume";
            newObject.tag = "Untagged";
            foreach (Transform child in newObject.transform)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            var newVolume = newObject.GetComponentInChildren(_postProcessVolumeType);
            foreach (Component c in newObject.GetComponents<Component>())
            {
                if ((c == newObject.transform) || (c == newVolume))
                    continue;
                Object.DestroyImmediate(c);
            }

            Object.DestroyImmediate(oldVolume);
            _builder.Repaint();
            Selection.activeGameObject = newObject;
        }

        [UnityEditor.Callbacks.DidReloadScripts(int.MaxValue)]
        static void DidReloadScripts()
        {
            DetectPostProcessingPackage();
        }

        static void DetectPostProcessingPackage()
        {
            _postProcessVolumeType = null;
            try
            {
                System.Reflection.Assembly
                    postProcAss = System.Reflection.Assembly.Load("Unity.PostProcessing.Runtime");
                _postProcessVolumeType = postProcAss.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume");
            }
            catch
            {
                // -> post processing not installed
            }
        }

        private VisualElement CreateSceneSetupMessageGUI(string label, string message,
            string buttonText = null,
            Action buttonAction = null)
        {
            var helpBox = new HelpBox(label, HelpBoxMessageType.None);
            helpBox.AddToClassList("col");
            helpBox.AddToClassList("mb-2");
            var messageLabel = helpBox.Q<Label>();
            messageLabel.AddToClassList("text-bold");
            messageLabel.style.fontSize = 16;
            var row = new VisualElement();
            row.AddToClassList("mt-2");
            row.AddToClassList("mb-1");
            row.AddToClassList("row");
            row.AddToClassList("align-items-stretch");
            row.AddToClassList("flex-1");
            var messageText = new Label(message);
            messageText.AddToClassList("pl-2");
            messageText.AddToClassList("white-space-normal");
            row.Add(messageText);
            if (buttonText != null)
            {
                messageText.AddToClassList("flex-10");
                var button = new Button(buttonAction)
                {
                    text = buttonText
                };
                button.AddToClassList("ml-2");
                button.AddToClassList("flex-2");
                button.AddToClassList("white-space-normal");
                row.Add(button);
            }
            helpBox.Add(row);
            return helpBox;
        }

        private bool CreateSceneSetupGUI(out VisualElement element)
        {
            var areLayersSetUp = UpdateLayers.AreLayersSetup();
            var isCollisionMatrixSetUp = UpdateLayers.IsCollisionLayerMatrixSetup();
            var mandatoryExpand = !areLayersSetUp || !isCollisionMatrixSetUp;
            
            element = new VisualElement();

            if (!mandatoryExpand) return false;

            // Layers warning
            if (!areLayersSetUp)
            {
                element.Add(CreateSceneSetupMessageGUI(
                    "Layers", 
                    "VRChat scenes must have the same Unity layer configuration as VRChat so we can all predict things like physics and collisions. Pressing this button will configure your project's layers to match VRChat.",
                    "Setup Layers for VRChat",
                    () =>
                    {
                        if (EditorUtility.DisplayDialog("Setup Layers for VRChat",
                                "This adds all VRChat layers to your project and pushes any custom layers down the layer list. If you have custom layers assigned to gameObjects, you'll need to reassign them. Are you sure you want to continue?",
                                "Do it!", "Don't do it"))
                        {
                            UpdateLayers.SetupEditorLayers();
                            OnShouldRevalidate?.Invoke(this, EventArgs.Empty);
                        }
                    }
                ));
            }
            else
            {
                element.Add(CreateSceneSetupMessageGUI("Layers", "Step Complete!"));
            }
            

            // Collision matrix warning
            if (isCollisionMatrixSetUp) return true;
            if (!areLayersSetUp)
            {
                element.Add(CreateSceneSetupMessageGUI("Collision Matrix", "You must first configure your layers for VRChat to proceed. Please see above."));
            }
            else
            {
                element.Add(CreateSceneSetupMessageGUI(
                    "Collision Matrix",
                    "VRChat uses specific layers for collision. In order for testing and development to run smoothly it is necessary to configure your project's collision matrix to match that of VRChat.",
                    "Setup Collision Matrix",
                    () =>
                    {
                        if (EditorUtility.DisplayDialog("Setup Collision Matrix",
                                "This will setup the correct physics collisions in the PhysicsManager for VRChat layers. Are you sure you want to continue?",
                                "Do it!", "Don't do it"))
                        {
                            UpdateLayers.SetupCollisionLayerMatrix();
                            OnShouldRevalidate?.Invoke(this, EventArgs.Empty);
                        }
                    }
                ));
            }

            return true;
        }

        private void OnGUISceneCheck(VRC_SceneDescriptor scene)
        {
            CheckUploadChanges(scene);

            List<VRC_EventHandler> sdkBaseEventHandlers = GatherComponentsOfTypeInScene<VRC_EventHandler>();

            if (sdkBaseEventHandlers.Count > 0)
            {
                _builder.OnGUIError(scene,
                    "You have Event Handlers in your scene that are not allowed in this build configuration.",
                    delegate
                    {
                        List<Object> gos = sdkBaseEventHandlers.ConvertAll(item => (Object) item.gameObject);
                        Selection.objects = gos.ToArray();
                    },
                    delegate
                    {
                        foreach (VRC_EventHandler eh in sdkBaseEventHandlers)
                        {
                            Object.DestroyImmediate(eh);
                        }
                    });
            }

            // If the user is trying to use native text components and has no TextMeshPro components, inform them that
            // TMP tends to appear clearer since it uses a signed distance field for rendering text.
            if (
                GatherComponentsOfTypeInScene<UnityEngine.UI.Text>().Count > 0 ||
                GatherComponentsOfTypeInScene<UnityEngine.TextMesh>().Count > 0
            )
            {
                // Search several common TMP types.
                if (
                    GatherComponentsOfTypeInScene<TMPro.TMP_Text>().Count == 0 &&
                    GatherComponentsOfTypeInScene<TMPro.TMP_Dropdown>().Count == 0 &&
                    GatherComponentsOfTypeInScene<TMPro.TMP_InputField>().Count == 0
                )
                {
                    _builder.OnGUIInformation(scene, "Your world contains one or more Unity text components, but no TextMeshPro components. Consider using TextMeshPro instead, since it's typically clearer and easier to read than native Unity text.");
                }
            }

            Vector3 g = Physics.gravity;
            if (Math.Abs(g.x) > float.Epsilon || Math.Abs(g.z) > float.Epsilon)
                _builder.OnGUIWarning(scene,
                    "Gravity vector is not straight down. Though we support different gravity, player orientation is always 'upwards' so things don't always behave as you intend.",
                    delegate { SettingsService.OpenProjectSettings("Project/Physics"); }, null);
            if (g.y > 0)
                _builder.OnGUIWarning(scene,
                    "Gravity vector is not straight down, inverted or zero gravity will make walking extremely difficult.",
                    delegate { SettingsService.OpenProjectSettings("Project/Physics"); }, null);
            if (Math.Abs(g.y) < float.Epsilon)
                _builder.OnGUIWarning(scene,
                    "Zero gravity will make walking extremely difficult, though we support different gravity, player orientation is always 'upwards' so this may not have the effect you're looking for.",
                    delegate { SettingsService.OpenProjectSettings("Project/Physics"); }, null);

            if (CheckFogSettings())
            {
                _builder.OnGUIWarning(
                    scene,
                    "Fog shader stripping is set to Custom, this may lead to incorrect or unnecessary shader variants being included in the build. You should use Automatic unless you change the fog mode at runtime.",
                    delegate { SettingsService.OpenProjectSettings("Project/Graphics"); },
                    delegate
                    {
                        EnvConfig.SetFogSettings(
                            new EnvConfig.FogSettings(EnvConfig.FogSettings.FogStrippingMode.Automatic));
                    });
            }

            if (scene.autoSpatializeAudioSources)
            {
                _builder.OnGUIWarning(scene,
                    "Your scene previously used the 'Auto Spatialize Audio Sources' feature. This has been deprecated, press 'Fix' to disable. Also, please add VRC_SpatialAudioSource to all your audio sources. Make sure Spatial Blend is set to 3D for the sources you want to spatialize.",
                    null,
                    delegate { scene.autoSpatializeAudioSources = false; }
                );
            }

            List<VRCPhysBoneBase> physBones = GatherComponentsOfTypeInScene<VRCPhysBoneBase>();
            if (physBones.Count > 0)
            {
                List<Object> parentlessPhysBoneObjects = new List<Object>(physBones.Count);
                foreach (VRCPhysBoneBase physBone in physBones)
                {
                    Transform rootTransform = physBone.GetRootTransform();
                    if (rootTransform.parent == null)
                    {
                        parentlessPhysBoneObjects.Add(physBone.gameObject);
                    }
                }
                if (parentlessPhysBoneObjects.Count > 0)
                {
                    _builder.OnGUIWarning(scene,
                        "One or more PhysBones in the scene have no parent, which may cause them to behave in unexpected ways. You should make sure your PhysBones are all set as the child of another game object.",
                        delegate { Selection.objects = parentlessPhysBoneObjects.ToArray(); });
                }
            }

            List<VRCPhysBoneColliderBase> physBoneColliders = GatherComponentsOfTypeInScene<VRCPhysBoneColliderBase>();
            List<ContactBase> contacts = GatherComponentsOfTypeInScene<ContactBase>();
            int physBoneShapeCount = physBones.Count + physBoneColliders.Count;
            int contactShapeCount = contacts.Count;
            if (physBoneShapeCount > CollisionScene.MAX_SHAPES_WORLD)
            {
                _builder.OnGUIWarning(scene,
                    $"This scene contains a total of {physBoneShapeCount} PhysBones and PhysBone Colliders, but in the client, only up to {CollisionScene.MAX_SHAPES_WORLD} can be active in the world at the same time. Consider reducing the number of PhysBone components or PhysBone Collider components in the scene.");
            }
            if (contactShapeCount > CollisionScene.MAX_SHAPES_WORLD)
            {
                _builder.OnGUIWarning(scene,
                    $"This scene contains {contactShapeCount} Contacts, but in the client, only up to {CollisionScene.MAX_SHAPES_WORLD} can be active in the world at the same time. Consider reducing the number of Contact components in the scene.");
            }

            List<AudioSource> audioSources = GatherComponentsOfTypeInScene<AudioSource>();
            foreach (AudioSource a in audioSources)
            {
                if (a.GetComponent<ONSPAudioSource>() != null)
                {
                    _builder.OnGUIWarning(scene,
                        "Found audio source(s) using ONSP, this is deprecated. Press 'fix' to convert to VRC_SpatialAudioSource.",
                        delegate { Selection.activeObject = a.gameObject; },
                        delegate
                        {
                            Selection.activeObject = a.gameObject;
                            AutoAddSpatialAudioComponents.ConvertONSPAudioSource(a);
                        }
                    );
                    break;
                }

                if (a.GetComponent<VRC_SpatialAudioSource>() == null)
                {
                    string msg =
                        "Found 3D audio source with no VRC Spatial Audio component, this is deprecated. Press 'fix' to add a VRC_SpatialAudioSource.";
                    if (IsAudioSource2D(a))
                        msg =
                            "Found 2D audio source with no VRC Spatial Audio component, this is deprecated. Press 'fix' to add a (disabled) VRC_SpatialAudioSource.";

                    _builder.OnGUIWarning(scene, msg,
                        delegate { Selection.activeObject = a.gameObject; },
                        delegate
                        {
                            Selection.activeObject = a.gameObject;
                            AutoAddSpatialAudioComponents.AddVRCSpatialToBareAudioSource(a);
                        }
                    );
                    break;
                }
            }

            if (VRCSdkControlPanel.HasSubstances())
            {
                _builder.OnGUIWarning(scene,
                    "One or more scene objects have Substance materials. This is not supported and may break in game. Please bake your Substances to regular materials.",
                    () => { Selection.objects = VRCSdkControlPanel.GetSubstanceObjects(); },
                    null);
            }

            if (VRCSdkControlPanel.ReferenceCameraHasTAAEnabled())
            {
                _builder.OnGUIWarning(scene,
                    "The Reference Camera has a Post-process Layer component that uses Temporal Anti-Aliasing. This is not supported. Please select a different anti-aliasing method.",
                    () => {
                        var refCam = VRCSdkControlPanel.GetReferenceCameraObject();
                        Selection.objects = refCam != null 
                                            ? new Object[] { refCam } 
                                            : new Object[0]; 
                    },
                    () => {
                        PostProcessLayer ppl = VRCSdkControlPanel.GetReferenceCameraObject()?.GetComponent<PostProcessLayer>();
                        if (ppl != null)
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.None;
                    });
            }

            string vrcFilePath = UnityWebRequest.UnEscapeURL(EditorPrefs.GetString("lastVRCPath"));
            bool isMobilePlatform = ValidationEditorHelpers.IsMobilePlatform();
            if (!string.IsNullOrEmpty(vrcFilePath))
            {
                if (ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge(ContentType.World, vrcFilePath, out int fileSize, isMobilePlatform))
                {
                    _builder.OnGUIWarning(scene,
                        ValidationHelpers.GetAssetBundleOverSizeLimitMessageSDKWarning(ContentType.World, fileSize, isMobilePlatform), null, null);
                }
            }

            if (ValidationEditorHelpers.CheckIfUncompressedAssetBundleFileTooLarge(ContentType.World, out int fileSizeUncompressed, isMobilePlatform))
            {
                _builder.OnGUIWarning(scene,
                    ValidationHelpers.GetAssetBundleOverSizeLimitMessageSDKWarning(ContentType.World, fileSizeUncompressed, isMobilePlatform, false), null, null);
            }

#if UNITY_ANDROID || UNITY_IOS
            _rootGameObjectsBuffer.Clear();
            scene.gameObject.scene.GetRootGameObjects(_rootGameObjectsBuffer);
            foreach (GameObject go in _rootGameObjectsBuffer)
            {
                // check root game objects for illegal shaders
                IEnumerable<Shader> illegalShaders = VRC.SDKBase.Validation.WorldValidation.FindIllegalShaders(go);
                foreach (Shader s in illegalShaders)
                {
                    _builder.OnGUIWarning(scene, "World uses unsupported shader '" + s.name + "'. This could cause low performance or future compatibility issues.", null, null);
                }
            }
#endif

            // Recommend super sampled UI if the shader is available.
            Shader ssShader = Shader.Find("VRChat/Mobile/Worlds/Supersampled UI");
            if (ssShader != null)
            {
                List<Graphic> uiDefaultGraphics = GatherComponentsOfTypeInScene<Graphic>();
                for (int i = uiDefaultGraphics.Count - 1; i >= 0; i--)
                {
                    // Only keep Graphic components using UI/Default
                    // Since material is virtual and could be overridden by anything, protect with try-catch
                    // in case it throws.
                    bool usingDefaultUi;
                    try
                    {
                        const string defaultMaterialName = "UI/Default";

                        Graphic graphic = uiDefaultGraphics[i];
                        if (graphic is TMPro.TMP_SubMeshUI tmpSubMeshUi)
                        {
                            // Special treatment of this TMP type, because this class will throw when accessing material
                            // if sharedMaterial is null...
                            usingDefaultUi = tmpSubMeshUi.sharedMaterial != null &&
                                             tmpSubMeshUi.sharedMaterial.shader.name == defaultMaterialName;
                        }
                        else
                        {
                            usingDefaultUi = uiDefaultGraphics[i].material != null &&
                                             uiDefaultGraphics[i].material.shader.name == defaultMaterialName &&
                                             uiDefaultGraphics[i].materialForRendering != null &&
                                             uiDefaultGraphics[i].materialForRendering.shader.name == defaultMaterialName;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);

                        // We can't tell if this is default or not. Just treat it as not default and leave it alone.
                        usingDefaultUi = false;
                    }

                    if (!usingDefaultUi)
                    {
                        uiDefaultGraphics.RemoveAt(i);
                    }
                }
                if (uiDefaultGraphics.Count > 0)
                {
                    _builder.OnGUIInformation(scene,
                        "Found one or more UI graphics using Unity's built-in UI shader. Press 'Auto Fix' to switch affected graphics to a super-sampled UI shader, which can help improve readability in VR.",
                        () =>
                        {
                            Object[] graphicObjects = new Object[uiDefaultGraphics.Count];
                            for (int i = 0; i < uiDefaultGraphics.Count; i++)
                            {
                                graphicObjects[i] = uiDefaultGraphics[i].gameObject;
                            }
                            Selection.objects = graphicObjects;
                        },
                        () =>
                        {
                            bool hasReportedMissingBuiltInSsMaterial = false;
                            Undo.SetCurrentGroupName("Switch to Super-Sampled UI");
                            int groupIndex = Undo.GetCurrentGroup();
                            try
                            {
                                foreach (Graphic graphic in uiDefaultGraphics)
                                {
                                    Material material = graphic.material;

                                    EditorUtility.SetDirty(graphic);
                                    Undo.RecordObject(graphic, $"Assign SSUI to graphic {graphic.name}");

                                    if (!AssetDatabase.IsMainAsset(material))
                                    {
                                        // This material isn't a file in the project, most likely the Default UI material built into Unity.
                                        // We can't modify this, so replace it instead.
                                        Material replacementMaterial =
                                            AssetDatabase.LoadAssetAtPath<Material>("Packages/com.vrchat.worlds/Editor/VRCSDK/SDK3/VRCSuperSampledUIMaterial.mat");
                                        if (replacementMaterial != null)
                                        {
                                            material = replacementMaterial;
                                        }
                                        else
                                        {
                                            if (!hasReportedMissingBuiltInSsMaterial)
                                            {
                                                Debug.LogWarning("Could not find substitute super-sampled UI material in the worlds package. (VRCSuperSampledUIMaterial)");
                                                hasReportedMissingBuiltInSsMaterial = true;
                                            }

                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // We're about to modify this material in place, so capture it for undo.
                                        EditorUtility.SetDirty(material);
                                        Undo.RecordObject(material, $"Assign SSUI to material {material.name}");
                                    }

                                    material.shader = ssShader;
                                    graphic.material = material;

                                    // Apply overrides if this graphic is part of a prefab instance.
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
                                }
                            }
                            finally
                            {
                                Undo.CollapseUndoOperations(groupIndex);
                            }
                        });
                }
            }
            
            foreach (VRC.SDK3.Components.VRCObjectSync os in GatherComponentsOfTypeInScene<VRC.SDK3.Components.VRCObjectSync>())
            {
                if (os.GetComponents<VRC.Udon.UdonBehaviour>().Any((ub) => ub.SyncIsManual))
                    _builder.OnGUIError(scene, "Object Sync cannot share an object with a manually synchronized Udon Behaviour",
                        delegate { Selection.activeObject = os.gameObject; }, null);
                if (os.GetComponent<VRC.SDK3.Components.VRCObjectPool>() != null)
                    _builder.OnGUIError(scene, "Object Sync cannot share an object with an object pool",
                        delegate { Selection.activeObject = os.gameObject; }, null);
            }
            
            VerifyMaxTextureSize(scene);
            // Pre-unity 2021 the 'Kaiser' algorithm would introduce a lot of aliasing - disable prior to that
#if UNITY_2021_1_OR_NEWER
            VerifyTextureMipFiltering(scene);
#endif

            VerifySpawnPlacement(scene);
            
            _validationsFoldout ??= _builder.rootVisualElement.Q<StepFoldout>("validations-foldout");
            _validationsFoldout?.SetTitle($"Review Any Alerts ({_builder.GUIAlertCount(scene)})");
        }

        private void VerifyMaxTextureSize(VRC_SceneDescriptor scene)
        {
            var badTextureImporters = VRCSdkControlPanel.GetOversizeTextureImporters(GatherComponentsOfTypeInScene<Renderer>());
            
            if (badTextureImporters.Count == 0)
                return;

            _builder.OnGUIWarning(scene, $"This scene has textures bigger than {VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE}. Please reduce them to save memory in your world.",
                null,
                () =>
                {
                    List<string> paths = new List<string>();
                    foreach (TextureImporter t in badTextureImporters)
                    {
                        Undo.RecordObject(t, $"Set Max Texture Size to {VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE}");
                        t.maxTextureSize = VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }

                    AssetDatabase.ForceReserializeAssets(paths);
                    AssetDatabase.Refresh();
                });
        }

        private void VerifyTextureMipFiltering(VRC_SceneDescriptor scene)
        {
            var badTextureImporters = VRCSdkControlPanel.GetBoxFilteredTextureImporters(GatherComponentsOfTypeInScene<Renderer>());
            if (badTextureImporters.Count == 0)
                return;

            _builder.OnGUIInformation(scene, $"This scene uses textures with 'Box' mipmap filtering, which blurs distant textures. Switch to 'Kaiser' for improved sharpness{(VRCPackageSettings.Instance.dpidMipmaps ? " (this will be overriden with the newer 'DPID' algorithm, this can be disabled in the settings)": "")}.",
                null,
                () =>
                {
                    List<string> paths = new List<string>();
                    foreach (TextureImporter t in badTextureImporters)
                    {
                        Undo.RecordObject(t, $"Set texture filtering to 'Kaiser'");
                        t.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }

                    AssetDatabase.ForceReserializeAssets(paths);
                    AssetDatabase.Refresh();
                });
        }
        
        private void VerifySpawnPlacement(VRC_SceneDescriptor scene)
        {
            bool IsSpawnInPositionLimits(Vector3 position)
            {
                return Mathf.Abs(position.x) < WORLD_MAX_SPAWN_OFFSET && 
                       Mathf.Abs(position.y) < WORLD_MAX_SPAWN_OFFSET &&
                       Mathf.Abs(position.z) < WORLD_MAX_SPAWN_OFFSET;
            }

            bool IsSpawnAboveRespawnPlane(Vector3 position)
            {
                return position.y >= scene.RespawnHeightY;
            }

            List<Transform> validSpawns = scene.GetValidatedSpawnList();

            if (validSpawns.Count == 0)
            {
                _builder.OnGUIError(scene,
                    WORLD_SPAWN_EMPTY_ERROR,
                    () => { Selection.activeGameObject = scene.gameObject; });
                return;
            }
            
            if (scene.spawnOrder is VRC_SceneDescriptor.SpawnOrder.First or VRC_SceneDescriptor.SpawnOrder.Demo)
            {
                var spawn = validSpawns[0];
                if (!IsSpawnInPositionLimits(spawn.position))
                {
                    _builder.OnGUIWarning(scene, 
                        string.Format(WORLD_SPAWN_OFFSET_WARNING, spawn.position),
                        () => { Selection.activeGameObject = spawn.gameObject; }
                    );
                }
                else if (!IsSpawnAboveRespawnPlane(spawn.position))
                {
                    _builder.OnGUIWarning(scene,
                        string.Format(WORLD_SPAWN_BELOW_RESPAWN_PLANE_WARNING, spawn.position, scene.RespawnHeightY),
                        () => { Selection.activeGameObject = scene.gameObject; });
                }
                return;
            }

            foreach (Transform spawn in validSpawns)
            {
                if (!IsSpawnInPositionLimits(spawn.position))
                {
                    _builder.OnGUIWarning(scene,
                        string.Format(WORLD_MULTI_SPAWN_OFFSET_WARNING, spawn.position),
                        () => { Selection.activeGameObject = spawn.gameObject; }
                    );
                    return;
                }
                else if (!IsSpawnAboveRespawnPlane(spawn.position))
                {
                    _builder.OnGUIWarning(scene,
                        string.Format(WORLD_MULTI_SPAWN_BELOW_RESPAWN_PLANE_WARNING, spawn.position, scene.RespawnHeightY),
                        () => { Selection.activeGameObject = scene.gameObject; });
                    return;
                }
            }
        }
        
        /// <summary>
        /// Get all components of a given type in loaded scenes, including disabled components.
        /// </summary>
        private static List<T> GatherComponentsOfTypeInScene<T>() where T : UnityEngine.Component
        {
            T[] candidates = Resources.FindObjectsOfTypeAll<T>();
            List<T> results = new List<T>(candidates.Length);

            foreach (T candidate in candidates)
            {
                if (!EditorUtility.IsPersistent(candidate.transform.root.gameObject) && !(candidate.hideFlags == HideFlags.NotEditable || candidate.hideFlags == HideFlags.HideAndDontSave))
                {
                    results.Add(candidate);
                }
            }

            return results;
        }

        private static void CheckUploadChanges(VRC_SceneDescriptor scene)
        {
            if (!EditorPrefs.HasKey("VRC.SDKBase_scene_changed") ||
                !EditorPrefs.GetBool("VRC.SDKBase_scene_changed")) return;
            EditorPrefs.DeleteKey("VRC.SDKBase_scene_changed");

            if (EditorPrefs.HasKey("VRC.SDKBase_capacity"))
            {
                scene.capacity = EditorPrefs.GetInt("VRC.SDKBase_capacity");
                EditorPrefs.DeleteKey("VRC.SDKBase_capacity");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_sex"))
            {
                scene.contentSex = EditorPrefs.GetBool("VRC.SDKBase_content_sex");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_sex");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_violence"))
            {
                scene.contentViolence = EditorPrefs.GetBool("VRC.SDKBase_content_violence");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_violence");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_gore"))
            {
                scene.contentGore = EditorPrefs.GetBool("VRC.SDKBase_content_gore");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_gore");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_other"))
            {
                scene.contentOther = EditorPrefs.GetBool("VRC.SDKBase_content_other");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_other");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_release_public"))
            {
                scene.releasePublic = EditorPrefs.GetBool("VRC.SDKBase_release_public");
                EditorPrefs.DeleteKey("VRC.SDKBase_release_public");
            }

            EditorUtility.SetDirty(scene);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static bool CheckFogSettings()
        {
            EnvConfig.FogSettings fogSettings = EnvConfig.GetFogSettings();
            if (fogSettings.fogStrippingMode == EnvConfig.FogSettings.FogStrippingMode.Automatic)
            {
                return false;
            }

            return fogSettings.keepLinear || fogSettings.keepExp || fogSettings.keepExp2;
        }

        private static bool IsAudioSource2D(AudioSource src)
        {
            AnimationCurve curve = src.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            return Math.Abs(src.spatialBlend) < float.Epsilon && (curve == null || curve.keys.Length <= 1);
        }
        
        private static Mesh[] _primitiveMeshes;

        private static List<UdonBehaviour> ShouldShowPrimitivesWarning()
        {
            if (_primitiveMeshes == null)
            {
                PrimitiveType[] primitiveTypes = (PrimitiveType[]) System.Enum.GetValues(typeof(PrimitiveType));
                _primitiveMeshes = new Mesh[primitiveTypes.Length];

                for (int i = 0; i < primitiveTypes.Length; i++)
                {
                    PrimitiveType primitiveType = primitiveTypes[i];
                    GameObject go = GameObject.CreatePrimitive(primitiveType);
                    _primitiveMeshes[i] = go.GetComponent<MeshFilter>().sharedMesh;
                    Object.DestroyImmediate(go);
                }
            }

            UdonBehaviour[] allBehaviours = Object.FindObjectsOfType<UdonBehaviour>();
            List<UdonBehaviour> failedBehaviours = new List<UdonBehaviour>(allBehaviours.Length);
            foreach (UdonBehaviour behaviour in allBehaviours)
            {
                IUdonVariableTable publicVariables = behaviour.publicVariables;
                foreach (string symbol in publicVariables.VariableSymbols)
                {
                    if (!publicVariables.TryGetVariableValue(symbol, out Mesh mesh))
                    {
                        continue;
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    bool all = true;
                    foreach (Mesh primitiveMesh in _primitiveMeshes)
                    {
                        if (mesh != primitiveMesh)
                        {
                            continue;
                        }

                        all = false;
                        break;
                    }

                    if (all)
                    {
                        continue;
                    }

                    failedBehaviours.Add(behaviour);
                }
            }

            return failedBehaviours;
        }

        private void FixPrimitivesWarning()
        {
            UdonBehaviour[] allObjects = Object.FindObjectsOfType<UdonBehaviour>();
            foreach (UdonBehaviour behaviour in allObjects)
            {
                IUdonVariableTable publicVariables = behaviour.publicVariables;
                foreach (string symbol in publicVariables.VariableSymbols)
                {
                    if (!publicVariables.TryGetVariableValue(symbol, out Mesh mesh))
                    {
                        continue;
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    bool all = true;
                    foreach (Mesh primitiveMesh in _primitiveMeshes)
                    {
                        if (mesh != primitiveMesh)
                        {
                            continue;
                        }

                        all = false;
                        break;
                    }

                    if (all)
                    {
                        continue;
                    }

                    Mesh clone = Object.Instantiate(mesh);

                    Scene scene = behaviour.gameObject.scene;
                    string scenePath = Path.GetDirectoryName(scene.path) ?? "Assets";

                    string folderName = $"{scene.name}_MeshClones";
                    string folderPath = Path.Combine(scenePath, folderName);

                    if (!AssetDatabase.IsValidFolder(folderPath))
                    {
                        AssetDatabase.CreateFolder(scenePath, folderName);
                    }

                    string assetPath = Path.Combine(folderPath, $"{clone.name}.asset");

                    Mesh existingClone = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                    if (existingClone == null)
                    {
                        AssetDatabase.CreateAsset(clone, assetPath);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        clone = existingClone;
                    }

                    publicVariables.TrySetVariableValue(symbol, clone);
                    EditorSceneManager.MarkSceneDirty(behaviour.gameObject.scene);
                }
            }
        }
        
        #endregion
        
        #region World Builder UI (UIToolkit)
        
        private const string ACCEPT_TERMS_BLOCK_TEXT = "You must accept the terms below to upload content to VRChat";
        private int _progressId;
        
        public void CreateBuilderErrorGUI(VisualElement root)
        {
            var errorContainer = new VisualElement();
            errorContainer.AddToClassList("builder-error-container");
            root.Add(errorContainer);
            var errorLabel = new Label(_pipelineManagers.Length > 1 ?
                "You can only have a single Pipeline Manager in a Scene" :
                "A VRCSceneDescriptor is required to build a VRChat World"
            );
            errorLabel.AddToClassList("mb-2");
            errorLabel.AddToClassList("text-center");
            errorLabel.AddToClassList("white-space-normal");
            errorLabel.style.maxWidth = 450;
            errorContainer.Add(errorLabel);

            if (_pipelineManagers.Length > 1)
            {
                var selectButton = new Button
                {
                    text = "Select all PipelineManagers"
                };
                selectButton.clicked += () =>
                {
                    Selection.objects = _pipelineManagers.Select(p => (Object) p.gameObject).ToArray();
                };
                errorContainer.Add(selectButton);
                return;
            }
            
            var addButton = new Button
            {
                text = "Add a VRCSceneDescriptor",
                tooltip = "Adds a VRCSceneDescriptor to the Scene"
            };
            addButton.clickable.clicked += () =>
            {
                var VRCWorld =
                    AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.vrchat.worlds/Samples/UdonExampleScene/Prefabs/VRCWorld.prefab");
                if (VRCWorld != null)
                {
                    var newVrcWorld = GameObject.Instantiate(VRCWorld);
                    Undo.RecordObject(newVrcWorld, "Adjusted Name");
                    newVrcWorld.name = "VRCWorld";
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
                _builder.ResetIssues();
            };
            errorContainer.Add(addButton);
        }

        private VRCWorld _worldData;
        private VRCWorld _originalWorldData;
        private VisualElement _saveChangesBlock;
        private VisualElement _visualRoot;
        private VisualElement _buildVisualRoot;
        private TagsField _tagsField;
        private VRCTextField _nameField;
        private VRCTextField _descriptionField;
        private IntegerField _capacityField;
        private IntegerField _recommendedCapacityField;
        private Label _lastUpdatedLabel;
        private Label _versionLabel;
        private Button _saveChangesButton;
        private Button _discardChangesButton;
        private Foldout _infoFoldout;
        private ThumbnailBlock _thumbnailBlock;
        private Thumbnail _thumbnail;
        private BuilderProgress _builderProgress;
        private Toggle _worldDebuggingToggle;
        private ContentWarningsField _contentWarningsField;
        protected VisualElement _v3Block;
        private VisualElement _visibilityBlock;
        private PopupField<string> _visibilityPopup;
        private Modal _labsPublishModal;
        private Modal _unpublishModal;
        private Button _publishToLabsBtn;
        private Button _publishToLabsCancelBtn;
        private Button _unpublishBtn;
        private Button _unpublishCancelBtn;
        private VisualElement _buildButtonsBlock;
        private StepFoldout _validationsFoldout;
        private VisualElement _mainBuildActionDisabledBlock;
        private Label _mainBuildActionDisabledText;
        
        protected PipelineManager[] _pipelineManagers;
        private string _lastBlueprintId;

        private string _newThumbnailImagePath;
        
        private bool _isContentInfoDirty;
        private bool IsContentInfoDirty
        {
            get => _isContentInfoDirty;
            set
            {
                _isContentInfoDirty = value;
                var isDirty = CheckDirty();
                var alreadyDirty = !_saveChangesBlock.ClassListContains("d-none");
                _saveChangesBlock.EnableInClassList("d-none", !isDirty);
                if (isDirty && !alreadyDirty)
                {
                    _saveChangesBlock.experimental.animation.Start(new Vector2(_visualRoot.layout.width, 0), new Vector2(_visualRoot.layout.width, 50), 250, (element, vector2) =>
                    {
                        element.style.height = vector2.y;
                    });
                }
            }
        }

        private bool _uiEnabled;
        private bool UiEnabled
        {
            get => _uiEnabled;
            set
            {
                _uiEnabled = value;
                _infoFoldout.SetEnabled(value);
                _saveChangesButton.SetEnabled(value);
                _discardChangesButton.SetEnabled(value);
                _thumbnailBlock.SetEnabled(value);
                _visibilityBlock.SetEnabled(value);
            }
        }

        private bool _isNewWorld;
        private bool IsNewWorld
        {
            get => _isNewWorld;
            set
            {
                _isNewWorld = value;
            }
        }

        public async void CreateContentInfoGUI(VisualElement root)
        {
            // We want to avoid any background operations while building
            if (VRCMultiPlatformBuild.MPBState == VRCMultiPlatformBuild.MultiPlatformBuildState.Building) return;
            
            root.Clear();
            root.UnregisterCallback<DetachFromPanelEvent>(HandlePanelDetach);
            EditorSceneManager.sceneClosed -= HandleSceneClosed;
            VRCSdkControlPanel.OnSdkPanelDisable -= HandleSdkPanelDisable;

            if (!APIUser.IsLoggedIn) return;
            
            var tree = Resources.Load<VisualTreeAsset>("VRCSdkWorldBuilderContentInfo");
            tree.CloneTree(root);
            var styles = Resources.Load<StyleSheet>("VRCSdkWorldBuilderContentInfoStyles");
            if (!root.styleSheets.Contains(styles))
            {
                root.styleSheets.Add(styles);
            }
            
            root.RegisterCallback<DetachFromPanelEvent>(HandlePanelDetach);
            EditorSceneManager.sceneClosed += HandleSceneClosed;
            VRCSdkControlPanel.OnSdkPanelDisable += HandleSdkPanelDisable;
            
            _visualRoot = root;
            _nameField = root.Q<VRCTextField>("content-name");
            _descriptionField = root.Q<VRCTextField>("content-description");
            _capacityField = root.Q<IntegerField>("content-capacity");
            _infoFoldout = _builder.rootVisualElement.Q<Foldout>("info-foldout");
            var capacityFieldHelpButton = root.Q<Button>("show-capacity-help-button");
            _recommendedCapacityField = root.Q<IntegerField>("content-recommended-capacity");
            var recommendedCapacityFieldHelpButton = root.Q<Button>("show-recommended-capacity-help-button");
            _thumbnailBlock = root.Q<ThumbnailBlock>();
            _thumbnail = _thumbnailBlock.Thumbnail;
            _tagsField = root.Q<TagsField>("content-tags");
            _contentWarningsField = root.Q<ContentWarningsField>("content-warnings");
            
            // Pass full-width bounds to the popup for better layout
            var warningTagsBlock = root.Q("warnings-tags-block");
            _contentWarningsField.SetPopupBoundsReference(warningTagsBlock);
            
            var worldDebuggingHelpButton = root.Q<Button>("show-world-debugging-help-button");
            var platformsBlock = root.Q<Label>("content-platform-info");
            _lastUpdatedLabel = root.Q<Label>("last-updated-label");
            _versionLabel = root.Q<Label>("version-label");
            _saveChangesBlock = root.panel.visualTree.Q("save-changes-block");
            _saveChangesButton = _saveChangesBlock.Q<Button>("save-changes-button");
            _discardChangesButton = _saveChangesBlock.Q<Button>("discard-changes-button");
            _worldDebuggingToggle = root.Q<Toggle>("world-debugging-toggle");
            _validationsFoldout = _builder.rootVisualElement.Q<StepFoldout>("validations-foldout");
            
            _visibilityBlock = root.Q("visibility-block");
            
            _visibilityPopup = new PopupField<string>(
                "Visibility", 
                new List<string> {"private", "public"},
                "private",
                selected => selected.Substring(0,1).ToUpper() + selected.Substring(1), 
                item => item.Substring(0,1).ToUpper() + item.Substring(1)
            );
            _visibilityBlock.Add(_visibilityPopup);
            
            var communityLabsHelpButton = root.Q<Button>("community-labs-help-button");
            _labsPublishModal = root.Q<Modal>("labs-publish-modal");
            _labsPublishModal.SetAnchor(root);
            _publishToLabsBtn = _labsPublishModal.Q<Button>("publish-to-labs-btn");
            _publishToLabsCancelBtn = _labsPublishModal.Q<Button>("publish-to-labs-cancel-btn");

            _publishToLabsBtn.clicked += HandlePublishWorldConfirm;
            _publishToLabsCancelBtn.clicked += HandlePublishWorldCancel;
            
            _unpublishModal = root.Q<Modal>("unpublish-modal");
            _unpublishModal.SetAnchor(root);
            _unpublishBtn = _unpublishModal.Q<Button>("unpublish-btn");
            _unpublishCancelBtn = _unpublishModal.Q<Button>("unpublish-cancel-btn");

            _unpublishBtn.clicked += HandleUnpublishWorldConfirm;
            _unpublishCancelBtn.clicked += HandleUnpublishWorldCancel;

            // Load the world data
            _nameField.Loading = true;
            _descriptionField.Loading = true;
            _thumbnail.Loading = true;
            _capacityField.SetValueWithoutNotify(32);
            _recommendedCapacityField.SetValueWithoutNotify(16);
            _nameField.Reset();
            _descriptionField.Reset();
            _thumbnail.ClearImage();
            IsNewWorld = false;
            UiEnabled = false;

            capacityFieldHelpButton.clicked += () =>
            {
                Modal.CreateAndShow("What is Maximum Capacity?",
                    "Maximum Capacity controls how many users are allowed into one instance of a world at a time.\n\n" +
                    "Additional users are not allowed to join over this limit with very few exceptions, such as the world creator, instance owners, and group owners.",
                    root);
            };
            recommendedCapacityFieldHelpButton.clicked += () =>
            {
                Modal.CreateAndShow("What is Recommended Capacity?",
                    "Recommended Capacity controls how many users are allowed into one Public or Group Public instance of a world before that instance is no longer visible on the world instance list.\n\n" +
                    "Instances above Recommended Capacity are still accessible through other methods, such as joining through a group, or off of a friend. All other instance types are not impacted by the Recommended Capacity.",
                    root);
            };
            worldDebuggingHelpButton.clicked += () =>
            {
                Modal.CreateAndShow("What is World Debugging?",
                    "By default, World debug tools are only usable by you, the owner of the world. If you enable world debugging, then anyone can use them in your world.\n\n" +
                    "Warning: Enabling world debug tools will reveal the location and state of Udon Behaviours in your world. If you are making a puzzle world, or have secrets in your world, this could ruin the fun!\n\n" +
                    "To use the world debug tools, you'll need to pass the --enable-debug-gui flag to VRChat at startup, and press RShift and ` along with 7, 8, or 9. See the keyboard binding documentation at docs.vrchat.com for more details.",
                    root);
            };
            communityLabsHelpButton.clicked += () =>
            {
                Application.OpenURL(COMMUNITY_LABS_HELP_URL);
            };
            
            _pipelineManagers = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (_pipelineManagers.Length == 0)
            {
                Core.Logger.LogError("No PipelineManager found in scene, make sure you have added a scene descriptor");
                return;
            }
            
            var worldId = _pipelineManagers[0].blueprintId;
            _lastBlueprintId = worldId;
            _worldData = new VRCWorld();
            if (string.IsNullOrWhiteSpace(worldId))
            {
                IsNewWorld = true;
            }
            else
            {
                try
                {
                    _worldData = await VRCApi.GetWorld(worldId, true);

                    if (APIUser.CurrentUser != null && _worldData.AuthorId != APIUser.CurrentUser?.id)
                    {
                        ClearWorldData(_pipelineManagers[0]);
                    }

                }
                catch (TaskCanceledException)
                {
                    // world scene was changed
                    return;
                }
                catch (ApiErrorException ex)
                {
                    // 404 here with a defined blueprint usually means we do not own the content
                    // so we clear the blueprint ID and treat it as a new world
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        ClearWorldData(_pipelineManagers[0]);
                    }
                    else
                    {
                        Debug.LogError(ex.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (IsNewWorld)
            {
                RestoreSessionState();

                platformsBlock.parent.AddToClassList("d-none");

                _worldData.CreatedAt = DateTime.Now;
                _worldData.UpdatedAt = DateTime.MinValue;
                _lastUpdatedLabel.parent.AddToClassList("d-none");
                _versionLabel.parent.AddToClassList("d-none");
                _worldData.Capacity = 32;
                _worldData.RecommendedCapacity = 16;
                
                _visibilityBlock.AddToClassList("d-none");
                ContentInfoLoaded?.Invoke(this, (_worldData, _newThumbnailImagePath));
            }
            else
            {
                WorldBuilderSessionState.Clear();

                platformsBlock.parent.RemoveFromClassList("d-none");
                _visibilityBlock.RemoveFromClassList("d-none");
                
                _nameField.value = _worldData.Name;
                _descriptionField.value = _worldData.Description;
                _capacityField.value = _worldData.Capacity;
                _recommendedCapacityField.value = _worldData.RecommendedCapacity;
                // handle worlds without recommended capacity set
                if (_worldData.RecommendedCapacity == 0)
                {
                    _recommendedCapacityField.value = Mathf.FloorToInt(_worldData.Capacity / 2f);
                }

                var platforms = new HashSet<string>();
                foreach (var p in _worldData.UnityPackages.Select(p => VRCSdkControlPanel.CONTENT_PLATFORMS_MAP[p.Platform]))
                {
                    platforms.Add(p);
                }
                platformsBlock.text = string.Join(", ", platforms);

                _lastUpdatedLabel.text = (_worldData.UpdatedAt != DateTime.MinValue ? _worldData.UpdatedAt : _worldData.CreatedAt).ToLocalTime().ToString(CultureInfo.CurrentCulture);
                _lastUpdatedLabel.parent.RemoveFromClassList("d-none");
                
                _versionLabel.text = _worldData.Version.ToString();
                _versionLabel.parent.RemoveFromClassList("d-none");

                _worldDebuggingToggle.value = _worldData.Tags?.Contains("debug_allowed") ?? false;

                var isPrivate = _worldData.ReleaseStatus == "private";
                _visibilityPopup.value = isPrivate ? "private" : "public";

                var shouldShowPublishToLabs = isPrivate && !(_worldData.Tags?.Any(t => COMMUNITY_LABS_BLOCKED_TAGS.Contains(t)) ?? false);
                var canPublish = false;
                try
                {
                    canPublish = await VRCApi.GetCanPublishWorld(_worldData.ID);
                }
                catch (ApiErrorException e)
                {
                    if (e.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        Debug.LogError("Failed to get publish status: " + e.ErrorMessage);
                    }
                }

                var shouldShowCantPublish = shouldShowPublishToLabs && !canPublish;

                if (shouldShowCantPublish)
                {
                    _visibilityPopup.choices = new List<string> {"private"};
                    _visibilityPopup.SetEnabled(false);
                    _visibilityPopup.tooltip = "You can't publish worlds right now";
                } else if (shouldShowPublishToLabs)
                {
                    _visibilityPopup.choices = new List<string> {"private", "publish to Community Labs"};
                    _visibilityPopup.SetEnabled(true);
                    _visibilityPopup.tooltip = "Publish to Community Labs";
                }
                
                communityLabsHelpButton.EnableInClassList("d-none", !isPrivate);

                _visibilityPopup.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue == "private" && isPrivate) return;
                    HandlePublishClick();
                });
            
                await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl);
                ContentInfoLoaded?.Invoke(this, (_worldData, _newThumbnailImagePath));
            }
            
           
            _nameField.Loading = false;
            _descriptionField.Loading = false;
            _thumbnail.Loading = false;
            UiEnabled = true;

            _originalWorldData = _worldData;
            _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());

            var worldTags = _worldData.Tags ?? new List<string>();
            _tagsField.TagFilter = tagList => tagList.Where(t =>
                (APIUser.CurrentUser?.hasSuperPowers ?? false) || t.StartsWith("author_tag_")).ToList();
            _tagsField.TagLimit = APIUser.CurrentUser?.hasSuperPowers ?? false ? 100 : 5; 
            _tagsField.FormatTagDisplay = input => input.Replace("author_tag_", "");
            _tagsField.IsProtectedTag = input => input.StartsWith("system_");
            _tagsField.tags = worldTags;
            _tagsField.OnAddTag += HandleAddTag;
            _tagsField.OnRemoveTag += HandleRemoveTag;

            _contentWarningsField.OriginalOptions = _originalWorldData.Tags;
            _contentWarningsField.SelectedOptions = worldTags;
            _contentWarningsField.OnToggleOption += HandleToggleTag;

            _nameField.RegisterValueChangedCallback(HandleNameChange);
            _descriptionField.RegisterValueChangedCallback(HandleDescriptionChange);
            _capacityField.RegisterValueChangedCallback(HandleCapacityChange);
            _recommendedCapacityField.RegisterValueChangedCallback(HandleRecommendedCapacityChange);
            _thumbnailBlock.OnNewThumbnailSelected += HandleThumbnailChanged;
            _worldDebuggingToggle.RegisterValueChangedCallback(HandleWorldDebuggingChange);

            _discardChangesButton.clicked += HandleDiscardChangesClick;
            _saveChangesButton.clicked += HandleSaveChangesClick;

            root.schedule.Execute(CheckBlueprintChanges).Every(1000);
        }
        
        private void ClearWorldData(PipelineManager pm)
        {
            // Do not clear blueprint IDs during a build or upload
            if (_buildState != SdkBuildState.Building && _uploadState != SdkUploadState.Uploading)
            {
                Core.Logger.LogError("Loaded data for an world we do not own, clearing blueprint ID");
                Undo.RecordObject(pm, "Cleared the blueprint ID we do not own");
                pm.blueprintId = "";
                _lastBlueprintId = "";
            }
            _worldData = new VRCWorld();
            IsNewWorld = true;
        }

        private void RestoreSessionState()
        {
            _worldData.Name = WorldBuilderSessionState.WorldName;
            _nameField.SetValueWithoutNotify(_worldData.Name);

            _worldData.Description = WorldBuilderSessionState.WorldDesc;
            _descriptionField.SetValueWithoutNotify(_worldData.Description);

            _worldData.Tags = new List<string>(WorldBuilderSessionState.WorldTags.Split('|', StringSplitOptions.RemoveEmptyEntries).Where(t => !string.IsNullOrWhiteSpace(t)));
            _tagsField.tags = _contentWarningsField.SelectedOptions = _worldData.Tags;

            _worldData.Capacity = WorldBuilderSessionState.WorldCapacity;
            _capacityField.SetValueWithoutNotify(_worldData.Capacity);
            _recommendedCapacityField.SetValueWithoutNotify(WorldBuilderSessionState.WorldRecommendedCapacity);

            _worldDebuggingToggle.SetValueWithoutNotify(_worldData.Tags.Contains("debug_allowed"));

            _newThumbnailImagePath = WorldBuilderSessionState.WorldThumbPath;
            if (!string.IsNullOrWhiteSpace(_newThumbnailImagePath))
                _thumbnail.SetImage(_newThumbnailImagePath);
        }

        #region Event Handlers

        private void HandlePanelDetach(DetachFromPanelEvent evt)
        {
            EditorSceneManager.sceneClosed -= HandleSceneClosed;
        }

        // This auto-cancels uploads when the user changes scenes
        private void HandleSceneClosed(Scene scene)
        {
            if (_buildState == SdkBuildState.Building) return;
            if (_worldUploadCancellationTokenSource == null) return;
            _worldUploadCancellationTokenSource.Cancel();
            _worldUploadCancellationTokenSource = null;
        }

        private void HandleSdkPanelDisable(object sender, EventArgs evt)
        {
            if (_worldUploadCancellationTokenSource == null) return;
            _worldUploadCancellationTokenSource.Cancel();
            _worldUploadCancellationTokenSource = null;
        }

        private void HandleFoldoutToggle(ChangeEvent<bool> evt)
        {
            SessionState.SetBool($"{WorldBuilderSessionState.SESSION_STATE_PREFIX}.Foldout.{((VisualElement) evt.currentTarget).name}", evt.newValue);
        }
        
        private void HandleNameChange(ChangeEvent<string> evt)
        {
            _worldData.Name = evt.newValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldName = _worldData.Name;

            // do not allow empty names
            _saveChangesButton.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue));
            IsContentInfoDirty = CheckDirty();
        }

        private void HandleDescriptionChange(ChangeEvent<string> evt)
        {
            _worldData.Description = evt.newValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldDesc = _worldData.Description;

            IsContentInfoDirty = CheckDirty();
        }

        private void HandleCapacityChange(ChangeEvent<int> evt)
        {
            int clampedValue = Mathf.Clamp(evt.newValue, 1, WORLD_MAX_CAPAITY);
            if (clampedValue != evt.newValue)
            {
                _capacityField.SetValueWithoutNotify(clampedValue);
            }

            _worldData.Capacity = clampedValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldCapacity = _worldData.Capacity;

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleRecommendedCapacityChange(ChangeEvent<int> evt)
        {
            int clampedValue = Mathf.Clamp(evt.newValue, 1, _worldData.Capacity);
            if (clampedValue != evt.newValue)
            {
                _recommendedCapacityField.SetValueWithoutNotify(clampedValue);
            }

            _worldData.RecommendedCapacity = clampedValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldRecommendedCapacity = _worldData.RecommendedCapacity;

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleWorldDebuggingChange(ChangeEvent<bool> evt)
        {
            if (_worldData.Tags == null)
            {
                _worldData.Tags = new List<string>();
            }

            if (evt.newValue)
            {
                if (!_worldData.Tags.Contains("debug_allowed"))
                {
                    _worldData.Tags.Add("debug_allowed");
                }
            }
            else
            {
                if (_worldData.Tags.Contains("debug_allowed"))
                {
                    _worldData.Tags.Remove("debug_allowed");
                }
            }

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleSelectThumbnailClick()
        {
            var imagePath = EditorUtility.OpenFilePanel("Select thumbnail", "", "png");
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            _newThumbnailImagePath = imagePath;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldThumbPath = _newThumbnailImagePath;

            _thumbnail.SetImage(_newThumbnailImagePath);
            IsContentInfoDirty = CheckDirty();
        }

        private void HandleAddTag(object sender, string tag)
        {
            if (_worldData.Tags == null)
                _worldData.Tags = new List<string>();

            var formattedTag = "author_tag_" + tag.ToLowerInvariant().Replace(' ', '_');
            if (string.IsNullOrWhiteSpace(formattedTag)) return;
            if (_worldData.Tags.Contains(formattedTag)) return;
            
            _worldData.Tags.Add(formattedTag);
            _tagsField.tags = _contentWarningsField.SelectedOptions = _worldData.Tags;

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }

        private void HandleRemoveTag(object sender, string tag)
        {
            if (_worldData.Tags == null)
                _worldData.Tags = new List<string>();

            if (!_worldData.Tags.Contains(tag))
                return;

            _worldData.Tags.Remove(tag);
            _tagsField.tags = _contentWarningsField.SelectedOptions = _worldData.Tags;

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }

        private void HandleToggleTag(object sender, string tag)
        {
            if (_worldData.Tags == null)
                _worldData.Tags = new List<string>();

            if (_worldData.Tags.Contains(tag))
                _worldData.Tags.Remove(tag);
            else
                _worldData.Tags.Add(tag);

            _tagsField.tags = _contentWarningsField.SelectedOptions = _worldData.Tags;

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleThumbnailChanged(object sender, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;
            
            _newThumbnailImagePath = imagePath;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldThumbPath = _newThumbnailImagePath;
            
            _thumbnail.SetImage(_newThumbnailImagePath);
            IsContentInfoDirty = CheckDirty();
        }
        
        private async void HandleDiscardChangesClick()
        {
            _worldData = _originalWorldData;
            _worldData.Tags = new List<string>(_originalWorldData.Tags);
            
            _nameField.value = _worldData.Name;
            _descriptionField.value = _worldData.Description;
            _tagsField.tags = _contentWarningsField.OriginalOptions = _worldData.Tags;
            _contentWarningsField.SelectedOptions = _worldData.Tags;
            _lastUpdatedLabel.text = _worldData.UpdatedAt != DateTime.MinValue ? _worldData.UpdatedAt.ToString() : _worldData.CreatedAt.ToString();
            _versionLabel.text = _worldData.Version.ToString();
            _worldDebuggingToggle.value = _worldData.Tags?.Contains("debug_allowed") ?? false;
            _nameField.Reset();
            _descriptionField.Reset();
            _newThumbnailImagePath = null;
            await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl);
            IsContentInfoDirty = false;
        }
        
        private async void HandleSaveChangesClick()
        {
            _tagsField.StopEditing();
            UiEnabled = false;

            if (_nameField.IsPlaceholder() || string.IsNullOrWhiteSpace(_nameField.text))
            {
                Debug.LogError("Name cannot be empty");
                return;
            }

            if (_descriptionField.IsPlaceholder())
            {
                _worldData.Description = "";
            }

            _worldUploadCancellationTokenSource = new CancellationTokenSource();
            _worldUploadCancellationToken = _worldUploadCancellationTokenSource.Token;
            
            if (!string.IsNullOrWhiteSpace(_newThumbnailImagePath))
            {
                _builderProgress.ClearProgress();

                // to avoid loss of exceptions, we hoist it into a local function
                async void Progress(string status, float percentage)
                {
                    // these callbacks can be dispatched off-thread, so we ensure we're main thread pinned
                    await UniTask.SwitchToMainThread();
                    _builderProgress.SetProgress(new BuilderProgress.ProgressBarStateData
                    {
                        Visible = true,
                        Progress = percentage * 0.8f,
                        Text = status
                    });
                }
                
                _builderProgress.SetCancelButtonVisibility(true);

                _newThumbnailImagePath = VRC_EditorTools.CropImage(_newThumbnailImagePath, 800, 600);
                VRCWorld updatedWorld;
                try
                {
                    updatedWorld = await VRCApi.UpdateWorldImage(
                        _worldData.ID,
                        _worldData,
                        _newThumbnailImagePath,
                        Progress, _worldUploadCancellationToken);

                    // also need to update the base world data
                    if (!WorldDataEqual())
                    {
                        _builderProgress.SetProgress(new BuilderProgress.ProgressBarStateData
                        {
                            Visible = true,
                            Text = "Saving World Changes...",
                            Progress = 1f
                        });
                        updatedWorld =
                            await VRCApi.UpdateWorldInfo(_worldData.ID, _worldData, _worldUploadCancellationToken);
                    }
                }
                catch (ApiErrorException e)
                {
                    InfoUpdateError(this, e.ErrorMessage);
                    return;
                }
                catch (Exception e)
                {
                    InfoUpdateError(this, e.Message);
                    return;
                }
                
                _worldData = updatedWorld;
                _originalWorldData = updatedWorld;
                await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl, _worldUploadCancellationToken);
                _contentWarningsField.OriginalOptions = _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());
                _tagsField.tags = _contentWarningsField.SelectedOptions = _worldData.Tags ?? new List<string>();
                _newThumbnailImagePath = null;
            }
            else
            {
                _builderProgress.SetProgress(new BuilderProgress.ProgressBarStateData
                {
                    Visible = true,
                    Text = "Saving World Changes...",
                    Progress = 1f
                });
                var updatedWorld = await VRCApi.UpdateWorldInfo(_worldData.ID, _worldData, _worldUploadCancellationToken);

                var successLabel = new Label("Your world was successfully updated");
                successLabel.AddToClassList("p-2");
                successLabel.AddToClassList("text-center");
                
                _worldData = updatedWorld;
                _originalWorldData = updatedWorld;
                _contentWarningsField.OriginalOptions = _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());
                _contentWarningsField.SelectedOptions = _tagsField.tags = new List<string>(_worldData.Tags ?? new List<string>());

                await _builder.ShowBuilderNotification("World Updated", successLabel, "green", 3000);
            }
            
            _builderProgress.SetCancelButtonVisibility(false);
            _builderProgress.HideProgress();

            UiEnabled = true;
            _nameField.value = _worldData.Name;
            _descriptionField.value = _worldData.Description;
            _lastUpdatedLabel.text = _worldData.UpdatedAt != DateTime.MinValue ? _worldData.UpdatedAt.ToString() : _worldData.CreatedAt.ToString();
            _versionLabel.text = _worldData.Version.ToString();
            _nameField.Reset();
            _descriptionField.Reset();
            IsContentInfoDirty = false;
        }
        
        private void HandlePublishClick()
        {
            if (_worldData.ReleaseStatus == "private")
            {
                _labsPublishModal.Open();
                _labsPublishModal.OnClose += (_, _) => { HandlePublishWorldCancel(); };
                return;
            }

            _unpublishModal.Open();
            _unpublishModal.OnClose += (_, _) => { HandleUnpublishWorldCancel(); };
        }

        private async void HandlePublishWorldConfirm()
        {
            try
            {
                await VRCApi.PublishWorld(_worldData.ID);
                await _builder.ShowBuilderNotification("Published world to Community Labs",
                    new GenericBuilderNotification("Your world is now visible to other players!", null,
                        "See it on the VRChat Website",
                        () => { Application.OpenURL($"https://vrchat.com/home/world/{_worldData.ID}"); }),
                    "green", 5000);
                CreateContentInfoGUI(_visualRoot);
            }
            catch (Exception e)
            {
                var message = e is ApiErrorException exception ? exception.ErrorMessage : e.Message;
                await _builder.ShowBuilderNotification("Failed to publish world",
                    new GenericBuilderNotification(
                        "Something went wrong when publishing your world to Community Labs", message),
                    "red", 5000);
                Debug.LogException(e);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
            }
        }

        private void HandlePublishWorldCancel()
        {
            _labsPublishModal.Close();
            _visibilityPopup.value = "private";
        }

        private async void HandleUnpublishWorldConfirm()
        {
            try
            {
                await VRCApi.UnpublishWorld(_worldData.ID);
                await _builder.ShowBuilderNotification("Unpublished world",
                    new GenericBuilderNotification(
                        "Your world is private. You can still access it via direct links", null,
                        "See it on the VRChat Website",
                        () => { Application.OpenURL($"https://vrchat.com/home/world/{_worldData.ID}"); }),
                    "green", 5000);
                CreateContentInfoGUI(_visualRoot);
            }
            catch (Exception e)
            {
                var message = e is ApiErrorException exception ? exception.ErrorMessage : e.Message;
                await _builder.ShowBuilderNotification("Failed to unpublish world",
                    new GenericBuilderNotification("Something went wrong when unpublishing your world",
                        message), "red", 5000);
                Debug.LogException(e);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
            }
        }

        private void HandleUnpublishWorldCancel()
        {
            _unpublishModal.Close();
            _visibilityPopup.value = "public";
        }

        #endregion

        private void SetThumbnailImage(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var newThumbnail = new Texture2D(2, 2);
            newThumbnail.LoadImage(bytes);
            _thumbnail.SetImage(newThumbnail);
        }

        private bool WorldDataEqual()
        {
            return _worldData.Name.Equals(_originalWorldData.Name) &&
                   _worldData.Description.Equals(_originalWorldData.Description) &&
                   _worldData.Tags.SequenceEqual(_originalWorldData.Tags) &&
                   (_worldData.PreviewYoutubeId == _originalWorldData.PreviewYoutubeId) && // yt id can be null
                   _worldData.Capacity.Equals(_originalWorldData.Capacity) &&
                   _worldData.RecommendedCapacity.Equals(_originalWorldData.RecommendedCapacity);
        }

        private bool CheckDirty()
        {
            // we ignore the diffs for new worlds, since they're not published yet
            if (IsNewWorld) return false;
            if (string.IsNullOrWhiteSpace(_worldData.ID) || string.IsNullOrWhiteSpace(_originalWorldData.ID))
                return false;
            return !WorldDataEqual()|| !string.IsNullOrWhiteSpace(_newThumbnailImagePath);
        }

        private void CheckBlueprintChanges()
        {
            if (!UiEnabled) return;
            if (_pipelineManagers.Length == 0) return;
            var blueprintId = _pipelineManagers[0].blueprintId;
            if (_lastBlueprintId == blueprintId) return;
            CreateContentInfoGUI(_visualRoot);
            _lastBlueprintId = blueprintId;
            OnContentChanged?.Invoke(this, EventArgs.Empty);
        }
        
        #region SDK Build Action Types

        private enum SDKBuildActionType
        {
            BuildAndPublish,
            BuildAndTest,
            BuildAndReload,
            ReloadLastBuild,
            TestLastBuild,
        }

        private string GetBuildTypeText(SDKBuildActionType actionType)
        {
            switch (actionType)
            {
                case SDKBuildActionType.BuildAndPublish: return "Build & Publish Your World Online";
                case SDKBuildActionType.BuildAndTest: return "Build & Test Your World";
                case SDKBuildActionType.BuildAndReload: return "Build & Reload Your World";
                case SDKBuildActionType.ReloadLastBuild: return "Reload Your Last Build";
                case SDKBuildActionType.TestLastBuild: return "Test Your Last Build";
            }
            throw new Exception($"Unknown SDK Build Action {actionType}");
        }
        
        private record SDKBuildAction
        {
            public SDKBuildActionType BuildActionType;
            public Action OnMainActionClicked;
            public Action<VisualElement> OnSetup; // Called when the user switched the build type to this build type
        }

        private List<SDKBuildAction> _sdkBuildActions;
        private SDKBuildAction _selectedSDKBuildAction;
        private List<BuildTarget> _selectedBuildTargets = new();

        private void ResetOptionVisibility(VisualElement root)
        {
            var isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            var isIOS = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            
            var forceNonVRToggle = root.Q<VisualElement>("force-non-vr-container");
            var numClientsContainer = root.Q<VisualElement>("num-clients-container");
            var enableWorldReloadContainer = root.Q<VisualElement>("enable-world-reload-container");

            if (isAndroid || isIOS)
            {
                // Android doesn't use these settings for local builds
                forceNonVRToggle.EnableInClassList("d-none", true);
                numClientsContainer.EnableInClassList("d-none", true);
                enableWorldReloadContainer.EnableInClassList("d-none", true);
            }
            else
            {
                forceNonVRToggle.EnableInClassList("d-none", false);
                numClientsContainer.EnableInClassList("d-none", false);
                enableWorldReloadContainer.EnableInClassList("d-none", false);
            }
        }

        private void OnMainActionClicked()
        {
            SDKBuildAction selected = _sdkBuildActions
                .FirstOrDefault(x => x.BuildActionType.ToString() == VRCSettings.SDKWorldBuildType);
            if (selected != null && selected.OnMainActionClicked != null)
            {
                selected.OnMainActionClicked();
            }
        }

        private void BuildAndPublishSetup(VisualElement root)
        {
            var mainActionButton = root.Q<Button>("main-action-button");
            var isMPB = _selectedBuildTargets.Count > 1;
            if (_selectedBuildTargets.Count == 0)
            {
                mainActionButton.text = "You need to select at least one platform";
                mainActionButton.SetEnabled(false);
            }
            else
            {
                mainActionButton.text = isMPB ? "Multi-Platform Build & Publish" : "Build & Publish";
                mainActionButton.SetEnabled(true);
            }

            ResetOptionVisibility(root);
            
            var forceNonVRToggleContainer = root.Q<VisualElement>("force-non-vr-container");
            var numClientsContainer = root.Q<VisualElement>("num-clients-container");
            var enableWorldReloadContainer = root.Q<VisualElement>("enable-world-reload-container");
            
            forceNonVRToggleContainer.EnableInClassList("d-none", true);
            numClientsContainer.EnableInClassList("d-none", true);
            enableWorldReloadContainer.EnableInClassList("d-none", true);
        }

        private async void OnBuildAndPublishAction()
        {
            VRC_SdkBuilder.ActiveBuildType = VRC_SdkBuilder.BuildType.Publish;

            // Kick off the multi-platform build process
            var isMPB = _selectedBuildTargets.Count > 1;
            if (isMPB)
            {
                StartMultiPlatformBuild(this, (_worldData, _newThumbnailImagePath));
                return;
            }

            UiEnabled = false;
            
            SubscribePanelToBuildCallbacks();

            _worldUploadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await BuildAndUpload(_worldData, _newThumbnailImagePath, _worldUploadCancellationTokenSource.Token);
            }
            finally
            {
                    
                UnsubscribePanelFromBuildCallbacks();
            }
        }
        
        private void BuildAndTestSetup(VisualElement root)
        {
            var mainActionButton = root.Q<Button>("main-action-button");
            mainActionButton.text = "Build & Test";
            
            ResetOptionVisibility(root);
        }

        private async void OnBuildAndTestAction()
        {
            VRC_SdkBuilder.ActiveBuildType = VRC_SdkBuilder.BuildType.Test;
                
            async void BuildSuccess(object sender, string path)
            {
                BuildStageSuccess(sender, path);
                    
                await Task.Delay(500);
                
                _builderProgress.HideProgress();
                UiEnabled = true;
                _thumbnail.Loading = false;
                RevertThumbnail();

                ShowBuildSuccessNotification();
            }

            SubscribePanelToBuildCallbacks(buildSuccess: BuildSuccess);

            try
            {
                await BuildAndTest();
            }
            finally
            {
                UnsubscribePanelFromBuildCallbacks(buildSuccess: BuildSuccess);
            }
        }
        
        private void BuildAndReloadSetup(VisualElement root)
        {
            var mainActionButton = root.Q<Button>("main-action-button");
            mainActionButton.text = "Build & Reload";
            
            ResetOptionVisibility(root);
        }

        private async void OnBuildAndReloadAction()
        {
            await Build();
        }
        
        private void ReloadLastBuildSetup(VisualElement root)
        {
            var mainActionButton = root.Q<Button>("main-action-button");
            mainActionButton.text = "Reload Last Build";
            
            ResetOptionVisibility(root);
        }

        private void OnReloadLastBuildAction()
        {
            // Todo: get this from settings or make key a const
            string path = EditorPrefs.GetString("lastVRCPath");
            if (File.Exists(path))
            {
                File.SetLastWriteTimeUtc(path, DateTime.Now);
            }
            else
            {
                Debug.LogWarning($"Cannot find last built scene, please Rebuild.");
            }
        }
        
        private void TestLastBuildSetup(VisualElement root)
        {
            var mainActionButton = root.Q<Button>("main-action-button");
            mainActionButton.text = "Test Last Build";
            
            ResetOptionVisibility(root);
        }

        private async void OnTestLastBuildAction()
        {
            await TestLastBuild();
        }
        
        #endregion
        
        
        public virtual void CreateBuildGUI(VisualElement root)
        {
            var tree = Resources.Load<VisualTreeAsset>("VRCSdkWorldBuilderBuildLayout");
            tree.CloneTree(root);
            _buildVisualRoot = root;
            var styles = Resources.Load<StyleSheet>("VRCSdkWorldBuilderBuildStyles");
            if (!root.styleSheets.Contains(styles))
            {
                root.styleSheets.Add(styles);
            }
            
            _buildButtonsBlock = root.Q("build-buttons-block");
            _builderProgress = root.Q<BuilderProgress>("progress-bar");
            
            // Setup build types and their associated action when clicking the main action button
            _sdkBuildActions = new List<SDKBuildAction>()
            {
                new SDKBuildAction{BuildActionType = SDKBuildActionType.BuildAndPublish, OnSetup = BuildAndPublishSetup, OnMainActionClicked = OnBuildAndPublishAction},
                new SDKBuildAction{BuildActionType = SDKBuildActionType.BuildAndTest, OnSetup = BuildAndTestSetup, OnMainActionClicked = OnBuildAndTestAction},
                new SDKBuildAction{BuildActionType = SDKBuildActionType.BuildAndReload, OnSetup = BuildAndReloadSetup, OnMainActionClicked = OnBuildAndReloadAction},
                new SDKBuildAction{BuildActionType = SDKBuildActionType.ReloadLastBuild, OnSetup = ReloadLastBuildSetup, OnMainActionClicked = OnReloadLastBuildAction},
                new SDKBuildAction{BuildActionType = SDKBuildActionType.TestLastBuild, OnSetup = TestLastBuildSetup, OnMainActionClicked = OnTestLastBuildAction},
            };
            
            var platformPopup = root.Q<PlatformSwitcherPopup>("platform-switcher-popup");
            {
                var buildTypeContainer = root.Q<VisualElement>("build-type-container");
                List<string> buildTypeOptions = _sdkBuildActions.Select(x => GetBuildTypeText(x.BuildActionType)).ToList(); 

                int selectedBuildTypeIndex = _sdkBuildActions.FindIndex(x=>x.BuildActionType.ToString() == VRCSettings.SDKWorldBuildType);
                if (selectedBuildTypeIndex < 0 || selectedBuildTypeIndex >= buildTypeOptions.Count)
                {
                    // Reset to a known good index if out of bounds
                    selectedBuildTypeIndex = 0;
                    VRCSettings.SDKWorldBuildType = _sdkBuildActions[0].BuildActionType.ToString();
                }
                
                var buildTypePopup = new PopupField<string>(null, buildTypeOptions, selectedBuildTypeIndex)
                {
                    name = "build-type-dropdown"
                };
                // Unity dropdown menus filter out a single '&' character
                buildTypePopup.formatListItemCallback += s => s.Replace("&", "&&"); 
                buildTypePopup.AddToClassList("ml-0");
                buildTypePopup.AddToClassList("flex-grow-1");
                
                if (_sdkBuildActions[selectedBuildTypeIndex].OnSetup != null)
                {
                    // Set up the currently selected build action
                    _sdkBuildActions[selectedBuildTypeIndex].OnSetup(root);
                }

                _selectedSDKBuildAction = _sdkBuildActions[selectedBuildTypeIndex];

                buildTypeContainer.Insert(1, buildTypePopup);

                var mainActionButton = root.Q<Button>("main-action-button");
                buildTypePopup.RegisterValueChangedCallback(evt =>
                {
                    SDKBuildAction selected = _sdkBuildActions
                        .FirstOrDefault(x => GetBuildTypeText(x.BuildActionType) == evt.newValue);
                    _selectedSDKBuildAction = selected;
                    mainActionButton.SetEnabled(true);
                    if (selected != null)
                    {
                        VRCSettings.SDKWorldBuildType = selected.BuildActionType.ToString();
                        if (selected.OnSetup != null)
                        {
                            selected.OnSetup(root);
                        }

                        // Only Build & Publish supports multi-platform
                        if (selected.BuildActionType != SDKBuildActionType.BuildAndPublish)
                        {
                            _selectedBuildTargets = new List<BuildTarget> { VRC_EditorTools.GetCurrentBuildTargetEnum() };
                            platformPopup.SelectedOptions = _selectedBuildTargets;
                        }
                    }
                    platformPopup.Refresh();
                });

                mainActionButton.clicked += OnMainActionClicked;
            }

            {
                _selectedBuildTargets = WorldBuilderSessionState.WorldPlatforms.Count > 0
                    ? WorldBuilderSessionState.WorldPlatforms
                    : new List<BuildTarget> { VRC_EditorTools.GetCurrentBuildTargetEnum() };
                // fire the build type setup on initial platform load
                _selectedSDKBuildAction?.OnSetup?.Invoke(root);
                platformPopup.SelectedOptions = _selectedBuildTargets;
                platformPopup.OnToggleOption += (_, target) =>
                {
                    if (platformPopup.SelectedOptions.Contains(target))
                    {
                        _selectedBuildTargets.Remove(target);
                        platformPopup.SelectedOptions = _selectedBuildTargets;
                        WorldBuilderSessionState.WorldPlatforms = _selectedBuildTargets;
                        return;
                    }
                    
                    _selectedBuildTargets.Add(target);
                    // If the current action isn't build & publish - we only support one target platform at a time
                    // This invokes platform switching logic
                    if (_selectedSDKBuildAction?.BuildActionType != SDKBuildActionType.BuildAndPublish)
                    {
                        _selectedBuildTargets.RemoveAll(t => t != target);
                    }
                    platformPopup.SelectedOptions = _selectedBuildTargets;
                    WorldBuilderSessionState.WorldPlatforms = _selectedBuildTargets;
                };
                platformPopup.OnPopupClosed += (_, platforms) =>
                {
                    var currentTarget = VRC_EditorTools.GetCurrentBuildTargetEnum();
                    // If only one target is selected - ask to switch
                    if (platforms.Count == 1 && platforms[0] != currentTarget)
                    {
                        if (EditorUtility.DisplayDialog("Build Target Switcher",
                                $"Are you sure you want to switch your build target to {VRC_EditorTools.GetTargetName(platforms[0])}? This could take a while.",
                                "Confirm", "Cancel"))
                        {
                            EditorUserBuildSettings.selectedBuildTargetGroup =
                                VRC_EditorTools.GetBuildTargetGroupForTarget(platforms[0]);
                            var switched =
                                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(
                                    EditorUserBuildSettings.selectedBuildTargetGroup, platforms[0]);
                            if (!switched)
                            {
                                _builder.ShowBuilderNotification(
                                    $"Failed to switch to {VRC_EditorTools.GetTargetName(platforms[0])} target platform",
                                    new GenericBuilderNotification(
                                        $"Check if the Platform Support for {VRC_EditorTools.GetTargetName(platforms[0])} is installed in the Unity Hub",
                                        "Unity Console might have more information",
                                        "Show Console",
                                        VRC_EditorTools.OpenConsoleWindow
                                    ),
                                    "red"
                                ).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            platformPopup.SelectedOptions = new List<BuildTarget> { currentTarget };
                        }
                    }

                    _selectedBuildTargets = platformPopup.SelectedOptions.ToList();
                    _selectedSDKBuildAction?.OnSetup?.Invoke(root);
                    WorldBuilderSessionState.WorldPlatforms = _selectedBuildTargets;
                };
            }
            
            _v3Block = root.Q("v3-block");
            
            var numClientsField = root.Q<IntegerField>("num-clients");
            numClientsField.RegisterValueChangedCallback(evt =>
            {
                VRCSettings.NumClients = Mathf.Clamp(evt.newValue, 0, 8);
                (evt.target as IntegerField)?.SetValueWithoutNotify(VRCSettings.NumClients);
            });
            numClientsField.SetValueWithoutNotify(VRCSettings.NumClients);

            var forceNonVrToggle = root.Q<Toggle>("force-non-vr");
            forceNonVrToggle.RegisterValueChangedCallback(evt =>
            {
                VRCSettings.ForceNoVR = evt.newValue;
            });
            forceNonVrToggle.SetValueWithoutNotify(VRCSettings.ForceNoVR);
            
            var enableWorldReloadToggle = root.Q<Toggle>("enable-world-reload");
            enableWorldReloadToggle.RegisterValueChangedCallback(evt =>
            {
                VRCSettings.WatchWorlds = evt.newValue;
            });
            enableWorldReloadToggle.SetValueWithoutNotify(VRCSettings.WatchWorlds);
            
            _mainBuildActionDisabledBlock = root.Q<VisualElement>("main-action-disabled-block");
            _mainBuildActionDisabledText = root.Q<Label>("main-action-disabled-text");

            SetupExtraPanelUI();

            root.schedule.Execute(() =>
            {
                if (!_builder || APIUser.CurrentUser == null) return;
                var buildsAllowed = _builder.NoGuiErrorsOrIssues() || APIUser.CurrentUser.developerType == APIUser.DeveloperType.Internal;
                
                var isBuildingMPB = VRCMultiPlatformBuild.MPB;
                
                _buildButtonsBlock.SetEnabled(!isBuildingMPB);
                
                _mainBuildActionDisabledBlock.EnableInClassList("d-none", buildsAllowed);
                
                SDKBuildAction selectedAction = _sdkBuildActions
                    .FirstOrDefault(x => x.BuildActionType.ToString() == VRCSettings.SDKWorldBuildType);
                if (selectedAction == null) throw new Exception($"Unable to identify selected build action {VRCSettings.SDKWorldBuildType}");
                
                var lastBuildUrl = VRC_SdkBuilder.GetLastUrl();

                if (IsLocalTesting(selectedAction.BuildActionType))
                {
                    if (!PlatformSupportsBuildAndTest())
                    {
                        _mainBuildActionDisabledText.text = "Building & testing on this platform is not supported";
                        _mainBuildActionDisabledBlock.RemoveFromClassList("d-none");
                        return;
                    }
                    if (selectedAction.BuildActionType == SDKBuildActionType.ReloadLastBuild ||
                        selectedAction.BuildActionType == SDKBuildActionType.TestLastBuild)
                    {
                        if (lastBuildUrl == null)
                        {
                            _mainBuildActionDisabledText.text = "No last build found";
                            _mainBuildActionDisabledBlock.RemoveFromClassList("d-none");
                            return;
                        }
                    }
                }
                else
                { // Online publishing
                    if (IsNewWorld && (string.IsNullOrWhiteSpace(_worldData.Name) || string.IsNullOrWhiteSpace(_newThumbnailImagePath)))
                    {
                        _mainBuildActionDisabledText.text = "Please set a name and thumbnail before uploading";
                        _mainBuildActionDisabledBlock.RemoveFromClassList("d-none");
                        return;
                    }
                }
                _mainBuildActionDisabledBlock.AddToClassList("d-none");
            }).Every(1000);
        }

        private bool IsLocalTesting(SDKBuildActionType type)
        {
            switch (type)
            {
                case SDKBuildActionType.BuildAndTest:
                case SDKBuildActionType.BuildAndReload:
                case SDKBuildActionType.ReloadLastBuild:
                case SDKBuildActionType.TestLastBuild:
                        return true;
                    
            }

            return false;
        }

        private bool PlatformSupportsBuildAndTest()
        {
            return VRC_EditorTools.GetCurrentBuildTargetEnum() is 
                BuildTarget.StandaloneWindows64 or 
                BuildTarget.Android or 
                BuildTarget.iOS;
        }

        public virtual void SetupExtraPanelUI()
        {
            
        }
        
        private async void StartMultiPlatformBuild(object sender, object data)
        {
            if (VRCMultiPlatformBuild.MPBState ==
                VRCMultiPlatformBuild.MultiPlatformBuildState.Building)
            {
                return;
            }
            
            // If we're already in MPB - restore the build target list
            if (VRCMultiPlatformBuild.MPB)
            {
                _selectedBuildTargets = VRCMultiPlatformBuild.MPBPlatformsList;
            }
            
            var (content, thumbnailPath) = ((VRCWorld content, string thumbnailPath)) data;

            if (string.IsNullOrWhiteSpace(content.ID) && string.IsNullOrWhiteSpace(content.Name))
            {
                return;
            }

            ContentInfoLoaded -= StartMultiPlatformBuild;
            
            UiEnabled = false;
            
            SubscribePanelToBuildCallbacks(buildError: MultiPlatformBuildError, uploadError: MultiPlatformUploadError);
            _worldUploadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await BuildAndUploadMultiPlatform(content, thumbnailPath, _worldUploadCancellationTokenSource.Token);
            }
            finally
            {
                UnsubscribePanelFromBuildCallbacks(buildError: MultiPlatformBuildError, uploadError: MultiPlatformUploadError);
            }
        }

        private async Task<(string path, string signature)> Build(bool runAfterBuild)
        {
            if (VRC_EditorTools.DryRunState)
            {
                return (string.Empty, string.Empty);
            }
            
            var buildBlocked = !VRCBuildPipelineCallbacks.OnVRCSDKBuildRequested(VRCSDKRequestedBuildType.Scene);
            if (buildBlocked)
            {
                throw await HandleBuildError(new BuildBlockedException("Build was blocked by the SDK callback"));
            }
            
            if (_builder == null || _scenes == null || _scenes.Length == 0)
            {
                throw await HandleBuildError(new BuilderException("Open the SDK panel to build and upload worlds"));
            }

            _builder.CheckedForIssues = false;
            _builder.ResetIssues();
            VRC_EditorTools.GetCheckProjectSetupMethod().Invoke(_builder, new object[] {});
            OnGUISceneCheck(_scenes[0]);
            var areLayersSetUp = UpdateLayers.AreLayersSetup();
            var isCollisionMatrixSetUp = UpdateLayers.IsCollisionLayerMatrixSetup();

            // add an error to block the build
            if (!areLayersSetUp || !isCollisionMatrixSetUp)
            {
                _builder.OnGUIError(_scenes[0], "You must address Layers and Collision Matrix issues before you can build.");
            }
            _builder.CheckedForIssues = true;
            if (!_builder.NoGuiErrorsOrIssuesForItem(_scenes[0]) || !_builder.NoGuiErrorsOrIssuesForItem(_builder))
            {
                var errorsList = new List<string>();
                errorsList.AddRange(_builder.GetGuiErrorsOrIssuesForItem(_scenes[0]).Select(i => i.issueText));
                errorsList.AddRange(_builder.GetGuiErrorsOrIssuesForItem(_builder).Select(i => i.issueText));
                throw await HandleBuildError(new ValidationException("World validation failed", errorsList));
            }

            EnvConfig.ConfigurePlayerSettings();
            EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", false);
            AssetExporter.CleanupUnityPackageExport(); // force unity package rebuild on next publish
            VRC_SdkBuilder.shouldBuildUnityPackage = false;
            VRC_SdkBuilder.PreBuildBehaviourPackaging();

            VRC_SdkBuilder.ClearCallbacks();

            var successTask = new TaskCompletionSource<(string path, string hash)>();
            var errorTask = new TaskCompletionSource<string>();

            VRC_SdkBuilder.RegisterBuildProgressCallback(OnBuildProgress);
            VRC_SdkBuilder.RegisterBuildErrorCallback(OnBuildError);
            VRC_SdkBuilder.RegisterBuildSuccessCallback(OnBuildSuccess);
            
            VRC_EditorTools.GetSetPanelBuildingMethod().Invoke(_builder, null);
            OnSdkBuildStart?.Invoke(this, EventArgs.Empty);
            _buildState = SdkBuildState.Building;
            OnSdkBuildStateChange?.Invoke(this, _buildState);
            
            await Task.Delay(100);
            
            try
            {
                if (!runAfterBuild)
                {
                    VRC_SdkBuilder.RunExportSceneResource();
                }
                else
                {
                    VRC_SdkBuilder.RunExportSceneResourceAndRun();
                }
            }
            catch (WorldAssetExportException e)
            {
                throw await HandleBuildError(e);
            }
            
            var result = await Task.WhenAny(successTask.Task, errorTask.Task);

            string bundlePath = null, worldSignature = null;
            (bundlePath, worldSignature) = result == successTask.Task ? successTask.Task.Result : (null, null);
            
            VRC_SdkBuilder.ClearCallbacks();

            if (bundlePath == null)
            {
                throw await HandleBuildError(new BuilderException(errorTask.Task.Result));
            }

            _buildState = SdkBuildState.Success;
            OnSdkBuildSuccess?.Invoke(this, bundlePath);
            OnSdkBuildStateChange?.Invoke(this, _buildState);

            await FinishBuild();

            PathToLastBuild = bundlePath;
            WorldSignatureOfLastBuild = worldSignature;

            return (bundlePath, worldSignature);

            void OnBuildProgress(object sender, string buildStatus)
            {
                OnSdkBuildProgress?.Invoke(sender, buildStatus);
            }

            void OnBuildSuccess(object _, (string path, string signature) buildResult)
            {
                successTask.TrySetResult(buildResult);
            }

            void OnBuildError(object _, string buildError)
            {
                errorTask.TrySetResult(buildError);
            }
        }
        
        private async Task FinishBuild()
        {
            await Task.Delay(100);
            _buildState = SdkBuildState.Idle;
            OnSdkBuildFinish?.Invoke(this, "World build finished");
            OnSdkBuildStateChange?.Invoke(this, _buildState);
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
        }
        
        private async Task<Exception> HandleBuildError(Exception exception)
        {
            if (exception is ValidationException ve)
            {
                Core.Logger.LogError("Validation issues encountered during build:");
                foreach (var error in ve.Errors)
                {
                    Core.Logger.LogError(error);
                }
            }
            OnSdkBuildError?.Invoke(this, exception.Message);
            _buildState = SdkBuildState.Failure;
            OnSdkBuildStateChange?.Invoke(this, _buildState);

            await FinishBuild();
            return exception;
        }
        
        private async Task<bool> Upload(VRCWorld world, string bundlePath, string worldSignature, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            if (VRC_EditorTools.DryRunState)
            {
                return true;
            }
            
            if (cancellationToken == default)
            {
               _worldUploadCancellationTokenSource = new CancellationTokenSource();
               _worldUploadCancellationToken = _worldUploadCancellationTokenSource.Token;
            }
            else
            {
               _worldUploadCancellationToken = cancellationToken;
            }

            if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
            {
                throw await HandleUploadError(new UploadException("Failed to find the built world bundle, the build likely failed"));
            }

            await VerifyUploadPermissions();

            bool mobile = ValidationEditorHelpers.IsMobilePlatform();
            if (ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge(ContentType.World, bundlePath, out int fileSize, mobile))
            {
                var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.World, mobile);
                throw await HandleUploadError(new UploadException(
                    $"World download size is too large for the target platform. {ValidationHelpers.FormatFileSize(fileSize)} > {ValidationHelpers.FormatFileSize(limit)}"));
            }
            
            if (ValidationEditorHelpers.CheckIfUncompressedAssetBundleFileTooLarge(ContentType.World, out int fileSizeUncompressed, mobile))
            {
                var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.World, mobile, false);
                throw await HandleUploadError(new UploadException(
                    $"World uncompressed size is too large for the target platform. {ValidationHelpers.FormatFileSize(fileSizeUncompressed)} > {ValidationHelpers.FormatFileSize(limit)}"));
            }

            VRC_EditorTools.GetSetPanelUploadingMethod().Invoke(_builder, null);
            _uploadState = SdkUploadState.Uploading;
            OnSdkUploadStateChange?.Invoke(this, _uploadState);
            OnSdkUploadStart?.Invoke(this, EventArgs.Empty);

            try
            {
                await Task.Delay(100, _worldUploadCancellationToken);
            }
            catch (TaskCanceledException)
            {
                throw await HandleUploadError(new UploadException("Upload Was Canceled"));
            }
            
            var pms = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (pms.Length == 0)
            {
                throw await HandleUploadError(new UploadException("The scene does not have a PipelineManager component present, make sure to add a SceneDescriptor before building and uploading"));
            }

            var pM = pms[0];

            var creatingNewWorld = string.IsNullOrWhiteSpace(pM.blueprintId) || string.IsNullOrWhiteSpace(world.ID);

            if (creatingNewWorld && (string.IsNullOrWhiteSpace(thumbnailPath) || !File.Exists(thumbnailPath)))
            {
                throw await HandleUploadError(new UploadException("You must provide a path to the thumbnail image when creating a new world"));
            }

            if (!creatingNewWorld)
            {
                var remoteData = await VRCApi.GetWorld(world.ID, cancellationToken: _worldUploadCancellationToken);
                if (APIUser.CurrentUser == null || remoteData.AuthorId != APIUser.CurrentUser?.id)
                {
                    throw await HandleUploadError(new OwnershipException("World's current ID belongs to a different user, assign a different ID"));
                }
            }

            if (string.IsNullOrWhiteSpace(pM.blueprintId))
            {
                Undo.RecordObject(pM, "Assigning a new ID");
                pM.AssignId(PipelineManager.ContentType.world);
            }

            await CheckCopyrightAgreement(pM, world);

            try
            {
                if (creatingNewWorld)
                {
                    thumbnailPath = VRC_EditorTools.CropImage(thumbnailPath, 800, 600);
                    _worldData = await VRCApi.CreateNewWorld(pM.blueprintId, world, bundlePath,
                        thumbnailPath, worldSignature,
                        (status, percentage) => { OnSdkUploadProgress?.Invoke(this, (status, percentage)); },
                        _worldUploadCancellationToken);
                }
                else
                {
                    _worldData = await VRCApi.UpdateWorldBundle(pM.blueprintId, world, bundlePath, worldSignature,
                        (status, percentage) => { OnSdkUploadProgress?.Invoke(this, (status, percentage)); },
                        _worldUploadCancellationToken);
                }
                
                _uploadState = SdkUploadState.Success;
                OnSdkUploadSuccess?.Invoke(this, _worldData.ID);
                
                var packageList = VRCAnalyticsTools.GetPackageList();
                try
                {
                    AnalyticsSDK.ProjectPublished(packageList.ToArray(), _worldData.ID,
                        AnalyticsSDK.ProjectType.World);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to send VPM manifest data to analytics");
                    Debug.LogException(e);
                }
                
                await FinishUpload();
            }
            catch (TaskCanceledException e)
            {
                AnalyticsSDK.WorldUploadFailed(pM.blueprintId, !creatingNewWorld);
                if (cancellationToken.IsCancellationRequested)
                {
                    Core.Logger.LogError("Request cancelled", API.LOG_CATEGORY);
                    throw await HandleUploadError(new UploadException("Request Cancelled", e));
                }
            }
            catch (ApiErrorException e)
            {
                AnalyticsSDK.WorldUploadFailed(pM.blueprintId, !creatingNewWorld);
                throw await HandleUploadError(new UploadException(e.ErrorMessage, e));
            }
            catch (BundleExistsException e)
            {
                if (!VRCMultiPlatformBuild.MPB)
                {
                    throw await HandleUploadError(new UploadException(e.Message, e));
                }

                SkipMPBUpload();
                return false;
            }
            catch (Exception e)
            {
                AnalyticsSDK.WorldUploadFailed(pM.blueprintId, !creatingNewWorld);
                throw await HandleUploadError(new UploadException(e.Message, e));
            }

            return true;
        }
        
        private void SkipMPBUpload()
        {
            _uploadState = SdkUploadState.Idle;
            OnSdkUploadFinish?.Invoke(this, "World upload skipped");
            OnSdkUploadStateChange?.Invoke(this, _uploadState);
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
            VRC_EditorTools.ToggleSdkTabsEnabled(_builder, true);
            _worldUploadCancellationToken = default;
        }

        
        private async Task FinishUpload()
        {
            await Task.Delay(100, _worldUploadCancellationToken);
            _uploadState = SdkUploadState.Idle;
            OnSdkUploadFinish?.Invoke(this, "World upload finished");
            OnSdkUploadStateChange?.Invoke(this, _uploadState);
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
            VRC_EditorTools.ToggleSdkTabsEnabled(_builder, true);
            _worldUploadCancellationToken = default;
        }
        
        private async Task<Exception> HandleUploadError(Exception exception)
        {
            OnSdkUploadError?.Invoke(this, exception.Message);
            _uploadState = SdkUploadState.Failure;
            OnSdkUploadStateChange?.Invoke(this, _uploadState);

            await FinishUpload();
            return exception;
        }
        
        private async Task CheckCopyrightAgreement(PipelineManager pM, VRCWorld world)
        {
            try
            {
                await VRCCopyrightAgreement.CheckCopyrightAgreement(pM, world);
            }
            catch (Exception e)
            {
                throw await HandleUploadError(e);
            }
        }

        private async Task VerifyUploadPermissions()
        {
            if (!APIUser.CurrentUser.canPublishWorlds)
            {
                VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
                throw await HandleBuildError(new BuildBlockedException("Current User does not have permissions to build and upload worlds"));
            }
        }

        

        #region Build Callbacks
        
        private void SubscribePanelToBuildCallbacks(EventHandler<object> buildStart = null,
            EventHandler<string> buildError = null, EventHandler<string> buildSuccess = null,
            EventHandler uploadStart = null, EventHandler<(string, float)> uploadProgress = null,
            EventHandler<string> uploadError = null, EventHandler<string> uploadSuccess = null,
            EventHandler<string> uploadFinish = null)
        {
            OnSdkBuildStart += buildStart ?? BuildStart;
            OnSdkBuildError += buildError ?? BuildError;
            OnSdkBuildSuccess += buildSuccess ?? BuildStageSuccess;

            OnSdkUploadStart += uploadStart ?? UploadStart;
            OnSdkUploadProgress += uploadProgress ?? UploadProgress;
            OnSdkUploadError += uploadError ?? UploadError;
            OnSdkUploadSuccess += uploadSuccess ?? UploadSuccess;
            OnSdkUploadFinish += uploadFinish ?? UploadFinish;
        }

        private void UnsubscribePanelFromBuildCallbacks(EventHandler<object> buildStart = null,
            EventHandler<string> buildError = null, EventHandler<string> buildSuccess = null,
            EventHandler uploadStart = null, EventHandler<(string, float)> uploadProgress = null,
            EventHandler<string> uploadError = null, EventHandler<string> uploadSuccess = null,
            EventHandler<string> uploadFinish = null)
        {
            OnSdkBuildStart -= buildStart ?? BuildStart;
            OnSdkBuildError -= buildError ?? BuildError;
            OnSdkBuildSuccess -= buildSuccess ?? BuildStageSuccess;

            OnSdkUploadStart -= uploadStart ?? UploadStart;
            OnSdkUploadProgress -= uploadProgress ?? UploadProgress;
            OnSdkUploadError -= uploadError ?? UploadError;
            OnSdkUploadSuccess -= uploadSuccess ?? UploadSuccess;
            OnSdkUploadFinish -= uploadFinish ?? UploadFinish;
        }
        
        private void BuildStart(object sender, object target)
        {
            UiEnabled = false;
            _thumbnail.Loading = true;
            _thumbnail.ClearImage();
            
            _builderProgress.SetProgress(new BuilderProgress.ProgressBarStateData
            {
                Visible = true,
                Text = "Building World",
                Progress = 0.0f
            });
        }
        private async void BuildError(object sender, string error)
        {
            Core.Logger.Log("Failed to build a world!");
            Core.Logger.LogError(error);

            VRC_SdkBuilder.ActiveBuildType = VRC_SdkBuilder.BuildType.None;

            await Task.Delay(100);
            _builderProgress.HideProgress();
            UiEnabled = true;
            _thumbnail.Loading = false;
            RevertThumbnail();
            
            await _builder.ShowBuilderNotification(
                "Build Failed",
                new WorldUploadErrorNotification(error),
                "red"
            );
        }
        
        private void MultiPlatformBuildError(object sender, string error)
        {
            if (Progress.Exists(VRCMultiPlatformBuild.MPBProgress))
            {
                Progress.Report(VRCMultiPlatformBuild.MPBProgress, 6, 6, $"{EditorUserBuildSettings.activeBuildTarget} bundle failed to build");
                Progress.Finish(VRCMultiPlatformBuild.MPBProgress, Progress.Status.Failed);
            }
            VRCMultiPlatformBuild.ClearMPBState();
            BuildError(sender, error);
        }
        
        private void BuildStageSuccess(object sender, string path)
        {
            VRC_SdkBuilder.ActiveBuildType = VRC_SdkBuilder.BuildType.None;
            _builderProgress.SetProgress(new BuilderProgress.ProgressBarStateData
            {
                Visible = true,
                Text = "World Built",
                Progress = 0.1f
            });
        }

        private async void RevertThumbnail()
        {
            if (IsNewWorld)
            {
                if (string.IsNullOrEmpty(_newThumbnailImagePath))
                {
                    _thumbnail.ClearImage();
                }
                else
                {
                    _thumbnail.SetImage(_newThumbnailImagePath);
                }
            }
            else
            {
                await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl);
            }
        }

        private void UploadStart(object sender, EventArgs e)
        {
            _thumbnail.ClearImage();
            _thumbnail.Loading = true;
            _builderProgress.SetCancelButtonVisibility(true);
            _builderProgress.OnCancel += (_, _) => CancelUpload();
            VRC_EditorTools.ToggleSdkTabsEnabled(_builder, false);
            _progressId = Progress.Start("World Upload", "Uploading World to VRChat", Progress.Options.Synchronous, VRCMultiPlatformBuild.MPBProgress);
        }

        private async void UploadProgress(object sender, (string status, float percentage) progress)
        {
            await UniTask.SwitchToMainThread();
            _builderProgress.SetProgress(new BuilderProgress.ProgressBarStateData
            {
                Visible = true,
                Text = progress.status,
                Progress = 0.2f + progress.percentage * 0.8f
            });
            _builderProgress.MarkDirtyRepaint();
            if (Progress.Exists(_progressId))
            {
                Progress.Report(_progressId, progress.percentage, progress.status);
            }
        }
        private async void UploadSuccess(object sender, string worldId)
        {
            await Task.Delay(100);
            _builderProgress.SetCancelButtonVisibility(false);
            _builderProgress.HideProgress();
            UiEnabled = true;

            _originalWorldData = _worldData;
            _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());
            _newThumbnailImagePath = null;

            await _builder.ShowBuilderNotification(
                "Upload Succeeded!",
                new WorldUploadSuccessNotification(worldId),
                "green"
            );
            
            CreateContentInfoGUI(_visualRoot);
        }
        private async void UploadError(object sender, string error)
        {
            Core.Logger.Log("Failed to upload a world!");
            Core.Logger.LogError(error);
            
            await Task.Delay(100);
            _builderProgress.SetCancelButtonVisibility(false);
            _builderProgress.HideProgress();
            UiEnabled = true;
            _thumbnail.Loading = false;
            RevertThumbnail();
            
            if (Progress.Exists(_platformProgressId))
            {
                Progress.Report(_platformProgressId, 2, 2, error);
                Progress.Finish(_platformProgressId, Progress.Status.Failed);
            }
            
            await _builder.ShowBuilderNotification(
                "Upload Failed",
                new WorldUploadErrorNotification(error),
                "red"
            );
        }
        
        private async void InfoUpdateError(object sender, string error)
        {
            Core.Logger.Log("Failed to update world info!");
            Core.Logger.LogError(error);
            
            await Task.Delay(100);
            _builderProgress.SetCancelButtonVisibility(false);
            _builderProgress.HideProgress();
            UiEnabled = true;
            _thumbnail.Loading = false;
            RevertThumbnail();
            
            if (Progress.Exists(_platformProgressId))
            {
                Progress.Report(_platformProgressId, 2, 2, error);
                Progress.Finish(_platformProgressId, Progress.Status.Failed);
            }
            
            await _builder.ShowBuilderNotification(
                "Update Failed",
                new GenericBuilderNotification(error),
                "red"
            );
        }
        
        private void MultiPlatformUploadError(object sender, string error)
        {
            if (Progress.Exists(VRCMultiPlatformBuild.MPBProgress))
            {
                Progress.Report(VRCMultiPlatformBuild.MPBProgress, 6, 6, $"{EditorUserBuildSettings.activeBuildTarget} bundle failed to upload");
                Progress.Finish(VRCMultiPlatformBuild.MPBProgress, Progress.Status.Failed);
            }
            VRCMultiPlatformBuild.ClearMPBState();
            UploadError(sender, error);
        }
        
        private void UploadFinish(object sender, string message)
        {
            _builderProgress.OnCancel -= (_, _) => CancelUpload();
            if (Progress.Exists(_progressId))
            {
                Progress.Finish(_progressId);
                _progressId = 0;
            }
        }

        private async void ShowBuildSuccessNotification()
        {
            await _builder.ShowBuilderNotification(
                "Build Succeeded!",
                new WorldBuildSuccessNotification(),
                "green"
            );
        }

        #endregion

        #endregion

        #region Public API Backing

        private SdkBuildState _buildState;
        private SdkUploadState _uploadState;

        private static CancellationTokenSource _worldUploadCancellationTokenSource;
        private CancellationToken _worldUploadCancellationToken;
        
        private static string PathToLastBuild
        {
            get => SessionState.GetString("VRC.SDK3.Editor_patToLastBuild", null);
            set => SessionState.SetString("VRC.SDK3.Editor_patToLastBuild", value);
        }

        private static string WorldSignatureOfLastBuild
        {
            get => SessionState.GetString("VRC.SDK3.Editor_worldSignatureLastBuild", null);
            set => SessionState.SetString("VRC.SDK3.Editor_worldSignatureLastBuild", value);
        }

        #endregion

        #region Public API
        
        public event EventHandler<object> OnSdkBuildStart;
        public event EventHandler<string> OnSdkBuildProgress;
        public event EventHandler<string> OnSdkBuildFinish;
        public event EventHandler<string> OnSdkBuildSuccess;
        public event EventHandler<string> OnSdkBuildError;
        public event EventHandler<SdkBuildState> OnSdkBuildStateChange;
        public SdkBuildState BuildState => _buildState;
        
        public event EventHandler OnSdkUploadStart;
        public event EventHandler<(string status, float percentage)> OnSdkUploadProgress;
        public event EventHandler<string> OnSdkUploadFinish;
        public event EventHandler<string> OnSdkUploadSuccess;
        public event EventHandler<string> OnSdkUploadError;
        public event EventHandler<SdkUploadState> OnSdkUploadStateChange;
        public SdkUploadState UploadState => _uploadState;

        public event EventHandler<object> ContentInfoLoaded;
        
        public async Task<string> Build()
        {
            var buildResult =  await Build(false);
            return buildResult.path;
        }
        
        public async Task<(string path, string signature)> BuildWithSignature()
        {
            return await Build(false);
        }
        
        public async Task BuildAndUpload(VRCWorld world, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            await BuildAndUpload(world, null, thumbnailPath, cancellationToken);
        }

        public async Task BuildAndUpload(VRCWorld world, string signature, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            if (VRC_EditorTools.DryRunState)
            {
                return;
            }
            
            // Front-run the Copyright Agreement to avoid blocking the upload
            var pms = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (pms.Length == 0)
            {
                throw await HandleUploadError(new UploadException("The scene does not have a PipelineManager component present, make sure to add a SceneDescriptor before building and uploading"));
            }
            
            if (string.IsNullOrWhiteSpace(pms[0].blueprintId))
            {
                Undo.RecordObject(pms[0], "Assigning a new ID");
                pms[0].AssignId(PipelineManager.ContentType.world);
            }

            await CheckCopyrightAgreement(pms[0], world);

            // Verify upload permissions before attempting to build
            await VerifyUploadPermissions();

            var buildResult = await BuildWithSignature();
            world.UdonProducts = _scenes[0].udonProducts;
            await Upload(world, buildResult.path, buildResult.signature, thumbnailPath, cancellationToken);
        }

        public async Task UploadLastBuild(VRCWorld world, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(PathToLastBuild))
            {
                OnSdkUploadError?.Invoke(this, "No last build found, you must build first");
                _uploadState = SdkUploadState.Failure;
                OnSdkUploadStateChange?.Invoke(this, _uploadState);
                await FinishUpload();
                throw new UploadException("No last build found, you must build first");
            }

            if (!File.Exists(PathToLastBuild))
            {
                PathToLastBuild = null;
                WorldSignatureOfLastBuild = null;
                OnSdkUploadError?.Invoke(this, "No last build found, you must build first");
                _uploadState = SdkUploadState.Failure;
                OnSdkUploadStateChange?.Invoke(this, _uploadState);
                await FinishUpload();
                throw new UploadException("No last build found, you must build first");
            }

            await Upload(world, PathToLastBuild, WorldSignatureOfLastBuild, thumbnailPath, cancellationToken);
        }
        
        private int _platformProgressId = -1;

        public async Task BuildAndUploadMultiPlatform(VRCWorld world, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            UiEnabled = false;
            // Give the UI a moment to update
            await Task.Delay(200, cancellationToken);
            
            _platformProgressId = VRCMultiPlatformBuild.StartMPB(_selectedBuildTargets);
            
            // If one of the targets is android - we must set the default texture format to ASTC
            // Doing it prior to building will also avoid the double-import issue
            if (_selectedBuildTargets.Contains(BuildTarget.Android) &&
                EditorUserBuildSettings.androidBuildSubtarget != MobileTextureSubtarget.ASTC)
            {
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
                AssetDatabase.Refresh();
            }

            // If we are not on the first platform - switch to it
            if (!_selectedBuildTargets.Contains(EditorUserBuildSettings.activeBuildTarget))
            {
                await VRCMultiPlatformBuild.SetUpNextMPBTarget(cancellationToken);
                return;
            }
            
            // Front-run the Copyright Agreement to avoid blocking the upload
            var pms = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (pms.Length == 0)
            {
                throw await HandleUploadError(new UploadException("The scene does not have a PipelineManager component present, make sure to add a SceneDescriptor before building and uploading"));
            }
            
            if (string.IsNullOrWhiteSpace(pms[0].blueprintId))
            {
                Undo.RecordObject(pms[0], "Assigning a new ID");
                pms[0].AssignId(PipelineManager.ContentType.world);
            }

            try
            {
                await VRCCopyrightAgreement.CheckCopyrightAgreement(pms[0], world);
            }
            catch (Exception e)
            {
                throw await HandleUploadError(e);
            }

            // Verify upload permissions before attempting to build
            await VerifyUploadPermissions();
            
            var buildResult = await Build(false);
            world.UdonProducts = _scenes[0].udonProducts;

            VRCMultiPlatformBuild.ReportMPBUploadStart(_platformProgressId);

            // Check if the upload was skipped to correctly report the status
            if (await Upload(world, buildResult.path, buildResult.signature, thumbnailPath, cancellationToken))
            {
                VRCMultiPlatformBuild.ReportMPBUploadFinish(_platformProgressId);
            }
            else
            {
                VRCMultiPlatformBuild.ReportMPBUploadSkipped(_platformProgressId);
            }
            
            _platformProgressId = -1;
            
            // Set up next target
            await VRCMultiPlatformBuild.SetUpNextMPBTarget(cancellationToken);
        }

        public async Task BuildAndTest()
        {
            await Build(true);
        }

        public Task TestLastBuild()
        {
            if (string.IsNullOrWhiteSpace(PathToLastBuild))
            {
                Core.Logger.LogError("No last build found, you must build first");
                return Task.CompletedTask;
            }

            if (!File.Exists(PathToLastBuild))
            {
                PathToLastBuild = null;
                WorldSignatureOfLastBuild = null;
                Core.Logger.LogError("No last build found, you must build first");
                return Task.CompletedTask;
            }
            VRC_SdkBuilder.RunLastExportedSceneResource();
            return Task.CompletedTask;
        }

        public void CancelUpload()
        {
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
            if (_worldUploadCancellationToken != default)
            {
                _worldUploadCancellationTokenSource.Cancel();
                Core.Logger.Log("World upload canceled");
                return;
            }
            
            Core.Logger.LogError("Custom cancellation token passed, you should cancel via its token source instead");
        }

        #endregion
    }
}
