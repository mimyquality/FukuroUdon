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

    public enum ActiveRelayLimitType
    {
        MinAndMaxX,
        MinX,
        MaxX,
        MinAndMaxY,
        MinY,
        MaxY,
        MinAndMaxZ,
        MinZ,
        MaxZ,
        MinAndMaxAngle,
        MinAngle,
        MaxAngle,
        MinAndMaxPitch,
        MinPitch,
        MaxPitch,
        MinAndMaxYaw,
        MinYaw,
        MaxYaw,
    }

    public enum ActiveRelayLimitEvent
    {
        ReachAndDepart,
        Reach,
        Depart
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-limited-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Limited Constraint")]
    [RequireComponent(typeof(LimitedConstraint))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByLimitedConstraint : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayLimitType _limitType = default;

        [SerializeField]
        private ActiveRelayLimitEvent _eventType = ActiveRelayLimitEvent.ReachAndDepart;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;


            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        public void OnReachedMinX()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxX:
                case ActiveRelayLimitType.MinX:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMinX()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxX:
                case ActiveRelayLimitType.MinX:
                    Depart();
                    break;
            }
        }

        public void OnReachedMaxX()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxX:
                case ActiveRelayLimitType.MaxX:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMaxX()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxX:
                case ActiveRelayLimitType.MaxX:
                    Depart();
                    break;
            }
        }

        public void OnReachedMinY()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxY:
                case ActiveRelayLimitType.MinY:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMinY()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxY:
                case ActiveRelayLimitType.MinY:
                    Depart();
                    break;
            }
        }

        public void OnReachedMaxY()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxY:
                case ActiveRelayLimitType.MaxY:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMaxY()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxY:
                case ActiveRelayLimitType.MaxY:
                    Depart();
                    break;
            }
        }

        public void OnReachedMinZ()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxZ:
                case ActiveRelayLimitType.MinZ:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMinZ()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxZ:
                case ActiveRelayLimitType.MinZ:
                    Depart();
                    break;
            }
        }

        public void OnReachedMaxZ()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxZ:
                case ActiveRelayLimitType.MaxZ:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMaxZ()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxZ:
                case ActiveRelayLimitType.MaxZ:
                    Depart();
                    break;
            }
        }

        public void OnReachedMinAngle()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxAngle:
                case ActiveRelayLimitType.MinAngle:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMinAngle()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxAngle:
                case ActiveRelayLimitType.MinAngle:
                    Depart();
                    break;
            }
        }

        public void OnReachedMaxAngle()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxAngle:
                case ActiveRelayLimitType.MaxAngle:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMaxAngle()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxAngle:
                case ActiveRelayLimitType.MaxAngle:
                    Depart();
                    break;
            }
        }

        public void OnReachedMinYaw()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxYaw:
                case ActiveRelayLimitType.MinYaw:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMinYaw()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxYaw:
                case ActiveRelayLimitType.MinYaw:
                    Depart();
                    break;
            }
        }

        public void OnReachedMaxYaw()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxYaw:
                case ActiveRelayLimitType.MaxYaw:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMaxYaw()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxYaw:
                case ActiveRelayLimitType.MaxYaw:
                    Depart();
                    break;
            }
        }

        public void OnReachedMinPitch()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxPitch:
                case ActiveRelayLimitType.MinPitch:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMinPitch()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxPitch:
                case ActiveRelayLimitType.MinPitch:
                    Depart();
                    break;
            }
        }

        public void OnReachedMaxPitch()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxPitch:
                case ActiveRelayLimitType.MaxPitch:
                    Reach();
                    break;
            }
        }

        public void OnDepartedMaxPitch()
        {
            switch (_limitType)
            {
                case ActiveRelayLimitType.MinAndMaxPitch:
                case ActiveRelayLimitType.MaxPitch:
                    Depart();
                    break;
            }
        }

        private void Reach()
        {
            switch (_eventType)
            {
                case ActiveRelayLimitEvent.ReachAndDepart:
                case ActiveRelayLimitEvent.Reach:
                    DoAction(Networking.LocalPlayer);
                    break;
            }
        }

        private void Depart()
        {
            switch (_eventType)
            {
                case ActiveRelayLimitEvent.ReachAndDepart:
                case ActiveRelayLimitEvent.Depart:
                    DoAction(Networking.LocalPlayer);
                    break;
            }
        }
    }
}