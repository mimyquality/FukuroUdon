using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal static class ObjectUtils
    {
        internal static void FromJsonOverwrite(string json, Object target)
        {
            var clone = Object.Instantiate(target);
            EditorJsonUtility.FromJsonOverwrite(json, clone);
            CopyProperties(clone, target);
            if (clone is Component c) Object.DestroyImmediate(c.gameObject);
            else Object.DestroyImmediate(clone);
        }

        internal static void CopyProperties(Object from, Object to, params string[] ignores)
        {
            using var so = new SerializedObject(to);
            using var soOrig = new SerializedObject(from);
            using var iter = soOrig.GetIterator();
            iter.Next(true);
            CopyFromSerializedProperty(so, iter, ignores);
            while (iter.Next(false))
                CopyFromSerializedProperty(so, iter, ignores);
            so.ApplyModifiedProperties();
        }

        private static void CopyFromSerializedProperty(SerializedObject so, SerializedProperty prop, params string[] ignores)
        {
            if(
                prop.propertyPath == "m_ObjectHideFlags" ||
                prop.propertyPath == "m_CorrespondingSourceObject" ||
                prop.propertyPath == "m_PrefabInstance" ||
                prop.propertyPath == "m_PrefabAsset" ||
                prop.propertyPath == "m_GameObject" ||
                prop.propertyPath == "m_EditorHideFlags" ||
                ignores.Contains(prop.propertyPath)
            ) return;

            so.CopyFromSerializedProperty(prop);
        }
    }

    internal abstract class DirectElements{}
}
