using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using static Silksprite.Kogapen.KogapenConstants;

namespace Silksprite.Kogapen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KogapenStream : UdonSharpBehaviour
    {
        [NonSerialized] public KogapenSync sync;
        [NonSerialized] public int streamId;

        [UdonSynced] Vector3[] _encodedStrokes = {};
        [UdonSynced] int _encodedStrokesLength;

        public override void OnPreSerialization()
        {
            var bufferToSync = sync.encodedStrokesBuffer;
            var lengthToSync = sync.encodedStrokesOffset;
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
                        sync._LogError(nameof(KogapenStream), $"OnPreSerialization(): abort because broken encoded length {bufferToSync[l + SizeOffsetFromBody].z}, should not happen");
                        sync._LogError(nameof(KogapenStream), $"OnPreSerialization(): stroke offset: {l - HeaderSize} / {lengthToSync}");
                        break;
                    }
                    l += strokeDataLength;
                    if (l > lengthToSync) break; // normal overflow by body
                }
                if (packetLength == 0) packetLength = l; // this single stroke is too large for a single Kogapen packet, but send anyway (may fit in Udon hard limit)
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenStream), $"OnPreSerialization(): break up length: {lengthToSync} into {packetLength}");
#endif
            }
            if (packetLength != _encodedStrokes.Length)
            {
                _encodedStrokes = new Vector3[packetLength];
            }
            Array.Copy(bufferToSync, 0, _encodedStrokes, 0, packetLength);
            _encodedStrokesLength = packetLength;
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            sync._ConsumeEncodedStrokesBuffer(_encodedStrokesLength); // always consume because stroke exceeding manual sync limit should always fail  
            if (sync.encodedStrokesOffset > 0)
            {
                // send remaining data
                sync.SendCustomEventDelayedSeconds(nameof(KogapenSync._RequestSyncStrokes), 2f);
            }
        }

        public override void OnDeserialization()
        {
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenStream),  $"OnDeserialization(): <{streamId}> => received length: {_encodedStrokesLength}");
#endif
            sync._EmitRemoteStrokes(_encodedStrokes, _encodedStrokesLength);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenStream),  $"OnOwnershipTransferred(): <{streamId}> => [{(Utilities.IsValid(player) ? player.playerId.ToString() : "null")}]");
#endif
            _CheckAssignedStream(player);
        }

        void _CheckAssignedStream(VRCPlayerApi owner)
        {
            if (sync.myStream)
            {
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenStream), $"_CheckAssignedStream(): already assigned <{sync.myStream.streamId}>");
#endif
                return;
            }

            if (!owner.isLocal)
            {
#if KOGAPEN_DEBUG
                sync._Log(nameof(KogapenStream), $"_CheckAssignedStream(): ignore because owner.isLocal = {owner.isLocal}");
#endif
                return;
            }

            sync.myStream = this;
#if KOGAPEN_DEBUG
            sync._Log(nameof(KogapenStream), $"_CheckAssignedStream(): remote assigned <{streamId}>");
#endif
        }
    }
}
