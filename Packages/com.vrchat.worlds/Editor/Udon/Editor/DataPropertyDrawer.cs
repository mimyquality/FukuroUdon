using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Object = System.Object;

namespace VRC.SDK3.Data.Editor
{
    [CustomPropertyDrawer(typeof(DataToken))]
    [CustomPropertyDrawer(typeof(DataList))]
    [CustomPropertyDrawer(typeof(DataDictionary))]
    public class DataPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 30 * EditorGUIUtility.pixelsPerPoint;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, $"{property.name} no defined editor for type of {property.type}");
        }
    }
}