/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components.Video;

    [System.Flags]
    public enum ActiveRelayVideoEvents
    {
        Ready = 1 << 0,
        Start = 1 << 1,
        Play = 1 << 2,
        Pause = 1 << 3,
        End = 1 << 4,
        Loop = 1 << 5,
        Error = 1 << 6,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-video")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Video")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayByVideo : ActiveRelayBy
    {
        [SerializeField, EnumFlag]
        private ActiveRelayVideoEvents _eventType;

        public override void OnVideoReady()
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.Ready) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }

        public override void OnVideoStart()
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.Start) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }

        public override void OnVideoPlay()
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.Play) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }

        public override void OnVideoPause()
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.Pause) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }

        public override void OnVideoEnd()
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.End) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }

        public override void OnVideoLoop()
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.Loop) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }

        public override void OnVideoError(VideoError videoError)
        {
            if (((int)_eventType & (int)ActiveRelayVideoEvents.Error) > 0)
            {
                DoAction(Networking.LocalPlayer);
            }
        }
    }
}
