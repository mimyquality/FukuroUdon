/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/


namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using VRC.SDKBase;
    using VRC.Udon;
    
    public abstract class LimitedConstraint : UdonSharpBehaviour
    {
        private protected const string EventName_OnReachedMinX =  "OnReachedMinX";
        private protected const string EventName_OnDepartedMinX = "OnDepartedMinX";
        private protected const string EventName_OnReachedMaxX = "OnReachedMaxX";
        private protected const string EventName_OnDepartedMaxX = "OnDepartedMaxX";
        private protected const string EventName_OnReachedMinY = "OnReachedMinY";
        private protected const string EventName_OnDepartedMinY = "OnDepartedMinY";
        private protected const string EventName_OnReachedMaxY = "OnReachedMaxY";
        private protected const string EventName_OnDepartedMaxY = "OnDepartedMaxY";
        private protected const string EventName_OnReachedMinZ = "OnReachedMinZ";
        private protected const string EventName_OnDepartedMinZ = "OnDepartedMinZ";
        private protected const string EventName_OnReachedMaxZ = "OnReachedMaxZ";
        private protected const string EventName_OnDepartedMaxZ = "OnDepartedMaxZ";
        private protected const string EventName_OnReachedMinAngle = "OnReachedMinAngle";
        private protected const string EventName_OnDepartedMinAngle = "OnDepartedMinAngle";
        private protected const string EventName_OnReachedMaxAngle = "OnReachedMaxAngle";
        private protected const string EventName_OnDepartedMaxAngle = "OnDepartedMaxAngle";
        private protected const string EventName_OnReachedMinYaw = "OnReachedMinYaw";
        private protected const string EventName_OnDepartedMinYaw = "OnDepartedMinYaw";
        private protected const string EventName_OnReachedMaxYaw = "OnReachedMaxYaw";
        private protected const string EventName_OnDepartedMaxYaw = "OnDepartedMaxYaw";
        private protected const string EventName_OnReachedMinPitch = "OnReachedMinPitch";
        private protected const string EventName_OnDepartedMinPitch = "OnDepartedMinPitch";
        private protected const string EventName_OnReachedMaxPitch = "OnReachedMaxPitch";
        private protected const string EventName_OnDepartedMaxPitch = "OnDepartedMaxPitch";
        
        private protected bool _isReachMinX, _isReachMaxX;
        private protected bool _isReachMinY, _isReachMaxY;
        private protected bool _isReachMaxZ, _isReachMinZ;
        private protected bool _isReachMinAngle, _isReachMaxAngle;
        private protected bool _isReachMinYaw, _isReachMaxYaw;
        private protected bool _isReachMinPitch, _isReachMaxPitch;

        private protected UdonBehaviour[] _eventReceivers = System.Array.Empty<UdonBehaviour>();
        
        private protected void SetIsReachMinX(bool value)
        {
            if (_isReachMinX != value)
            {
                _isReachMinX = value;

                SendLimitEndEvent(value ? EventName_OnReachedMinX : EventName_OnDepartedMinX);
            }
        }

        private protected void SetIsReachMaxX(bool value)
        {
            if (_isReachMaxX != value)
            {
                _isReachMaxX = value;

                SendLimitEndEvent(value ? EventName_OnReachedMaxX : EventName_OnDepartedMaxX);
            }
        }

        private protected void SetIsReachMinY(bool value)
        {
            if (_isReachMinY != value)
            {
                _isReachMinY = value;

                SendLimitEndEvent(value ? EventName_OnReachedMinY : EventName_OnDepartedMinY);
            }
        }

        private protected void SetIsReachMaxY(bool value)
        {
            if (_isReachMaxY != value)
            {
                _isReachMaxY = value;

                SendLimitEndEvent(value ? EventName_OnReachedMaxY : EventName_OnDepartedMaxY);
            }
        }

        private protected void SetIsReachMinZ(bool value)
        {
            if (_isReachMinZ != value)
            {
                _isReachMinZ = value;

                SendLimitEndEvent(value ? EventName_OnReachedMinZ : EventName_OnDepartedMinZ);
            }
        }

        private protected void SetIsReachMaxZ(bool value)
        {
            if (_isReachMaxZ != value)
            {
                _isReachMaxZ = value;

                SendLimitEndEvent(value ? EventName_OnReachedMaxZ : EventName_OnDepartedMaxZ);
            }
        }
        
        private protected void SetIsReachMinAngle(bool value)
        {
            if (_isReachMinAngle != value)
            {
                _isReachMinAngle = value;

                SendLimitEndEvent(value ? EventName_OnReachedMinAngle : EventName_OnDepartedMinAngle);
            }
        }

        private protected void SetIsReachMaxAngle(bool value)
        {
            if (_isReachMaxAngle != value)
            {
                _isReachMaxAngle = value;

                SendLimitEndEvent(value ? EventName_OnReachedMaxAngle : EventName_OnDepartedMaxAngle);
            }
        }
        
        private protected void SetIsReachMinYaw(bool value)
        {
            if (_isReachMinYaw != value)
            {
                _isReachMinYaw = value;

                SendLimitEndEvent(value ? EventName_OnReachedMinYaw : EventName_OnDepartedMinYaw);
            }
        }

        private protected void SetIsReachMaxYaw(bool value)
        {
            if (_isReachMaxYaw != value)
            {
                _isReachMaxYaw = value;

                SendLimitEndEvent(value ? EventName_OnReachedMaxYaw : EventName_OnDepartedMaxYaw);
            }
        }

        private protected void SetIsReachMinPitch(bool value)
        {
            if (_isReachMinPitch != value)
            {
                _isReachMinPitch = value;

                SendLimitEndEvent(value ? EventName_OnReachedMinPitch : EventName_OnDepartedMinPitch);
            }
        }

        private protected void SetIsReachMaxPitch(bool value)
        {
            if (_isReachMaxPitch != value)
            {
                _isReachMaxPitch = value;

                SendLimitEndEvent(value ? EventName_OnReachedMaxPitch : EventName_OnDepartedMaxPitch);
            }
        }
        
        private void SendLimitEndEvent(string eventName)
        {
            for (int i = 0; i < _eventReceivers.Length; i++)
            {
                if (!Utilities.IsValid(_eventReceivers[i])) continue;
                if (_eventReceivers[i] == this) continue;

                _eventReceivers[i].SendCustomEvent(eventName);
            }
        }
    }
}