using System;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.Api;


namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCAvatarPedestal))]
    public class VRCAvatarPedestalEditor : VRCInspectorBase
    {
        
        private SerializedProperty _blueprintId;
        private SerializedProperty _placement;
        private SerializedProperty _changeAvatarsOnUse;
        private SerializedProperty _scale;

        private PropertyField _fieldBlueprintId;
        private PropertyField _fieldPlacement;
        private PropertyField _fieldChangeAvatarsOnUse;
        private PropertyField _fieldScale;

        private HelpBox _errorMessageBox;
        private HelpBox _warningMessageBox;
        
        private const string ERROR_PRIVATE_AVATAR = "This pedestal contains a private avatar ID. Only the author of the avatar will be able to see the pedestal and interact with it in VRChat.";
        private const string ERROR_PRIVATE_OWN_AVATAR = "This pedestal contains a private avatar ID of your own. Only you will be able to see the pedestal and interact with it in VRChat.";
        private const string ERROR_FAILED_TO_LOAD = "Failed to load avatar information. Make sure the avatar ID is valid and the avatar is public.";
        private const string ERROR_UNAUTHORIZED = "Failed to load avatar information. Make sure you are logged in via the VRChat SDK Control Panel.";
        
        private void OnEnable()
        {
            _blueprintId = serializedObject.FindProperty(nameof(VRCAvatarPedestal.blueprintId));
            _placement = serializedObject.FindProperty(nameof(VRCAvatarPedestal.Placement));
            _changeAvatarsOnUse = serializedObject.FindProperty(nameof(VRCAvatarPedestal.ChangeAvatarsOnUse));
            _scale = serializedObject.FindProperty(nameof(VRCAvatarPedestal.scale));
        }

        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();
            _fieldBlueprintId = AddFieldLabel(_blueprintId, "Avatar ID");
            
            _errorMessageBox = new HelpBox(string.Empty, HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            Root.Add(_errorMessageBox);
            _warningMessageBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            Root.Add(_warningMessageBox);
            
            _fieldPlacement = AddField(_placement);
            _fieldChangeAvatarsOnUse = AddField(_changeAvatarsOnUse);
            _fieldScale = AddField(_scale);
            _fieldBlueprintId.RegisterValueChangeCallback(evt => AvatarChanged(evt.changedProperty.stringValue).ConfigureAwait(false));

            AvatarChanged(_blueprintId.stringValue).ConfigureAwait(false);
        }

        private void ShowError(string message)
        {
            HideWarning();
            _errorMessageBox.text = message;
            _errorMessageBox.style.display = DisplayStyle.Flex;
        }
        
        private void HideError()
        {
            _errorMessageBox.style.display = DisplayStyle.None;
        }
        
        private void ShowWarning(string message)
        {
            HideError();
            _warningMessageBox.text = message;
            _warningMessageBox.style.display = DisplayStyle.Flex;
        }
        
        private void HideWarning()
        {
            _warningMessageBox.style.display = DisplayStyle.None;
        }
        
        private async Task AvatarChanged(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                HideError();
                HideWarning();
                return;
            }
            
            try
            {
                // Attempt to log in if the user is not logged in
                if (!Core.APIUser.IsLoggedIn)
                {
                    // No void task completion source in .NET Standard, so we use a <bool> here
                    var resultTask = new TaskCompletionSource<bool>();
                    Core.APIUser.InitialFetchCurrentUser(_ => resultTask.SetResult(true), _ => resultTask.SetResult(false));
                    await resultTask.Task;
                }
                var avatarInfo = await VRCApi.GetAvatar(id, true);
                // AVM Avatars should not trigger a warning or error
                if (avatarInfo.ReleaseStatus != "public" && !avatarInfo.Lock)
                {
                    if (avatarInfo.AuthorId == Core.APIUser.CurrentUser?.id)
                    {
                        Core.Logger.LogWarning(ERROR_PRIVATE_OWN_AVATAR);
                        ShowWarning(ERROR_PRIVATE_OWN_AVATAR);
                    }
                    else
                    {
                        Core.Logger.LogError(ERROR_PRIVATE_AVATAR);
                        ShowError(ERROR_PRIVATE_AVATAR);
                    }
                    return;
                }
                
                // If everything is ok - we can hide all the messages
                HideWarning();
                HideError();
            }
            catch (ApiErrorException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    Core.Logger.LogError(ERROR_PRIVATE_AVATAR);
                    ShowError(ERROR_PRIVATE_AVATAR);
                    return;
                }

                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Core.Logger.LogWarning(ERROR_UNAUTHORIZED);
                    ShowWarning(ERROR_UNAUTHORIZED);
                    return;
                }
                Core.Logger.LogException(e);
                Core.Logger.LogError(e.ErrorMessage);
                Core.Logger.LogWarning(ERROR_FAILED_TO_LOAD);
                ShowWarning(ERROR_FAILED_TO_LOAD);
            }
            catch (Exception e)
            {
                Core.Logger.LogException(e);
                Core.Logger.LogWarning(ERROR_FAILED_TO_LOAD);
                ShowWarning(ERROR_FAILED_TO_LOAD);
            }
        }
    }
}