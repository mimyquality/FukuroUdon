using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.Editor;
using VRC.SDK3.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Editor.V3;
using Core = VRC.Core;
using Tools = VRC.Tools;

[assembly: VRCSdkControlPanelBuilder(typeof(VRCSdkControlPanelWorldBuilderV3))]
namespace VRC.SDK3.Editor
{
    public class VRCSdkControlPanelWorldBuilderV3 : VRCSdkControlPanelWorldBuilder
    {
        public override void SetupExtraPanelUI()
        {
            V3SdkUI.SetupV3UI(() => _builder.NoGuiErrorsOrIssues(), () =>
                {
                    bool uploadBlocked = !VRCBuildPipelineCallbacks.OnVRCSDKBuildRequested(VRCSDKRequestedBuildType.Scene);
                    if (!uploadBlocked)
                    {
                        if (Core.APIUser.CurrentUser.canPublishWorlds)
                        {
                            EnvConfig.ConfigurePlayerSettings();
                            EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", false);
                            
                            VRC_SdkBuilder.shouldBuildUnityPackage = false;
                            VRC_SdkBuilder.PreBuildBehaviourPackaging();
                            VRC_SdkBuilder.ExportSceneToV3();
                        }
                        else
                        {
                            VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
                        }
                    }
                }, 
                _v3Block);
        }
        
        public override bool IsValidBuilder(out string message)
        {
            if (!VRC.SDKBase.Editor.V3.V3SdkUI.V3Enabled())
            {
                message = "SDK V3 is not enabled.";
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
    }
}