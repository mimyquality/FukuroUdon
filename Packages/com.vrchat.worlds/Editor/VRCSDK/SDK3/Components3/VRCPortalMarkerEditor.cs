
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRCPortalMarker = VRC.SDK3.Components.VRCPortalMarker;


namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCPortalMarker))]
    public class VRCPortalMarkerEditor : VRCInspectorBase
    {
        
        private SerializedProperty propWorld;
        private SerializedProperty propRoomId;
        private SerializedProperty propCustomPortalName;

        private PropertyField fieldWorld;
        private PropertyField fieldRoomId;
        private PropertyField fieldCustomPortalName;

        private HelpBox helpBoxTag;
        
        
        private void OnEnable()
        {
            propWorld = serializedObject.FindProperty(nameof(VRCPortalMarker.world));
            propRoomId = serializedObject.FindProperty(nameof(VRCPortalMarker.roomId));
            propCustomPortalName = serializedObject.FindProperty(nameof(VRCPortalMarker.customPortalName));
        }

        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();
            fieldRoomId = AddFieldLabel(propRoomId, "World ID");
            fieldCustomPortalName = AddField(propCustomPortalName);
            fieldWorld = AddField(propWorld);
            fieldWorld.RegisterValueChangeCallback(evt => WorldChanged());
            
            WorldChanged();
        }
        
        private void WorldChanged()
        {
            bool value = (VRC_PortalMarker.VRChatWorld)propWorld.enumValueIndex == VRC_PortalMarker.VRChatWorld.None;
            fieldRoomId.SetEnabled(value);
        }
    }
}