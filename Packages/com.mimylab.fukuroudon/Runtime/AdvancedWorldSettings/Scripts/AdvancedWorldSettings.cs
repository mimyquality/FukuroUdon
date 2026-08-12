/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDKBase.Editor.Attributes;
    using VRC.SDK3.Rendering;

    [System.Flags]
    public enum AdvancedWorldSettingsInitializeEyeHeightTypes
    {
        Join = 1 << 0,
        AvatarChange = 1 << 1
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Advanced-World-Settings#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/Advanced World Settings")]
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AdvancedWorldSettings : UdonSharpBehaviour
    {
        // 6: reserved6, 7: reserved7, 19: InternalUI, 31: prohibited
        private const int ForceScreenVisible = 0b0010000000000011000000 | (1 << 31);
        // 18: MirrorReflection
        private const int ForceScreenInvisible = 0b0001000000000000000000;

        [Header("Movement")]
        [SerializeField] private bool _initializeMovement = true;
        [SerializeField][Range(0f, 5f)] private float _walkSpeed = 2f;
        [SerializeField][Range(0f, 5f)] private float _strafeSpeed = 2f;
        [SerializeField][Range(0f, 10f)] private float _runSpeed = 4f;
        [SerializeField][Range(0f, 10f)] private float _jumpImpulse = 3f;
        [SerializeField][Range(0f, 10f)] private float _gravityStrength = 1f;
        [SerializeField] private bool _immobile = false;

        [Header("Pickups")]
        [SerializeField] private bool _initializePickups = true;
        [SerializeField] private bool _enablePickups = true;

        [Header("Player Voice")]
        [SerializeField] private bool _initializePlayerVoice = true;
        [SerializeField][Range(0f, 24f)] private float _voiceGain = 15f;
        [SerializeField][Range(0f, 1000000f)] private float _voiceDistanceNear = 0f;
        [SerializeField][Range(0f, 1000000f)] private float _voiceDistanceFar = 25f;
        [Space]
        [SerializeField][Range(0f, 1000f)] private float _voiceVolumetricRadius = 0f;
        [SerializeField] private bool _voiceLowpass = true;

        [Header("Avatar Audio")]
        [HelpBox("アバター側の音源に設定された値と、より小さい方が反映されます。", HelpBoxAttribute.MessageType.Info)]
        [SerializeField] private bool _initializeAvatarAudio = false;
        [SerializeField][Range(0f, 10f)] private float _avatarAudioGain = 10f;
        [SerializeField][Min(0f)] private float _avatarAudioDistanceNear = 0f;
        [SerializeField][Min(0f)] private float _avatarAudioDistanceFar = 40f;
        [Space]
        [SerializeField][Min(0f)] private float _avatarAudioVolumetricRadius = 0f;
        [SerializeField] private bool _avatarAudioForceSpatial = false;
        [HideInInspector][SerializeField] private bool _avatarAudioCustomCurve = false;

        [Header("Avatar Scaling")]
        [SerializeField] private bool _initializeAvatarScaling = true;
        [SerializeField] private bool _allowManualAvatarScaling = true;
        [SerializeField][Range(0.2f, 5f)] private float _avatarEyeHeightMinimum = 0.2f;
        [SerializeField][Range(0.2f, 5f)] private float _avatarEyeHeightMaximum = 5f;
        [Space]
        [Tooltip("チェックを入れたタイミングで、アバターの目線高さの上限と下限を設定範囲に制限します。")]
        [SerializeField][EnumFlag] private AdvancedWorldSettingsInitializeEyeHeightTypes _initializeAvatarEyeHight = 0;
        [SerializeField][Range(0.01f, 10000f)] private float _avatarEyeHeightLowerLimit = 0.1f;
        [SerializeField][Range(0.01f, 10000f)] private float _avatarEyeHeightUpperLimit = 100f;

        [Header("Screen Camera Settings")]
        [SerializeField] private bool _initializeScreenCameraSettings = false;
        [SerializeField] private bool _screenAllowHDR = false;
        [SerializeField] private DepthTextureMode _screenDepthTextureMode = DepthTextureMode.None;
        [SerializeField] private bool _screenUseOcclusionCulling = true;
        [SerializeField] private bool _screenAllowMSAA = true;
        [SerializeField] private LayerMask _screenCullingMask = ~ForceScreenInvisible;
        [SerializeField] private CameraClearFlags _screenClearFlags = CameraClearFlags.Skybox;
        [Tooltip("Screen Clear Flags が SolidColor に設定されている時の背景色。")]
        [SerializeField] private Color _screenBackgroundColor = Color.black;
        [SerializeField] private bool _screenLayerCullSpherical = false;
        [Tooltip("値が0のレイヤーは Far Clip Plane と同じ値として扱われます。")]
        [SerializeField] private float[] _screenLayerCullDistances = new float[32];

        [Header("Photo Camera Settings")]
        [SerializeField] private bool _initializePhotoCameraSettings = false;
        [SerializeField] private bool _photoAllowHDR = false;
        [Tooltip("Photo Camera では None にすることはできません。")]
        [SerializeField] private DepthTextureMode _photoDepthTextureMode = DepthTextureMode.Depth;
        [SerializeField] private bool _photoUseOcclusionCulling = true;
        [SerializeField] private bool _photoAllowMSAA = true;
        [SerializeField] private CameraClearFlags _photoClearFlags = CameraClearFlags.Skybox;
        [Tooltip("Screen Clear Flags が SolidColor に設定されている時の背景色。")]
        [SerializeField] private Color _photoBackgroundColor = Color.black;
        [SerializeField] private bool _photoLayerCullSpherical = false;
        [Tooltip("値が0のレイヤーは Far Clip Plane と同じ値として扱われます。")]
        [SerializeField] private float[] _photoLayerCullDistances = new float[32];

        [Header("Quality Settings")]
        [SerializeField] private bool _initializeQualitySettings = false;
        [SerializeField][Range(0.1f, 10000f)] private float _shadowDistance = 50.0f;
        [SerializeField][Range(0.0f, 1.0f)] private float _shadowCascade2Split = 1.0f / 3.0f;
        [SerializeField][Range(0.0f, 1.0f)] private float _shadowCascade4Split0 = 2f / 30f;
        [SerializeField][Range(0.0f, 1.0f)] private float _shadowCascade4Split1 = 6f / 30f;
        [SerializeField][Range(0.0f, 1.0f)] private float _shadowCascade4Split2 = 14f / 30f;

        private VRCPlayerApi _localPlayer;
        private bool _isFirstAvatarChanged = true;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            if ((_screenCullingMask & ForceScreenVisible) != ForceScreenVisible)
            {
                _screenCullingMask |= ForceScreenVisible;
            }
            if ((_screenCullingMask & ForceScreenInvisible) > 0)
            {
                _screenCullingMask &= ~ForceScreenInvisible;
            }

            if ((_photoDepthTextureMode & DepthTextureMode.Depth) != DepthTextureMode.Depth)
            {
                _photoDepthTextureMode |= DepthTextureMode.Depth;
            }
        }
#endif

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;

            if (_initializeScreenCameraSettings)
            {
                var screenCamera = VRCCameraSettings.ScreenCamera;
                screenCamera.AllowHDR = _screenAllowHDR;
                screenCamera.DepthTextureMode = _screenDepthTextureMode;
                screenCamera.UseOcclusionCulling = _screenUseOcclusionCulling;
                screenCamera.AllowMSAA = _screenAllowMSAA;
                screenCamera.CullingMask = _screenCullingMask;
                screenCamera.ClearFlags = _screenClearFlags;
                screenCamera.BackgroundColor = _screenBackgroundColor;
                screenCamera.LayerCullSpherical = _screenLayerCullSpherical;
                screenCamera.LayerCullDistances = _screenLayerCullDistances;
            }

            if (_initializePhotoCameraSettings &&
                Utilities.IsValid(VRCCameraSettings.PhotoCamera))
            {
                var photoCamera = VRCCameraSettings.PhotoCamera;
                photoCamera.AllowHDR = _photoAllowHDR;
                photoCamera.DepthTextureMode = _photoDepthTextureMode;
                photoCamera.UseOcclusionCulling = _photoUseOcclusionCulling;
                photoCamera.AllowMSAA = _photoAllowMSAA;
                photoCamera.ClearFlags = _photoClearFlags;
                photoCamera.BackgroundColor = _photoBackgroundColor;
                photoCamera.LayerCullSpherical = _photoLayerCullSpherical;
                photoCamera.LayerCullDistances = _photoLayerCullDistances;
            }

            if (_initializeQualitySettings)
            {
                VRCQualitySettings.SetShadowDistance(_shadowDistance);
                VRCQualitySettings.ShadowCascade2Split = _shadowCascade2Split;
                VRCQualitySettings.ShadowCascade4Split = new Vector3(_shadowCascade4Split0, _shadowCascade4Split1, _shadowCascade4Split2);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (_initializeMovement)
                {
                    player.SetWalkSpeed(_walkSpeed);
                    player.SetStrafeSpeed(_strafeSpeed);
                    player.SetRunSpeed(_runSpeed);
                    player.SetJumpImpulse(_jumpImpulse);
                    player.SetGravityStrength(_gravityStrength);
                    player.Immobilize(_immobile);
                }

                if (_initializePickups)
                {
                    player.EnablePickups(_enablePickups);
                }

                if (_initializeAvatarScaling)
                {
                    player.SetManualAvatarScalingAllowed(_allowManualAvatarScaling);
                    player.SetAvatarEyeHeightMinimumByMeters(_avatarEyeHeightMinimum);
                    player.SetAvatarEyeHeightMaximumByMeters(_avatarEyeHeightMaximum);
                }
            }

            if (_initializePlayerVoice)
            {
                player.SetVoiceGain(_voiceGain);
                player.SetVoiceDistanceNear(_voiceDistanceNear);
                player.SetVoiceDistanceFar(_voiceDistanceFar);
                player.SetVoiceVolumetricRadius(_voiceVolumetricRadius);
                player.SetVoiceLowpass(_voiceLowpass);
            }

            if (_initializeAvatarAudio)
            {
                player.SetAvatarAudioGain(_avatarAudioGain);
                player.SetAvatarAudioNearRadius(_avatarAudioDistanceNear);
                player.SetAvatarAudioFarRadius(_avatarAudioDistanceFar);
                player.SetAvatarAudioVolumetricRadius(_avatarAudioVolumetricRadius);
                player.SetAvatarAudioForceSpatial(_avatarAudioForceSpatial);
                //player.SetAvatarAudioCustomCurve(_avatarAudioCustomCurve);
                var tmp = _avatarAudioCustomCurve;    // 未使用変数警告対策
            }
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            if (_isFirstAvatarChanged)
            {
                if (((int)_initializeAvatarEyeHight & (int)AdvancedWorldSettingsInitializeEyeHeightTypes.Join) > 0)
                {
                    ClampAvatarEyeHeight();
                }

                _isFirstAvatarChanged = false;
            }
            else
            {
                if (((int)_initializeAvatarEyeHight & (int)AdvancedWorldSettingsInitializeEyeHeightTypes.AvatarChange) > 0)
                {
                    ClampAvatarEyeHeight();
                }
            }
        }

        private void ClampAvatarEyeHeight()
        {
            float avatarEyeHeight = _localPlayer.GetAvatarEyeHeightAsMeters();
            avatarEyeHeight = Mathf.Clamp(avatarEyeHeight, _avatarEyeHeightLowerLimit, _avatarEyeHeightUpperLimit);
            _localPlayer.SetAvatarEyeHeightByMeters(avatarEyeHeight);
        }
    }
}
