using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace VRC.Dynamics
{
    public class ContactReceiverUdonEmitter : IContactReceiverUdonEmitter
    {
        private readonly IUdonBehaviour[] _udonBehaviours;

        private const string EventOnContactEnter = "_onContactEnter";
        private const string EventOnContactExit = "_onContactExit";

        public ContactReceiverUdonEmitter(GameObject hostGameObject)
        {
            _udonBehaviours = hostGameObject.GetComponents<IUdonBehaviour>();
        }

        public void OnContactEnter(ContactEnterInfo contactInfo)
        {
            foreach (IUdonBehaviour udonBehaviour in _udonBehaviours)
            {
                udonBehaviour.RunEvent(EventOnContactEnter, ("contactInfo", contactInfo));
            }
        }

        public void OnContactExit(ContactExitInfo contactInfo)
        {
            foreach (IUdonBehaviour udonBehaviour in _udonBehaviours)
            {
                udonBehaviour.RunEvent(EventOnContactExit, ("contactInfo", contactInfo));
            }
        }
    }
}
