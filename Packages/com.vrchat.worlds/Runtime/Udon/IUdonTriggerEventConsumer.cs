using UnityEngine;

namespace VRC.Udon
{
    public interface IUdonTriggerEventConsumer
    {
        int Priority { get; }

        bool TryConsumeOnTriggerEnter(UdonBehaviour udonBehaviour, Collider other);
        bool TryConsumeOnTriggerExit(UdonBehaviour udonBehaviour, Collider other);
        bool TryConsumeOnTriggerStay(UdonBehaviour udonBehaviour, Collider other);
    }
}