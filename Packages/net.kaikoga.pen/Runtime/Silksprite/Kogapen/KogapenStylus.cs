using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using static Silksprite.Kogapen.KogapenConstants;

namespace Silksprite.Kogapen
{
    // NOTE: see KogapenRuntimeUtils.ValidateAutoSyncType() and KogapenEditorUtils.DrawAutoSyncTypeUdonSharpBehaviourHeader()
    // [UdonBehaviourSyncMode(BehaviourSyncMode.Auto)]
    public class KogapenStylus : UdonSharpBehaviour
    {
        const float pickerShakeVR = 0.05f;
        const float pickerShakeDesktop = 0.005f;
        const float pickerSizeVR = 0.02f;
        const float pickerSizeDesktop = 0.04f;
        const float pickerSwatchSecondsVR = 0.2f;
        const float pickerSwatchSecondsDesktop = 0.5f;
        const float pickerStrokeSecondsVR = 1.2f;
        const float pickerStrokeSecondsDesktop = 1.5f;

        const float damperRetract = 0.15f;
        const float damperSize = 0.2f;
        const float damperOffset = -0.01f;

        [SerializeField] internal KogapenSync sync;
        [SerializeField] public TrailRenderer trailRenderer;
        [SerializeField] internal VRCPickup pickup;

        [UdonSynced] // TODO: allow local picker but global pen?
        [SerializeField] internal Color color;

        [SerializeField] internal KogapenStylusKind stylusKind = KogapenStylusKind.PenAndEraser;
        [SerializeField] internal KogapenStylusPickerKind pickerKind = KogapenStylusPickerKind.Local;
        [SerializeField] internal KogapenStrokePreviewKind strokePreviewKind = KogapenStrokePreviewKind.None;
        [SerializeField] internal bool eraserTapMatchPlayer = false;
        [SerializeField] internal bool eraserTapMatchColor = false;
        [SerializeField] internal bool eraserSwipeMatchPlayer = false;
        [SerializeField] internal bool eraserSwipeMatchColor = false;

        [SerializeField] internal MeshRenderer penMesh;
        [SerializeField] internal MeshRenderer eraserMesh;
        [SerializeField] public SphereCollider eraserCollider;

        [SerializeField] internal string respawnMethodName = "Respawn";

        [SerializeField] Vector3 _penOrientation;
        [SerializeField] Vector3 _spawnPosition;
        [SerializeField] Quaternion _spawnRotation;
        [SerializeField] Color _spawnColor;

        bool _initializedLocal;
        bool _initializedTrail;
        float _pickerShake = pickerShakeDesktop;
        float _pickerSize = pickerSizeDesktop;
        float _pickerSwatchSeconds = pickerSwatchSecondsDesktop;
        float _pickerStrokeSeconds = pickerStrokeSecondsDesktop;
        int _damperLayerMask = 1;

        internal bool localIsUsing;
        internal bool localIsDamper;
        internal KogapenOrientation localOrientation = KogapenOrientation.Pen;
        internal bool localIsStylusDown;
        internal KogapenEraserMode localEraserMode = KogapenEraserMode.Disabled;

        int _localStrokeId;
        int _localPlayerId;
        float _localFulcrumResetAt;
        Vector3 _localFulcrumPosition;

        MaterialPropertyBlock _workMaterialPropertyBlock;
        int _colorProperty;

        [SerializeField] [HideInInspector] Color localStrokeColor;
        
        void Start()
        {
            _colorProperty = VRCShader.PropertyToID("_Color");
            _Kogapen_SetDisplayDesyncMode(false);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        void OnValidate()
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;
            _penOrientation = Vector3.down;
            var t = transform;
            _spawnPosition = t.position;
            _spawnRotation = t.rotation;
            _spawnColor = color;
            _Kogapen_RefreshColors();
            _Kogapen_SetDisplayDesyncMode(false);
        }
#endif

        public override void OnDeserialization()
        {
            if (pickerKind != KogapenStylusPickerKind.Global) return;
            _Kogapen_RefreshColors();
            _Kogapen_SetDisplayDesyncMode(false);
        }

        void _Kogapen_RefreshColors()
        {
            if (trailRenderer)
            {
                trailRenderer.endColor = trailRenderer.startColor = color;
            }

            var k = (color.r + color.g + color.b) / 3f;
            k += k < 0.4f ? 0.3f : -0.3f; 
            localStrokeColor = new Color(k + color.r, k + color.g, k + color.b, color.a * 2f) / 2f;
        }

        public void _Kogapen_SetDisplayDesyncMode(bool isDesyncMode)
        {
            if (!penMesh) return;
            _workMaterialPropertyBlock = _workMaterialPropertyBlock ?? new MaterialPropertyBlock();
            penMesh.GetPropertyBlock(_workMaterialPropertyBlock, 0);
            _workMaterialPropertyBlock.SetColor(_colorProperty, (isDesyncMode ? localStrokeColor : color).gamma); // conform to LineRenderer's color
            penMesh.SetPropertyBlock(_workMaterialPropertyBlock, 0);
        }

        public void _Kogapen_PickColor(Color newColor)
        {
            if (color == newColor) return;
            color = newColor;
            _Kogapen_RefreshColors();
            _Kogapen_SetDisplayDesyncMode(false);
            if (pickerKind == KogapenStylusPickerKind.Global) RequestSerialization();
            _localFulcrumResetAt = Time.time;
        }

        public override void OnPickup()
        {
            if (!_initializedLocal)
            {
                // check here to mitigate initial startup time
                if (Networking.LocalPlayer.IsUserInVR())
                {
                    _pickerShake = pickerShakeVR;
                    _pickerSize = pickerSizeVR;
                    _pickerSwatchSeconds = pickerSwatchSecondsVR;
                    _pickerStrokeSeconds = pickerStrokeSecondsVR;
                    _damperLayerMask = sync.damperLayerMask;
                }
                _initializedLocal = true;
            }

            if (pickup.currentHand == VRC_Pickup.PickupHand.Left)
            {
                sync.localLeftStylus = this;
                if (sync.localRightStylus == this) sync.localRightStylus = null;
            }
            else
            {
                sync.localRightStylus = this;
                if (sync.localLeftStylus == this) sync.localLeftStylus = null;
            }

            _localFulcrumResetAt = Time.time;
            if (!sync.myStream)
            {
                // this stylus is picked up in desync mode: colored as localStrokeColor until synced or drop
                _Kogapen_SetDisplayDesyncMode(true);
            }
        }

        public override void OnDrop() => _Kogapen_Release();

        void _Kogapen_Release()
        {
            if (sync.localLeftStylus == this)
            {
                sync.localLeftStylus = null;
            }

            if (sync.localRightStylus == this)
            {
                sync.localRightStylus = null;
            }

            localOrientation = KogapenOrientation.Pen;
            localIsUsing = false;
            _Kogapen_RefreshStates();
            _Kogapen_SetDisplayDesyncMode(false);
        }

        public override void OnPickupUseDown()
        {
            localIsUsing = true;
            _Kogapen_RefreshStates();
        }

        public override void OnPickupUseUp()
        {
            localIsUsing = false;
            _Kogapen_RefreshStates();
        }

        void _Kogapen_RefreshStates()
        {
            if (trailRenderer) trailRenderer.transform.localPosition = Vector3.zero;

            localIsDamper = _Kogapen_Damper();
            var willStylusDown = localIsUsing /* || localIsDamper */ ;

            if (willStylusDown)
            {
                if (!localIsStylusDown)
                {
                    localIsStylusDown = true;
                    _localFulcrumPosition = transform.position;
                    _localFulcrumResetAt = Time.time;
                    switch (localOrientation)
                    {
                        case KogapenOrientation.Pen:
                            _localStrokeId = ++sync.localStrokeId;
                            Kogapen_All_StylusDown();
                            if (strokePreviewKind == KogapenStrokePreviewKind.Trail)
                            {
                                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Kogapen_All_StylusDown));
                            }
                            break;
                        case KogapenOrientation.Eraser:
                            localEraserMode = KogapenEraserMode.EraseAsTap;
                            break;
                        // default:
                        //     break;
                    }
                }
            }
            else
            {
                if (localIsStylusDown)
                {
                    localIsStylusDown = false;
                    switch (localOrientation)
                    {
                        case KogapenOrientation.Pen:
                            _Kogapen_DispatchStylusUp();
                            break;
                        case KogapenOrientation.Eraser:
                            localEraserMode = KogapenEraserMode.Disabled;
                            break;
                        // default:
                        //     break;
                    }
                } 
                switch (stylusKind)
                {
                    case KogapenStylusKind.None:
                    case KogapenStylusKind.PenOnly:
                        localOrientation = KogapenOrientation.Pen;
                        break;
                    case KogapenStylusKind.EraserOnly:
                        localOrientation = KogapenOrientation.Eraser;
                        break;
                    case KogapenStylusKind.PenAndEraser:
                        var worldEraserOffset = transform.TransformDirection(_penOrientation);
                        var headDirection = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
                        var orientation = Vector3.Dot(worldEraserOffset, headDirection);
                        localOrientation = orientation >= 0f ? KogapenOrientation.Pen : KogapenOrientation.Eraser;
                        break;
                    default:
                        localOrientation = KogapenOrientation.Pen;
                        break;
                }
            }

            if (eraserMesh) eraserMesh.gameObject.SetActive(localOrientation == KogapenOrientation.Eraser);

        }

        void _Kogapen_DispatchStylusUp()
        {
            var stylusUpEventName = nameof(Kogapen_All_StylusUpWithoutStream);
            if (sync.myStream)
            {
                stylusUpEventName = nameof(Kogapen_All_StylusUpWithStream);
            }

            if (strokePreviewKind == KogapenStrokePreviewKind.Trail)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, stylusUpEventName);
            }
            else
            {
                SendCustomEvent(stylusUpEventName);
            }
        }

        readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        bool _Kogapen_Damper()
        {
            var penOrientation = transform.TransformDirection(_penOrientation);
            var damperPosition = trailRenderer.transform.position - penOrientation * damperRetract;
            var size = Physics.RaycastNonAlloc(damperPosition, penOrientation, _raycastHits, damperSize, _damperLayerMask, QueryTriggerInteraction.Ignore);
            if (size <= 0) return false;

            trailRenderer.transform.position = damperPosition + penOrientation * (_raycastHits[0].distance + damperOffset);
            return _raycastHits[0].distance < damperRetract;
        }

        public void _Kogapen_Update()
        {
            if (!pickup.currentPlayer.isLocal)
            {
                // drop
                _Kogapen_Release();
                return;
            }

            _Kogapen_RefreshStates();
            var fulcrumPosition = transform.position;
            var fulcrumDeviation = fulcrumPosition - _localFulcrumPosition;
            var fulcrumDeviated = fulcrumDeviation.magnitude > _pickerShake;

            switch (localOrientation)
            {
                case KogapenOrientation.Pen:
                    if (localIsStylusDown)
                    {
                        // pen
                        if (trailRenderer.positionCount >= MaxPacketSize)
                        {
                            localIsStylusDown = false;
                            _Kogapen_DispatchStylusUp();
                        }
                    }
                    else
                    {
                        // picker
                        if (pickerKind != KogapenStylusPickerKind.Disabled)
                        {
                            if (fulcrumDeviated)
                            {
                                _localFulcrumPosition = fulcrumPosition - fulcrumDeviation.normalized * _pickerShake / 2;
                                _localFulcrumResetAt = Time.time;
                            }
                            else
                            {
                                var t = Time.time - _localFulcrumResetAt;
                                sync._Kogapen_Picker(this, trailRenderer.transform.position, _pickerSize, t > _pickerSwatchSeconds, t > _pickerStrokeSeconds);
                            }
                        }

                    }
                    break;
                case KogapenOrientation.Eraser:
                    if (localIsStylusDown)
                    {
                        // eraser
                        switch (localEraserMode)
                        {
                            // case KogapenEraserMode.Disabled:
                            //     break;
                            case KogapenEraserMode.EraseAsTap:
                                sync._Kogapen_Erase(this, eraserTapMatchPlayer, eraserTapMatchColor ? color : Color.clear);
                                if (fulcrumDeviated) localEraserMode = KogapenEraserMode.EraseAsSwipe;
                                break;
                            case KogapenEraserMode.EraseAsSwipe:
                                sync._Kogapen_Erase(this, eraserSwipeMatchPlayer, eraserSwipeMatchColor ? color : Color.clear);
                                break;
                            // default:
                            //     break;
                        }
                    }
                    break;
                // default:
                //     break;
            }
        }

        VRCObjectSync _vrcObjectSync;
        UdonSharpBehaviour[] _udonSharpBehaviours;

        public void Kogapen_Respawn()
        {
            if (Utilities.IsValid(pickup.currentPlayer)) return;
            if (!(_vrcObjectSync || pickup.pickupable)) return; // for third party object syncs
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            color = _spawnColor;
            _Kogapen_RefreshColors();
            _Kogapen_SetDisplayDesyncMode(false);

            if (_udonSharpBehaviours == null)
            {
                _vrcObjectSync = pickup.GetComponent<VRCObjectSync>();
                _udonSharpBehaviours = GetComponents<UdonSharpBehaviour>();
            }

            if (_vrcObjectSync)
            {
#if UNITY_EDITOR
                // VRCObjectSync.Respawn() may not work in simulators
#else
                _vrcObjectSync.Respawn();
#endif
            }
            else
            {
                // Respawn as local pen. Rigidbody is not supported, I am a pen
                transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

                // try manual respawn (and update manual sync components ) by broadcasting respawn method by name
                foreach (var udonComponent in _udonSharpBehaviours)
                {
                    udonComponent.SendCustomEvent(respawnMethodName);
                }
            }
        }

        public void Kogapen_All_StylusDown()
        {
            // check here to mitigate initial startup time 2
            if (!_initializedTrail)
            {
                sync._Kogapen_SetupTrailRenderer(trailRenderer);
                _initializedTrail = true;
            }

            var currentPlayer = pickup.currentPlayer;
            if (!Utilities.IsValid(currentPlayer))
            {
                currentPlayer = Networking.GetOwner(pickup.gameObject);
            }
            if (!Utilities.IsValid(currentPlayer))
            {
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenStylus), "Kogapen_All_StylusDown(): invalid player");
#endif
                _localPlayerId = -1;
                _localStrokeId = -1;
                return;
            }
            _localPlayerId = currentPlayer.playerId;
            if (!currentPlayer.isLocal) _localStrokeId = -1;
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenStylus), $"Kogapen_All_StylusDown(): Stroke_{_localPlayerId}_{_localStrokeId}");
#endif
            trailRenderer.enabled = true;
            trailRenderer.Clear();
        }

        public void Kogapen_All_StylusUpWithoutStream()
        {
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenStylus), $"Kogapen_All_StylusUpWithoutStream(): Stroke_{_localPlayerId}_{_localStrokeId}");
#endif
            if (_localPlayerId >= 0)
            {
                // Emit local stroke for local and remote player; remote strokes are synced later
                sync._EmitLocalStroke(this, _localPlayerId, _localStrokeId, color, localStrokeColor);
            }
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }

        public void Kogapen_All_StylusUpWithStream()
        {
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenStylus), $"Kogapen_All_StylusUpWithStream(): Stroke_{_localPlayerId}_{_localStrokeId}");
#endif
            if (_localPlayerId >= 0 && _localStrokeId >= 0)
            {
                // Emit local stroke for local player only; remote strokes are synced now
                sync._EmitLocalStroke(this, _localPlayerId, _localStrokeId, color, localStrokeColor);
            }
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
    }

    public enum KogapenStylusKind
    {
        None = 0,
        PenOnly = 1,
        EraserOnly = 2,
        PenAndEraser = 3
    }
    
    public enum KogapenStylusPickerKind
    {
        Disabled = 0,
        Local = 1,
        Global = 2,
    }

    public enum KogapenStrokePreviewKind
    {
        None = 0,
        Trail = 1
    }

    internal enum KogapenOrientation
    {
        Pen = 0,
        Eraser = 1,
    }

    internal enum KogapenEraserMode
    {
        Disabled = 0,
        EraseAsTap = 1,
        EraseAsSwipe = 2,
    }
}
