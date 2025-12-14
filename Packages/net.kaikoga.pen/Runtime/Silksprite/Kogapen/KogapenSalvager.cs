using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using static Silksprite.Kogapen.KogapenConstants;

namespace Silksprite.Kogapen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KogapenSalvager : UdonSharpBehaviour
    {
        [SerializeField] internal KogapenSync sync;

        Transform _strokesContainer;
        Transform _nextStroke;

        int _lastPlayerId;
        bool _isRestartRequested;

        Vector3[] _positionsBuffer = {};

        Vector3[] _encodedStrokesBuffer = {};
        int _encodedStrokesOffset;

        [UdonSynced] Vector3[] _encodedStrokes = {};
        [UdonSynced] int _encodedStrokesLength;

        public int maxPacketStrokes = 128;

        void _AllocateStrokePositionsBuffer(int value)
        {
            if (value > _positionsBuffer.Length)
            {
                // reallocate positions buffer (may be empty)
                _positionsBuffer = new Vector3[value * 2];
            }
        }

        void _ExpandEncodedStrokesBuffer(int value)
        {
            if (value > _encodedStrokesBuffer.Length)
            {
                // resize array
                var buf = new Vector3[value * 2];
                _encodedStrokesBuffer.CopyTo(buf, 0);
                _encodedStrokesBuffer = buf;
            }
        }

        void _ConsumeEncodedStrokesBuffer(int value)
        {
            if (value >= _encodedStrokesOffset)
            {
                _encodedStrokesOffset = 0;
                return;
            }
            Array.Copy(_encodedStrokesBuffer, value, _encodedStrokesBuffer, 0, _encodedStrokesOffset - value);
            _encodedStrokesOffset -= value;
        }

        void Start()
        {
            _strokesContainer = sync.strokesContainer;
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;

            if (player.playerId <= _lastPlayerId) return;
            _lastPlayerId = player.playerId;
            _isRestartRequested = true;
            _RequestResumeSalvage();
        }


        void _RequestResumeSalvage()
        {
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenSalvager), $"_RequestResumeSalvage(): IsClogged = {Networking.IsClogged}");
#endif
            SendCustomEventDelayedSeconds(nameof(_ResumeSalvage), Networking.IsClogged ? 2f : 0.5f);
        }

        public void _ResumeSalvage()
        {
            // trigger network manual sync rate limit
            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            _SalvageStrokes();
            var bufferToSync = _encodedStrokesBuffer;
            var lengthToSync = _encodedStrokesOffset;
            var packetLength = lengthToSync;
            if (packetLength > MaxPacketSize)
            {
                // break up length
                var l = 0;
                while (l < MaxPacketSize)
                {
                    packetLength = l;
                    l += HeaderSize;
                    if (l > lengthToSync)
                    {
                        // normal overflow by header
                        l -= HeaderSize;
                        break;
                    }
                    var strokeDataLength = (int)bufferToSync[l + SizeOffsetFromBody].z;
                    if (strokeDataLength < 0)
                    {
                        sync._LogError(nameof(KogapenSalvager), $"OnPreSerialization(): abort because broken encoded length {bufferToSync[l + SizeOffsetFromBody].z}, should not happen");
                        sync._LogError(nameof(KogapenSalvager), $"OnPreSerialization(): stroke offset: {l - HeaderSize} / {lengthToSync}");
                        break;
                    }
                    l += strokeDataLength;
                    if (l > lengthToSync) break; // normal overflow by body
                }
                if (packetLength == 0) packetLength = l; // this single stroke is too large for a single Kogapen packet, but send anyway (may fit in Udon hard limit)
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenSalvager), $"OnPreSerialization(): break up length: {lengthToSync} into {packetLength}");
#endif
            }

            if (packetLength != _encodedStrokes.Length)
            {
                _encodedStrokes = new Vector3[packetLength];
            }
            Array.Copy(_encodedStrokesBuffer, 0, _encodedStrokes, 0, packetLength);
            _encodedStrokesLength = packetLength;
            _ConsumeEncodedStrokesBuffer(packetLength);
        }

        readonly char[] _strokeNameSeparator = {'_'};
        void _SalvageStrokes()
        {
            var childCount = _strokesContainer.childCount;
            if (childCount == 0)
            {
                _nextStroke = null;
                _isRestartRequested = false;
                return;
            }

            if (_nextStroke == null)
            {
                if (_isRestartRequested)
                {
                    _isRestartRequested = false;
                }
                else 
                {
                    return;
                }
            }

            var childIndex = _nextStroke ? _nextStroke.GetSiblingIndex() : 0;
            var childIndexMax = Math.Min(childIndex + maxPacketStrokes, childCount); 
            
            for (; childIndex < childIndexMax; childIndex++)
            {
                _nextStroke = _strokesContainer.GetChild(childIndex);
                if (_encodedStrokesOffset > MaxPacketSize)
                {
#if KOGAPEN_DEBUG
                    sync._Log(nameof(KogapenSalvager), $"_SalvageStrokes(): break up at {childIndex} because: {_encodedStrokesOffset} > {MaxPacketSize}");
#endif
                    return;
                }
                var lineRenderer = _nextStroke.GetComponent<LineRenderer>();

                var positionCount = lineRenderer.positionCount;
                _AllocateStrokePositionsBuffer(positionCount);
                positionCount = lineRenderer.GetPositions(_positionsBuffer);

                var metadata = _nextStroke.gameObject.name.Split(_strokeNameSeparator, 3);
                var encodedPlayerId = Convert.ToSingle(metadata[1]);
                var encodedStrokeId = Convert.ToSingle(metadata[2]);

                var encodedColor = lineRenderer.startColor;

                _ExpandEncodedStrokesBuffer(_encodedStrokesOffset + positionCount + HeaderSize);
                var buffer = _encodedStrokesBuffer;

                // see KogapenSync._DecodeStroke() for header format
                buffer[_encodedStrokesOffset++] = new Vector3(1, encodedPlayerId, encodedStrokeId);
                buffer[_encodedStrokesOffset++] = new Vector3(positionCount, 0, positionCount);
                buffer[_encodedStrokesOffset++] = new Vector3(encodedColor.r, encodedColor.g, encodedColor.b);
                Array.Copy(_positionsBuffer, 0, _encodedStrokesBuffer, _encodedStrokesOffset, positionCount);
                _encodedStrokesOffset += positionCount; 
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenSalvager), $"_SalvageStrokes(): ({encodedPlayerId} {encodedStrokeId} {positionCount}) offset: {_encodedStrokesOffset}");
#endif
            }
            if (childIndex == childCount) _nextStroke = null;
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (_isRestartRequested)
            {
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenSalvager), "OnPostSerialization(): continue salvage because restart is requested");
#endif
            }
            else if (_nextStroke)
            {
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenSalvager), "OnPostSerialization(): continue salvage because more strokes");
#endif
            }
            else
            {
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenSalvager), "OnPostSerialization(): salvage completed");
#endif
                return;
            }

            _RequestResumeSalvage();
        }

        public override void OnDeserialization()
        {
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenSalvager),  $"OnDeserialization(): received length: {_encodedStrokesLength}");
#endif
            sync._EmitRemoteStrokes(_encodedStrokes, _encodedStrokesLength);
        }
    }
}
