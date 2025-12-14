using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Transform Initialization/Copy Tool",
        "You can restore the Transform of any object to its prefab state or copy it from another object. It is intended to be used when an avatar is stuck in a crouching position and cannot return to its original position."
    )]
    [DocsHowTo("Just set the crouching avatar and costume to `Editing target` and press the button. Generally, `Reset to prefab` is recommended, but if the reference is broken when unpacking the prefab, use `Reset to animator`. If you can't do that with a costume that is not set to Humanoid, set the original prefab to the copy source of `Copy from other object` and press the button to copy the state from the prefab.")]
    [DocsMenuLocation(Common.MENU_HEAD + "Transform Resetter")]
    internal class TransformResetter : EditorWindow
    {
        [MenuItem(Common.MENU_HEAD + "Transform Resetter")]
        static void Init() => GetWindow(typeof(TransformResetter)).Show();

        [DocsField] private static readonly string[] L_TARGET = {"Edit target", "This is the target for Transform editing."};
        [DocsField] private static readonly string[] L_PREFAB = {"Reset to prefab", "Resets the prefab to its initial state. This is only available when the prefab has not been unpacked."};
        [DocsField] private static readonly string[] L_ANIMATOR = {"Reset to animator", "Resets the Animator to its initial state. Use this if you have unpacked the Prefab."};
        [DocsField] private static readonly string[] L_COPY = {"Copy from other object", "Copies the Transform from another selected object. Use this when neither of the above menus can be used."};

        [DocsField] private static readonly string[] L_ALL = {"All transforms", "The process is performed on all Transforms."};
        [DocsField] private static readonly string[] L_HUMANOID = {"Humanoid transforms", "The process is performed on humanoid bones."};
        [DocsField] private static readonly string[] L_FROM = {"Copy from", "This is the source of the Transform copy. The object will be copied from this object to the one being edited."};

        private static readonly HumanBodyBones[] humanBodyBones = (Enum.GetValues(typeof(HumanBodyBones)) as HumanBodyBones[]).Where(h => h != HumanBodyBones.LastBone).ToArray();
        public GameObject target;
        public GameObject prefab;
        public bool isHuman = false;
        public bool isPrefab = false;
        public bool resetPosition = true;
        public bool resetRotation = true;
        public bool resetScale = false;

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            target = L10n.ObjectField(L_TARGET, target, typeof(GameObject), true) as GameObject;
            if(EditorGUI.EndChangeCheck())
            {
                if(target)
                {
                    var animator = target.GetComponent<Animator>();
                    isHuman = animator && animator.isHuman;
                    isPrefab = PrefabUtility.IsPartOfAnyPrefab(target);
                }
                else
                {
                    isHuman = false;
                    isPrefab = false;
                }
            }
            EditorGUI.indentLevel++;
            // 翻訳すると分かりづらいのでキーは作ってない
            resetPosition = L10n.ToggleLeft("Position", resetPosition);
            resetRotation = L10n.ToggleLeft("Rotation", resetRotation);
            resetScale = L10n.ToggleLeft("Scale", resetScale);
            EditorGUI.indentLevel--;

            EditorGUI.BeginDisabledGroup(!target);

            // Reset all transforms
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!isPrefab);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            L10n.LabelField(L_PREFAB, EditorStyles.boldLabel);
            if(L10n.Button(L_ALL))
                ResetAllTransformToPrefab(target, BoolToTarget(resetRotation, resetPosition, resetScale));
            if(L10n.Button(L_HUMANOID))
                ResetHumanoidTransformToPrefab(target, BoolToTarget(resetRotation, resetPosition, resetScale));
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            // Reset humanoid transforms
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!isHuman);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            L10n.LabelField(L_ANIMATOR, EditorStyles.boldLabel);
            if(L10n.Button(L_HUMANOID))
                ResetHumanoidTransformToAvatar(target, BoolToTarget(resetRotation, resetPosition, resetScale));
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            // Copy all transforms
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            L10n.LabelField(L_COPY, EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            prefab = L10n.ObjectField(L_FROM, prefab, typeof(GameObject), true) as GameObject;
            EditorGUI.BeginDisabledGroup(!prefab);
            if(L10n.Button(L_ALL))
                CopyTransforms(target.GetComponent<Transform>(), prefab.GetComponent<Transform>(), BoolToTarget(resetRotation, resetPosition, resetScale));
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();
        }

        private static void ResetAllTransformToPrefab(GameObject target, TransformTarget transformTarget)
        {
            if(!target || !PrefabUtility.IsPartOfAnyPrefab(target)) return;

            var transforms = target.GetComponentsInChildren<Transform>(true);
            foreach(var transform in transforms) ResetTransform(transform, transformTarget);
        }

        private static void ResetHumanoidTransformToPrefab(GameObject target, TransformTarget transformTarget)
        {
            var animator = target.GetComponent<Animator>();
            if(!animator || !animator.isHuman || !PrefabUtility.IsPartOfAnyPrefab(target)) return;

            foreach(var humanBodyBone in humanBodyBones)
                ResetTransform(animator.GetBoneTransform(humanBodyBone), transformTarget);
        }

        private static void ResetHumanoidTransformToAvatar(GameObject target, TransformTarget transformTarget)
        {
            var animator = target.GetComponent<Animator>();
            if(!animator || !animator.isHuman || !animator.avatar) return;

            var skeletonBones = animator.avatar.humanDescription.skeleton;
            foreach(var humanBodyBone in humanBodyBones)
            {
                var transform = animator.GetBoneTransform(humanBodyBone);
                if(!transform) continue;
                var skeletonBone = skeletonBones.FirstOrDefault(s => s.name == transform.name);
                if(string.IsNullOrEmpty(skeletonBone.name)) continue;
                SetTransform(transform, skeletonBone.rotation, skeletonBone.position, skeletonBone.scale, transformTarget);
            }
        }

        private static void CopyTransforms(Transform target, Transform prefab, TransformTarget transformTarget)
        {
            if(!target || !prefab) return;
            for(int i = 0; i < prefab.childCount; i++)
            {
                var childPrefab = prefab.GetChild(i);
                var child = target.Find(childPrefab.name);
                if(!child) continue;
                SetTransform(child, childPrefab.localRotation, childPrefab.localPosition, childPrefab.localScale, transformTarget);
                CopyTransforms(child, childPrefab, transformTarget);
            }
        }

        private static void ResetTransform(Transform transform, TransformTarget transformTarget)
        {
            if(!transform) return;
            using var so = new SerializedObject(transform);
            using var m_LocalRotation = so.FindProperty("m_LocalRotation");
            using var m_LocalPosition = so.FindProperty("m_LocalPosition");
            using var m_LocalScale = so.FindProperty("m_LocalScale");
            if(transformTarget.HasFlag(TransformTarget.Rotation)) PrefabUtility.RevertPropertyOverride(m_LocalRotation, InteractionMode.UserAction);
            if(transformTarget.HasFlag(TransformTarget.Position)) PrefabUtility.RevertPropertyOverride(m_LocalPosition, InteractionMode.UserAction);
            if(transformTarget.HasFlag(TransformTarget.Scale   )) PrefabUtility.RevertPropertyOverride(m_LocalScale   , InteractionMode.UserAction);
        }

        private static void SetTransform(Transform transform, Quaternion rotation, Vector3 position, Vector3 scale, TransformTarget transformTarget)
        {
            using var so = new SerializedObject(transform);
            using var m_LocalRotation = so.FindProperty("m_LocalRotation");
            using var m_LocalPosition = so.FindProperty("m_LocalPosition");
            using var m_LocalScale = so.FindProperty("m_LocalScale");
            if(transformTarget.HasFlag(TransformTarget.Rotation)) m_LocalRotation.quaternionValue = rotation;
            if(transformTarget.HasFlag(TransformTarget.Position)) m_LocalPosition.vector3Value = position;
            if(transformTarget.HasFlag(TransformTarget.Scale   )) m_LocalScale.vector3Value = scale;
            so.ApplyModifiedProperties();
        }

        private static TransformTarget BoolToTarget(bool rotation, bool position, bool scale)
        {
            var transformTarget = TransformTarget.None;
            if(rotation) transformTarget |= TransformTarget.Rotation;
            if(position) transformTarget |= TransformTarget.Position;
            if(scale   ) transformTarget |= TransformTarget.Scale;
            return transformTarget;
        }

        [Flags]
        private enum TransformTarget
        {
            None = 0,
            Rotation = 1 << 0,
            Position = 1 << 1,
            Scale = 1 << 2
        }
    }
}
