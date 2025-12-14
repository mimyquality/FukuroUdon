using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace VRC.Dynamics
{
    public class PhysBoneUdonEmitter : IPhysBoneUdonEmitter
    {
        private readonly IUdonBehaviour[] _udonBehaviours;

        private const string EventOnPhysBoneGrabbed = "_onPhysBoneGrabbed";
        private const string EventOnPhysBoneReleased = "_onPhysBoneReleased";
        private const string EventOnPhysBonePosed = "_onPhysBonePosed";
        private const string EventOnPhysBoneUnPosed = "_onPhysBoneUnPosed";

        public PhysBoneUdonEmitter(GameObject hostGameObject)
        {
            _udonBehaviours = hostGameObject.GetComponents<IUdonBehaviour>();
        }

        public void OnPhysBoneGrabbed(PhysBoneGrabbedInfo physBoneInfo)
        {
            foreach (IUdonBehaviour udonBehaviour in _udonBehaviours)
            {
                udonBehaviour.RunEvent(EventOnPhysBoneGrabbed, ("physBoneInfo", physBoneInfo));
            }
        }

        public void OnPhysBoneReleased(PhysBoneReleasedInfo physBoneInfo)
        {
            foreach (IUdonBehaviour udonBehaviour in _udonBehaviours)
            {
                udonBehaviour.RunEvent(EventOnPhysBoneReleased, ("physBoneInfo", physBoneInfo));
            }
        }

        public void OnPhysBonePosed(PhysBonePosedInfo physBoneInfo)
        {
            foreach (IUdonBehaviour udonBehaviour in _udonBehaviours)
            {
                udonBehaviour.RunEvent(EventOnPhysBonePosed, ("physBoneInfo", physBoneInfo));
            }
        }

        public void OnPhysBoneUnPosed(PhysBoneUnPosedInfo physBoneInfo)
        {
            foreach (IUdonBehaviour udonBehaviour in _udonBehaviours)
            {
                udonBehaviour.RunEvent(EventOnPhysBoneUnPosed, ("physBoneInfo", physBoneInfo));
            }
        }
    }
}