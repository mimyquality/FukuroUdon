using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Batch replacement tool for all object references",
        "Replace all object references in all objects at once. For example, you can replace the avatar's materials all at once, or even replace the textures within the materials at once."
    )]
    [DocsHowTo("Simply set the object you want to edit (such as an avatar) to `Edit target`, and set the objects before and after the modification to `From` and `To`! When you press the execute button, all references will be replaced and the edited results will be displayed.")]
    [DocsMenuLocation(Common.MENU_HEAD + "Reference Replacer")]
    internal class ReferenceReplacer : EditorWindow
    {
        public Vector2 scrollPos;
        public Object target;
        public Object from;
        public Object to;
        public HashSet<Object> modified = new();
        [MenuItem(Common.MENU_HEAD + "Reference Replacer")]
        static void Init() => GetWindow(typeof(ReferenceReplacer)).Show();

        [DocsField] private static readonly string[] L_TARGET = {"Edit target", "This is the object to be edited."};
        [DocsField] private static readonly string[] L_FROM = {"From", "The object before replacement."};
        [DocsField] private static readonly string[] L_TO = {"To", "This is the object after replacement."};
        [DocsField] private static readonly string[] L_MODIFIED = {"Modified Objects", "The objects that have been edited."};

        void OnGUI()
        {
            target = L10n.ObjectField(L_TARGET, target, typeof(Object), true);
            EditorGUILayout.Space();
            from = L10n.ObjectField(L_FROM, from, typeof(Object), true);
            to = L10n.ObjectField(L_TO, to, typeof(Object), true);
            EditorGUILayout.Space();
            if(L10n.Button("Run"))
            {
                modified.Clear();
                var scaned = new HashSet<Object>();
                if(target is GameObject gameObject)
                {
                    Replace(scaned, gameObject.GetComponentsInChildren<Component>(true));
                }
                else if(target is SceneAsset)
                {
                    if(AssetDatabase.GetAssetPath(target) != SceneManager.GetActiveScene().path)
                    {
                        EditorUtility.DisplayDialog("Reference Replacer", L10n.L("Please open a scene before running."), L10n.L("OK"));
                        return;
                    }
                    else
                    {
                        Replace(scaned, SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Component>(true)).ToArray());
                    }
                }
                else
                {
                    var assetPath = AssetDatabase.GetAssetPath(target);
                    if(!string.IsNullOrEmpty(assetPath)) Replace(scaned, AssetDatabase.LoadAllAssetsAtPath(assetPath));
                    else Replace(scaned, target);
                }
                EditorUtility.DisplayDialog("Reference Replacer", L10n.L("Complete!"), L10n.L("OK"));
            }
            if(modified.Count > 0)
            {
                EditorGUILayout.Space();
                L10n.LabelField(L_MODIFIED, EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                EditorGUI.BeginDisabledGroup(true);
                foreach(var m in modified) EditorGUILayout.ObjectField(m, typeof(Object), true);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        private void Replace(HashSet<Object> scaned, Object[] objs)
        {
            foreach(var obj in objs) Replace(scaned, obj);
        }

        private Object Replace(HashSet<Object> scaned, Object obj)
        {
            if(obj == from) obj = to;

            if(!obj || scaned.Contains(obj)) return obj;
            scaned.Add(obj);
            if(Common.SkipScan(obj)) return obj;

            using var so = new SerializedObject(obj);
            using var iter = so.GetIterator();
            var enterChildren = true;
            var isDirty = false;
            while(iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if(iter.propertyType == SerializedPropertyType.ObjectReference && iter.name != "m_CorrespondingSourceObject")
                {
                    var replaced = Replace(scaned, iter.objectReferenceValue);
                    if(iter.objectReferenceValue != replaced)
                    {
                        iter.objectReferenceValue = replaced;
                        isDirty = true;
                    }
                }
            }
            if(isDirty)
            {
                so.ApplyModifiedProperties();
                modified.Add(obj);
            }
            return obj;
        }
    }
}
