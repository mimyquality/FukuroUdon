/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;

    public enum ActiveRelayActiveEvent
    {
        ActiveAndInactive,
        Active,
        Inactive,
        Ignore
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ActiveRelayTo : UdonSharpBehaviour
    {
        private protected virtual void OnEnable() { }
        private protected virtual void OnDisable() { }
    }
}
