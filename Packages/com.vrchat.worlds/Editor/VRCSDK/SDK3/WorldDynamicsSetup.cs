using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone;

namespace VRC.SDK3.Dynamics
{
    /// <summary>
    /// World specific VRC dynamics setup.
    /// </summary>
    public static class WorldDynamicsSetup
    {
        [InitializeOnLoadMethod]
        private static void EditorInit()
        {
            // WARNING: Cannot use #else due to stripping limitations
            DynamicsComponent.DefaultUsage =
#if !VRC_ENABLE_PROPS
                DynamicsUsage.World;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitInitializeEvents()
        {
            ContactBase.OnInitialize = Contact_OnInitialize;
            VRCPhysBoneBase.OnInitialize = PhysBone_OnInitialize;
            VRCPhysBoneColliderBase.OnPreShapeInitialize += PhysBoneCollider_OnPreShapeInitialize;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitPhysBoneGrabHelper()
        {
            // PhysBoneManager singleton should exist following DynamicsSetup.
            PhysBoneManager.Inst.gameObject.AddComponent<PhysBoneGrabHelper>();
        }

        private static bool Contact_OnInitialize(ContactBase contact)
        {
            // WARNING: Cannot use #else due to stripping limitations
#if !VRC_ENABLE_PROPS
            contact.Usage = DynamicsUsage.World;
#endif

            // Let receivers emit Udon events
            if (contact is ContactReceiver receiver)
            {
                IContactReceiverUdonEmitter emitter = new ContactReceiverUdonEmitter(contact.gameObject);
                receiver.AssignUdonEmitter(emitter);
            }

            return true;
        }

        private static void PhysBone_OnInitialize(VRCPhysBoneBase physBone)
        {
            // WARNING: Cannot use #else due to stripping limitations
#if !VRC_ENABLE_PROPS
            physBone.Usage = DynamicsUsage.World;
#endif

            // Let physbones emit Udon events
            IPhysBoneUdonEmitter emitter = new PhysBoneUdonEmitter(physBone.gameObject);
            physBone.AssignUdonEmitter(emitter);
        }

        private static void PhysBoneCollider_OnPreShapeInitialize(VRCPhysBoneColliderBase physBoneCollider)
        {
            // WARNING: Cannot use #else due to stripping limitations
#if !VRC_ENABLE_PROPS
            physBoneCollider.Usage = DynamicsUsage.World;
#endif
        }
    }
}