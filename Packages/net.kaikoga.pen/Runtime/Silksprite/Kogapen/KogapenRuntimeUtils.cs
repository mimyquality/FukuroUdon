using System.Reflection;
using UdonSharp;
using UnityEditor;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Silksprite.Kogapen
{
    public static class KogapenRuntimeUtils
    {
        #region copy of UdonSharpEditorUtility.GetBackingUdonBehaviour()

        const string BackingFieldName = "_udonSharpBackingUdonBehaviour";
        static readonly FieldInfo BackingBehaviourField = typeof(UdonSharpBehaviour).GetField(BackingFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        static UdonBehaviour GetBackingUdonBehaviour(UdonSharpBehaviour behaviour)
        {
            return (UdonBehaviour)BackingBehaviourField.GetValue(behaviour);
        }

        #endregion

        // This effectively implements BehaviourSyncMode.Auto (Continuous only if VRCObjectSync exists, else Manual)  
        public static void ValidateAutoSyncType(UdonSharpBehaviour behaviour)
        {
            var backingUdon = GetBackingUdonBehaviour(behaviour);
            var oldSyncMethod = backingUdon.SyncMethod;
            // NOTE: skip check other Continuous sync udon because Kogapen use Manual only   
            var newSyncMethod = behaviour.GetComponent<VRCObjectSync>() ? Networking.SyncType.Continuous : Networking.SyncType.Manual;
            if (oldSyncMethod == newSyncMethod) return;
            backingUdon.SyncMethod = newSyncMethod;
#if UNITY_EDITOR
            Undo.RecordObject(backingUdon, "Update Auto Sync Type");
            EditorUtility.SetDirty(backingUdon);
#endif
        }
    }
}