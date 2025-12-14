using jp.lilxyzw.editortoolbox.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [CustomEditor(typeof(EditorOnlyBehaviour), true)] [CanEditMultipleObjects]
    internal class LocalizedComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            var iter = serializedObject.GetIterator();
            iter.NextVisible(true);
            while(iter.NextVisible(false)) L10n.PropertyField(iter);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(DirectElements), true)]
    internal class DirectElementsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var end = property.GetEndProperty();
            property.NextVisible(true);
            bool isObjectArray = property.isArray && property.arrayElementType.StartsWith("PPtr");

            position.height = EditorGUI.GetPropertyHeight(property);
            L10n.PropertyField(position, property);
            position.yMin = position.yMax + EditorGUIUtility.standardVerticalSpacing;

            while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, end))
            {
                position.height = EditorGUI.GetPropertyHeight(property);
                L10n.PropertyField(position, property);
                position.yMin = position.yMax + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            using var end = property.GetEndProperty();
            using var iterator = property.Copy();
            iterator.NextVisible(true);
            float height = EditorGUI.GetPropertyHeight(iterator);
            while(iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += EditorGUI.GetPropertyHeight(iterator) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }

}
