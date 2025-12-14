using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Shows the icon if the object is EditorOnly.")]
    internal class EditorOnlyLabel : IHierarchyExtensionComponent
    {
        public int Priority => -800;

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            var rectEO = fullRect;
            rectEO.x = 36;
            rectEO.width = rectEO.height;

            EditorGUI.BeginChangeCheck();
            var isEditorOnly = GUIHelper.DToggleMiniLabel("E", "jp.lilxyzw.editortoolbox.EditorOnlyLabel", instanceID.ToString(), rectEO, IsEditorOnly(gameObject.transform));
            if(EditorGUI.EndChangeCheck())
            {
                using var so = new SerializedObject(gameObject);
                using var m_TagString = so.FindProperty("m_TagString");
                m_TagString.stringValue = isEditorOnly ? "EditorOnly" : "Untagged";
                so.ApplyModifiedProperties();
            }
        }

        private static bool IsEditorOnly(Transform obj)
        {
            if(obj.tag == "EditorOnly") return true;
            if(obj.transform.parent == null) return false;
            return IsEditorOnly(obj.transform.parent);
        }
    }
}
