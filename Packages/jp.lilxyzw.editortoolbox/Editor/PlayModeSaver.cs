using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Save changes in PlayMode",
        "Values changed in PlayMode can be maintained even after exiting PlayMode. Click the hamburger menu in the upper right corner of each component and select `Save changes in PlayMode` to save the changes to that component."
    )]
    public static class PlayModeSaver
    {
        private static Dictionary<int, (string json, int? instanceID, Type type)> changes = new();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged -= Apply;
            EditorApplication.playModeStateChanged += Apply;
        }

        [MenuItem("CONTEXT/Object/Save changes in PlayMode")]
        private static void SaveAfterPlayMode(MenuCommand command)
        {
            var json = EditorJsonUtility.ToJson(command.context);
            var id = command.context.GetInstanceID();
            changes[id] = (json, command.context is Component co ? co.gameObject.GetInstanceID() : null, command.context.GetType());
        }

        [MenuItem("CONTEXT/Object/Save changes in PlayMode", true)]
        private static bool SaveAfterPlayMode() => EditorApplication.isPlaying || EditorApplication.isPaused;

        private static void Apply(PlayModeStateChange state)
        {
            if(state != PlayModeStateChange.EnteredEditMode || changes.Count == 0) return;
            foreach(var kv in changes)
            {
                var obj = EditorUtility.InstanceIDToObject(kv.Key);

                // PlayModeで追加されたコンポーネントの場合は追加処理
                if(!obj && kv.Value.instanceID != null && EditorUtility.InstanceIDToObject((int)kv.Value.instanceID) is GameObject go && go)
                {
                    obj = Undo.AddComponent(go, kv.Value.type);
                }

                if(obj) ObjectUtils.FromJsonOverwrite(kv.Value.json, obj);
            }
            changes.Clear();
        }
    }
}
