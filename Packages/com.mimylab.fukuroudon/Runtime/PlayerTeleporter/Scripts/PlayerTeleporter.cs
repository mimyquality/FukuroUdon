
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
//using VRC.SDK3.Components;

namespace MimyLab
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerTeleporter : UdonSharpBehaviour
    {
        VRCPlayerApi _localPlayer;

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
        }

        public void _TeleportToTransform()
        {
            _TeleportToTransform(this.transform);
        }
        public void _TeleportToTransform(Transform target)
        {
            _localPlayer.TeleportTo(target.position, target.rotation, VRC_SceneDescriptor.SpawnOrientation.Default, false);
        }

        public void _TeleportToPlayer(VRCPlayerApi target)
        {
            if (!Utilities.IsValid(target)) { return; }

            _localPlayer.TeleportTo(target.GetPosition(), target.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.Default, false);
        }
    }
}
