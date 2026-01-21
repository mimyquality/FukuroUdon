/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;

    public enum ActiveRelayEventType
    {
        ActiveAndInactive,
        Active,
        Inactive,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ActiveRelayTo : UdonSharpBehaviour
    {
        private protected abstract void OnEnable();
        private protected abstract void OnDisable();
    }
}
