using System;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using static Silksprite.Kogapen.KogapenConstants;

namespace Silksprite.Kogapen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KogapenSync : UdonSharpBehaviour
    {
        [SerializeField] internal string syncId;
        
        [SerializeField] internal KogapenStream[] streams;
        [SerializeField] internal KogapenSpool[] spools;

        [SerializeField] internal Transform localStrokesContainer;
        public Transform strokesContainer;
        [SerializeField] internal GameObject strokePrefab;
        [SerializeField] internal Material strokeMaterialPC;
        [SerializeField] internal Material strokeMaterialQuest;
        [SerializeField] internal bool strokePCIsRoundedTrail;
        [SerializeField] internal float strokeDefaultWidth = 0.005f;

        // should equal Stroke prefab collider layer (= Player);
        [SerializeField] internal LayerMask strokeLayerMask = 1 << 9;

        // - Should exclude strokeLayerMask (= Player), apparently
        // - May want to also exclude UI and UIMenu
        // - Does not need to exclude Stylus (= Pickup with trigger, while damper ignores triggers)
        // Default value below is carefully crafted... 
        [SerializeField] internal LayerMask damperLayerMask =
            (1 << 0) | // Default
            (1 << 8) | // Interactive
            (1 << 11) | // Environment
            (1 << 13) | // Pickup
            (1 << 14) | // PickupNoEnvironment
            (1 << 17); // Walkthrough

        int _cachedPlayerCount;
        int _localPlayerId;
        [NonSerialized] public int localStrokeId;

        [NonSerialized] public KogapenStylus localLeftStylus;
        [NonSerialized] public KogapenStylus localRightStylus;
        bool _localStylusIsDesync;

        [NonSerialized] public KogapenStream myStream;
        [UdonSynced] int[] streamRequestingPlayerIds = new int[MaxPlayerCount];
        [UdonSynced] int streamRequestingPlayerCount;
        bool _streamRequested = false;

        // current stroke to encode / decode
        int _encodedPlayerId;
        int _encodedStrokeId;
        Color _encodedColor;
        int _positionCount;
        Vector3[] _positionsBuffer = {};
        
        // encoded strokes buffer
        [NonSerialized] public Vector3[] encodedStrokesBuffer = { };
        [NonSerialized] public int encodedStrokesOffset;

        void _AllocateStrokePositionsBuffer(int value)
        {
            _positionCount = value;
            if (value > _positionsBuffer.Length)
            {
                // reallocate positions buffer (may be empty)
                _positionsBuffer = new Vector3[value * 2];
            }
        }

        void _ExpandEncodedStrokesBuffer(int value)
        {
            if (value > encodedStrokesBuffer.Length)
            {
                // resize array
                var buf = new Vector3[value * 2];
                encodedStrokesBuffer.CopyTo(buf, 0);
                encodedStrokesBuffer = buf;
            }
        }

        public void _ConsumeEncodedStrokesBuffer(int value)
        {
            if (value >= encodedStrokesOffset)
            {
                encodedStrokesOffset = 0;
                return;
            }
            Array.Copy(encodedStrokesBuffer, value, encodedStrokesBuffer, 0, encodedStrokesOffset - value);
            encodedStrokesOffset -= value;
        }

        void Start()
        {
            var gameObjectName = gameObject.name;
            strokesContainer.SetParent(null);
            strokesContainer.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            strokesContainer.localScale = Vector3.one;
            strokesContainer.gameObject.name = $"{gameObjectName}:{syncId}:Strokes";
            
            localStrokesContainer.SetParent(null);
            localStrokesContainer.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            localStrokesContainer.localScale = Vector3.one;
            localStrokesContainer.gameObject.name = $"{gameObjectName}:{syncId}:LocalStrokes";

            _Kogapen_SetupLineRenderer(strokePrefab.GetComponent<LineRenderer>());
            for (var i = 0; i < streams.Length; i++)
            {
                var stream = streams[i];
                stream.sync = this;
                stream.streamId = i;
            }
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), $"Start(): streams: {streams.Length}");
#endif
        }
        
        public override void PostLateUpdate()
        {
            // deal with desync status
            if (myStream)
            {
                // check desync -> sync
                if (_localStylusIsDesync)
                {
                    if (localLeftStylus) localLeftStylus._Kogapen_SetDisplayDesyncMode(false);
                    if (localRightStylus) localRightStylus._Kogapen_SetDisplayDesyncMode(false);
                    _localStylusIsDesync = false;
#if KOGAPEN_DEBUG
                    _Log(nameof(KogapenSync), $"PostLateUpdate(): _localStylusIsDesync = {_localStylusIsDesync}");
#endif
                }
            }
            else
            {
                // check sync -> desync
                if (!_localStylusIsDesync)
                {
                    var someStylus = localLeftStylus ? localLeftStylus : localRightStylus;
                    if (someStylus)
                    {
                        if (localLeftStylus) localLeftStylus._Kogapen_SetDisplayDesyncMode(true);
                        if (localRightStylus) localRightStylus._Kogapen_SetDisplayDesyncMode(true);
                        _localStylusIsDesync = true;

                        // HACK: dark magic follows...
                        // We have to somehow tell the owner of sync that local player is requesting a stream
                        if (Networking.IsOwner(gameObject))
                        {
#if KOGAPEN_DEBUG
                            _Log(nameof(KogapenSync), $"PostLateUpdate(): _localStylusIsDesync = {_localStylusIsDesync} (request as sync owner)");
#endif
                            // I am owner
                            _Master_AddStreamRequest(_localPlayerId);
                        }
                        else
                        {
#if KOGAPEN_DEBUG
                            _Log(nameof(KogapenSync), $"PostLateUpdate(): _localStylusIsDesync = {_localStylusIsDesync} (request as client)");
#endif
                            // I am not owner, so try to trigger OnRequestOwnership
                            Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        }
                    }
                }
            }

            // deal with eraser
            if (localLeftStylus != null) localLeftStylus._Kogapen_Update();
            if (localRightStylus != null) localRightStylus._Kogapen_Update();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            _cachedPlayerCount = VRCPlayerApi.GetPlayerCount();
            if (player.isLocal)
            {
                _localPlayerId = player.playerId;
#if KOGAPEN_DEBUG
                _logHeader = null;
#endif
            }
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), $"OnPlayerJoined(): {player.displayName} [{player.playerId}] is {(player.isLocal ? "local" : "remote")} {(Networking.GetOwner(gameObject).playerId == player.playerId ? "sync owner" : "client")}");
#endif
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            _cachedPlayerCount = VRCPlayerApi.GetPlayerCount();
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), $"OnOwnershipRequest(): to [{requestedOwner.playerId}] by [{requestingPlayer.playerId}] on [{Networking.GetOwner(gameObject).playerId}]");
#endif
            if (!Networking.IsOwner(gameObject))
            {
                // return true here because stream request should run on sync owner.
                // This turns local player into fake sync owner (and perhaps do some fake stream assignments),
                // but it is okay because we can endure fake stream assignments
                return true;
            }
            // HACK: Ownership request to Sync triggers stream request 
            _Master_AddStreamRequest(requestingPlayer.playerId);
            // HACK: Ownership transfer should fail
            return false;
        }

        void _Kogapen_SetupLineRenderer(LineRenderer lineRenderer)
        {
#if UNITY_ANDROID
            lineRenderer.material = strokeMaterialQuest;
            lineRenderer.widthMultiplier = strokeDefaultWidth;
#else
            lineRenderer.material = strokeMaterialPC;
            if (strokePCIsRoundedTrail)
            {
                lineRenderer.widthMultiplier = 0f;
                lineRenderer.numCornerVertices = lineRenderer.numCapVertices = 0;
                lineRenderer.material.SetFloat("_Width", strokeDefaultWidth);
            }
            else
            {
                lineRenderer.widthMultiplier = strokeDefaultWidth;
            }
#endif
        }

        public void _Kogapen_SetupTrailRenderer(TrailRenderer trailRenderer)
        {
#if UNITY_ANDROID
            trailRenderer.material = strokeMaterialQuest;
            trailRenderer.widthMultiplier = strokeDefaultWidth; 
#else
            trailRenderer.material = strokeMaterialPC;
            if (strokePCIsRoundedTrail)
            {
                trailRenderer.widthMultiplier = 0f;
                trailRenderer.numCornerVertices = trailRenderer.numCapVertices = 0;
                trailRenderer.material.SetFloat("_Width", strokeDefaultWidth); 
            }
            else
            {
                trailRenderer.widthMultiplier = strokeDefaultWidth; 
            }
#endif
        }

        readonly Collider[] _collisions = new Collider[4];
        readonly char[] _strokeNameSeparator = {'_'};

        public void _Kogapen_Erase(KogapenStylus stylus, bool matchPlayer, Color matchColor)
        {
            var eraserCollider = stylus.eraserCollider;
            var c = Physics.OverlapSphereNonAlloc(eraserCollider.transform.position, eraserCollider.radius, _collisions, strokeLayerMask, QueryTriggerInteraction.Ignore);
            for (var i = 0 ; i < c; i++)
            {
                var collision = _collisions[i];
                var collisionTransform = collision.transform;
                if (!collisionTransform.IsChildOf(strokesContainer)) continue;
                var collisionParent = collisionTransform.parent;
                var collisionName = collisionParent.name;
                var metadata = collisionName.Split(_strokeNameSeparator, 3);
                _encodedPlayerId = Convert.ToInt32(metadata[1]);
                if (matchPlayer && _encodedPlayerId != _localPlayerId) continue;
                var lineRenderer = collisionParent.GetComponent<LineRenderer>();
                if (matchColor.a > 0f && lineRenderer.startColor != matchColor) continue;
#if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), $"_Erase(): {collisionName}");
#endif
                _encodedStrokeId = Convert.ToInt32(metadata[2]);
                _positionCount = 0;
                _DrawStroke(lineRenderer);
                _EncodeStroke();
                _RequestSyncStrokes();
            }
        }

        public void _Kogapen_Picker(KogapenStylus stylus, Vector3 position, float pickerSize, bool swatches, bool strokes)
        {
            if (!(swatches || strokes)) return;

            var c = Physics.OverlapSphereNonAlloc(position, pickerSize, _collisions, strokeLayerMask, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < c; i++)
            {
                var collision = _collisions[i];
                var collisionTransform = collision.transform;
                var collisionParent = collisionTransform.parent;
                if (!collisionParent) continue;
                var lineRenderer = collisionParent.GetComponent<LineRenderer>();
                if (!lineRenderer) continue;
                if (!(lineRenderer.loop ? swatches : strokes)) continue;
                var pickedColor = lineRenderer.startColor;
                stylus._Kogapen_PickColor(pickedColor);
                return;
            }
        }

        public void _Kogapen_Clear(bool localPlayerOnly)
        {
            foreach (Transform stroke in strokesContainer)
            {
                var strokeName = stroke.name;
    #if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), $"_Kogapen_Clear(): {strokeName}");
    #endif
                var metadata = strokeName.Split(_strokeNameSeparator, 3);
                ;_encodedPlayerId = Convert.ToInt32(metadata[1]);
                if (localPlayerOnly && _encodedPlayerId != _localPlayerId) continue;
                _encodedStrokeId = Convert.ToInt32(metadata[2]);
                _positionCount = 0;
                _DrawStroke(stroke.GetComponent<LineRenderer>());
                _EncodeStroke();
            }
            _RequestSyncStrokes();
        }

        public void _EmitLocalStroke(KogapenStylus stylus, int playerId, int strokeId, Color color, Color localStrokeColor)
        {
            var trailRenderer = stylus.trailRenderer;
            _AllocateStrokePositionsBuffer(trailRenderer.positionCount);

            if (_positionCount < 2) return;
            trailRenderer.GetPositions(_positionsBuffer);

            if (strokeId < 0)
            {
                var localStroke = _InstantiateLocalStroke(playerId);
                localStroke.endColor = localStroke.startColor = localStrokeColor;
                _DrawStroke(localStroke);
                return;
            }

            var stroke = _InstantiateStroke(playerId, strokeId);
            stroke.endColor = stroke.startColor = color;
            _DrawStroke(stroke);

            if (_cachedPlayerCount == 1)
            {
#if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), "_EmitLocalStroke(): abort encoding strokes because no remote players");
#endif
                _ConsumeEncodedStrokesBuffer(0x7fffffff);
                return;
            }
            
            _encodedPlayerId = playerId;
            _encodedStrokeId = strokeId;
            _encodedColor = color;
            _EncodeStroke();
            _RequestSyncStrokes();
        }

        public void _EmitRemoteStrokes(Vector3[] encodedStrokes, int encodedStrokesLength)
        {
            var position = 0;
            do
            {
                position = _DecodeStroke(encodedStrokes, encodedStrokesLength, position);
            } while (position > -1);
        }

        LineRenderer _InstantiateStroke(int playerId, int strokeId)
        {
            _ClearLocalStrokes(playerId);
            var strokeName = $"Stroke_{playerId}_{strokeId}";
            var stroke = strokesContainer.Find(strokeName);
            if (stroke != null) return stroke.GetComponent<LineRenderer>();

            var strokeObject = Instantiate(strokePrefab, strokesContainer, false);
            strokeObject.name = strokeName;
            return strokeObject.GetComponent<LineRenderer>();
        }

        void _DrawStroke(LineRenderer stroke)
        {
            stroke.positionCount = _positionCount;
            stroke.SetPositions(_positionsBuffer);

#if UNITY_EDITOR
            {
                var strokeCollider = stroke.GetComponentInChildren<BoxCollider>(true);
                if (_positionCount == 0)
                {
                    strokeCollider.enabled = false;
                    return;
                }
                strokeCollider.center = stroke.GetPosition(0);
                strokeCollider.size = new Vector3(0.2f, 0.2f, 0.2f);
                strokeCollider.enabled = true;
            }
#else
            {
                var strokeCollider = stroke.GetComponentInChildren<MeshCollider>(true);
                if (_positionCount == 0)
                {
                    strokeCollider.enabled = false;
                    return;
                }
                var mesh = new Mesh();
                var width = stroke.widthMultiplier; 
                stroke.widthMultiplier = strokeDefaultWidth;
                stroke.BakeMesh(mesh);
                stroke.widthMultiplier = width;
                strokeCollider.sharedMesh = mesh;
                strokeCollider.enabled = true;
            }
#endif
        }

        LineRenderer _InstantiateLocalStroke(int playerId)
        {
            var strokeName = $"Stroke_{playerId}_-1";
            var stroke = Instantiate(strokePrefab, localStrokesContainer, false);
            stroke.name = strokeName;
            return stroke.GetComponent<LineRenderer>();
        }

        void _ClearLocalStrokes(int playerId)
        {
            var strokeName = $"Stroke_{playerId}_-1";
            for (var oldStroke = localStrokesContainer.Find(strokeName); oldStroke; oldStroke = localStrokesContainer.Find(strokeName))
            {
                oldStroke.transform.SetParent(null);
                Destroy(oldStroke.gameObject);
            }
        }

        void _EncodeStroke()
        {
            _ExpandEncodedStrokesBuffer(encodedStrokesOffset + HeaderSize + _positionCount);
            var buffer = encodedStrokesBuffer;
            // see KogapenSync._DecodeStroke() for header format
            buffer[encodedStrokesOffset++] = new Vector3(1, _encodedPlayerId, _encodedStrokeId);
            buffer[encodedStrokesOffset++] = new Vector3(_positionCount, 0, _positionCount);
            buffer[encodedStrokesOffset++] = new Vector3(_encodedColor.r, _encodedColor.g, _encodedColor.b);
            Array.Copy(_positionsBuffer, 0, encodedStrokesBuffer, encodedStrokesOffset, _positionCount);
            encodedStrokesOffset += _positionCount; 
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), $"_EncodeStroke(): ({_encodedPlayerId} {_encodedStrokeId}) {_positionCount} offset: {encodedStrokesOffset}");
#endif
        }

        int _DecodeStroke(Vector3[] encodedStrokes, int encodedStrokesLength, int position)
        {
            if (encodedStrokesLength < position + HeaderSize)
            {
#if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), $"_DecodeStroke(): -1 because header overflow, position: {position} length: {encodedStrokesLength})");
#endif
                return -1;
            }

            var vId = encodedStrokes[position++];
            var vSize = encodedStrokes[position++];
            var vColor = encodedStrokes[position++];

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (vId.x != 1)
            {
                _LogError(nameof(KogapenSync), "_DecodeStroke(): -1 because broken command, should not happen");
                _LogError(nameof(KogapenSync), $"_DecodeStroke(): header dump: {vId.x:G7} {vId.y:G7} {vId.z:G7} {vSize.x:G7} {vSize.y:G7} {vSize.z:G7} {vColor.x:G7} {vColor.y:G7} {vColor.z:G7}");
                _LogError(nameof(KogapenSync), $"_DecodeStroke(): stroke offset: {position - HeaderSize} / {encodedStrokesLength}");
                return -1;
            }

            var encodedLength = (int)vSize.z; // check here to early detect body overflow
            if (encodedLength < 0)
            {
                _LogError(nameof(KogapenSync), "_DecodeStroke(): -1 because broken encoded length, should not happen");
                _LogError(nameof(KogapenSync), $"_DecodeStroke(): header dump: {vId.x:G7} {vId.y:G7} {vId.z:G7} {vSize.x:G7} {vSize.y:G7} {vSize.z:G7} {vColor.x:G7} {vColor.y:G7} {vColor.z:G7}");
                _LogError(nameof(KogapenSync), $"_DecodeStroke(): stroke offset: {position - HeaderSize} / {encodedStrokesLength}");
                return -1;
            }
            if (encodedStrokesLength < position + encodedLength)
            {
                _LogError(nameof(KogapenSync), "_DecodeStroke(): -1 because body overflow, should not happen");
                _LogError(nameof(KogapenSync), $"_DecodeStroke(): header dump: {vId.x:G7} {vId.y:G7} {vId.z:G7} {vSize.x:G7} {vSize.y:G7} {vSize.z:G7} {vColor.x:G7} {vColor.y:G7} {vColor.z:G7}");
                _LogError(nameof(KogapenSync), $"_DecodeStroke(): stroke offset: {position - HeaderSize} / {encodedStrokesLength}");
                return -1;
            }
            _encodedPlayerId = (int)vId.y;
            _encodedStrokeId = (int)vId.z;
            _AllocateStrokePositionsBuffer((int)vSize.x); // sets _positionCount = vSize.x
            // and also ensures _positionsBuffer.Length > lineRenderer.PositionCount because _positionsBuffer does not shrink
            var encodedOffset = (int)vSize.y;
            _encodedColor = new Color(vColor.x, vColor.y, vColor.z, 1f);

            var stroke = _InstantiateStroke(_encodedPlayerId, _encodedStrokeId);
            stroke.endColor = stroke.startColor = _encodedColor;

            if (_positionCount < 0)
            {
                stroke.positionCount = 0;
            }
            else
            {
                if (stroke.positionCount > 0) stroke.GetPositions(_positionsBuffer);
                Array.Copy(encodedStrokes, position, _positionsBuffer, encodedOffset, Mathf.Min(encodedLength, _positionCount - encodedOffset));
                _DrawStroke(stroke);
            }

            position += encodedLength;
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), $"_DecodeStroke(): ({_encodedPlayerId} {_encodedStrokeId}) {_positionCount} offset: {position}");
#endif
            return position;
        }

        public void _RequestSyncStrokes()
        {
            if (_cachedPlayerCount == 1)
            {
#if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), "_RequestSyncStrokes(): discard encoded strokes because no remote players");
#endif
                _ConsumeEncodedStrokesBuffer(0x7fffffff);
                return;
            }

            if (myStream)
            {
                _SyncStrokes();
                return;
            }

#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), "_RequestSyncStrokes(): waiting for stream");
#endif
            // Master would SetOwner a random stream for me someday. myStream will be set in OnOwnershipTransferred()
            SendCustomEventDelayedSeconds(nameof(_RequestSyncStrokes), 2f);
        }

        void _SyncStrokes()
        {
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), "_SyncStrokes():");
#endif
            // trigger network manual sync rate limit
            myStream.RequestSerialization();
        }

        void _Master_AddStreamRequest(int playerId)
        {
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), $"_Master_AddStreamRequest(): [{playerId}]");
#endif
            for (var i = 0; i < streamRequestingPlayerCount; i++)
            {
                if (streamRequestingPlayerIds[i] == playerId)
                {
                    playerId = 0;
                }
            }

            if (playerId > 0)
            {
                for (var i = 0; i < streamRequestingPlayerIds.Length; i++)
                {
                    if (streamRequestingPlayerIds[i] <= 0)
                    {
                        streamRequestingPlayerIds[i] = playerId;
                        streamRequestingPlayerCount = Mathf.Max(streamRequestingPlayerCount, i + 1);
                        RequestSerialization();
                        break;
                    }
                }
            }

            if (_streamRequested) return;
            _streamRequested = true;
            SendCustomEventDelayedSeconds(nameof(_Master_AssignStreams), 0.2f);
        }

        public void _Master_AssignStreams()
        {
#if KOGAPEN_DEBUG
            _Log(nameof(KogapenSync), "_Master_AssignStreams()");
#endif
            var streamId = 0;
            for (var i = 0; i < streamRequestingPlayerCount; i++)
            {
                var playerId = streamRequestingPlayerIds[i];
                if (playerId <= 0) continue;

#if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), $"_Master_AssignStreams(): assigning stream for [{playerId}] (request {i}):");
#endif

                for (; streamId < streams.Length; streamId++)
                {
                    var stream = streams[streamId];
                    if (!stream) continue;
                    if (stream == myStream)
                    {
#if KOGAPEN_DEBUG
                        _Log(nameof(KogapenSync), $"_Master_AssignStreams(): stream <{streamId}> is already assigned to local [{_localPlayerId}]");
#endif
                        if (playerId == _localPlayerId)
                        {
                            streamRequestingPlayerIds[i] = 0;
                            break;
                        }

                        continue;
                    }

                    var owner = Networking.GetOwner(stream.gameObject);
                    if (Utilities.IsValid(owner) && !owner.isLocal)
                    {
                        var assignedPlayerId = owner.playerId;
#if KOGAPEN_DEBUG
                        _Log(nameof(KogapenSync), $"_Master_AssignStreams(): stream <{streamId}> is already assigned to [{assignedPlayerId}]");
#endif
                        if (assignedPlayerId == playerId)
                        {
                            streamRequestingPlayerIds[i] = 0;
                            break;
                        }

                        continue;
                    }

                    Networking.SetOwner(VRCPlayerApi.GetPlayerById(playerId), stream.gameObject);
                    streamRequestingPlayerIds[i] = 0;
                    if (Networking.LocalPlayer.playerId == playerId)
                    {
                        myStream = stream;
#if KOGAPEN_DEBUG
                        _Log(nameof(KogapenSync), $"_Master_AssignStreams(): self assigning <{streamId}> to [{playerId}]");
#endif
                    }
                    else
                    {
#if KOGAPEN_DEBUG
                        _Log(nameof(KogapenSync), $"_Master_AssignStreams(): assigning <{streamId}> to [{playerId}]");
#endif
                    }

                    break;
                }

                if (streamId != streams.Length) continue;

#if KOGAPEN_DEBUG
                _Log(nameof(KogapenSync), "_Master_AssignStreams(): fully assigned streams, initiating Kogapen_All_RequestReleaseStreams()");
#endif
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Kogapen_All_RequestReleaseStreams));
                SendCustomEventDelayedSeconds(nameof(_Master_AssignStreams), 1f);
                RequestSerialization();
                return;
            }

            _streamRequested = false;
            RequestSerialization();
        }

        public void Kogapen_All_RequestReleaseStreams()
        {
            // NOTE: non-empty encodedStrokesBuffer (that means we have some data to send) does not prevent releasing streams
            // This is not an issue because when that happens it is certain that we needed more stream objects
            foreach (var stream in streams)
            {
                if (stream != myStream)
                {
                    if (!Networking.IsOwner(stream.gameObject)) continue;
#if KOGAPEN_DEBUG
                    _Log(nameof(KogapenSync), $"Kogapen_All_RequestReleaseStreams(): releasing <{stream.streamId}> because loose owner");
#endif
                    Networking.SetOwner(Networking.GetOwner(gameObject), stream.gameObject);
                }
                else if (localLeftStylus == localRightStylus) // this means localLeftStylus == null && localRightStylus == null
                {
#if KOGAPEN_DEBUG
                    _Log(nameof(KogapenSync), $"Kogapen_All_RequestReleaseStreams(): releasing <{stream.streamId}> because !localIsDrawing");
#endif
                    Networking.SetOwner(Networking.GetOwner(gameObject), stream.gameObject);
                    myStream = null;
                }
            }
        }

        string _logHeader;
        string LogHeader => _logHeader ?? (_logHeader = $"[Kogapen] :{syncId}: [{_localPlayerId}] ");

#if KOGAPEN_DEBUG
        public void _Log(string category, string message) => Debug.Log($"{LogHeader}{category}.{message}", this);
#endif
        public void _LogError(string category, string message) => Debug.LogError($"!!! {LogHeader}{category}.{message}", this);
        
        [PublicAPI]
        public void Kogapen_ClearLocal() => _Kogapen_Clear(false);

        [PublicAPI]
        public void Kogapen_ClearGlobal() => _Kogapen_Clear(true);
        
        [PublicAPI]
        public void Kogapen_RespawnAll()
        {
            foreach (var spool in spools)
            {
                if (!spool) continue;
                spool.Kogapen_RespawnAll();
            }
        }
    }
}
