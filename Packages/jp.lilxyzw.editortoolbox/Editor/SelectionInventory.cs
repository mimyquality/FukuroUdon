using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Inventory for any object",
        "This is an inventory that allows you to place any object, assets, folders, etc. Click on the added item to select it immediately."
    )]
    [DocsMenuLocation(Common.MENU_HEAD + "Selection Inventory")]
    internal class SelectionInventory : EditorWindow
    {
        public Vector2 scrollPos;
        public int lastSelection = 0;
        public List<int> selectionIndices = new();
        private SerializedObject serializedObject = null;
        private int clickingIndex = -1;
        private static readonly Color backgroundColor = new(0.5f,0.5f,0.5f,0.1f);
        private static readonly Color colorActive = new(0.1f,0.6f,1.0f,0.333333f);
        private Object currentSelection = null;

        [MenuItem(Common.MENU_HEAD + "Selection Inventory")]
        static void Init() => GetWindow(typeof(SelectionInventory)).Show();

        private void ClearSelection()
        {
            lastSelection = 0;
            selectionIndices.Clear();
        }

        void OnGUI()
        {
            var fullrect = new Rect(0,0,position.width, position.height - 2);
            var e = Event.current;
            var items = DragAndDrop.objectReferences?.Where(o => !SelectionInventoryData.instance.objects.Contains(o));
            var isDraggingFromOther = items?.Count() > 0 && fullrect.Contains(e.mousePosition);
            serializedObject ??= new SerializedObject(SelectionInventoryData.instance);
            serializedObject.UpdateIfRequiredOrScript();

            // D&D中はGUIを無効化
            GUI.enabled = !isDraggingFromOther;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Active object:", GUILayout.ExpandWidth(false));
            using (new EditorGUI.DisabledScope(true))
            {
                currentSelection = EditorGUILayout.ObjectField(currentSelection, typeof(Object), false);
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.MaxHeight(18)))
            {
                Undo.RecordObject(SelectionInventoryData.instance, "Add Active Object");
                SelectionInventoryData.instance.objects.Add(currentSelection);
                SelectionInventoryData.instance.Save();
            }
            EditorGUILayout.EndHorizontal();

            L10n.LabelField("Add Something Here By Drag And Drop");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            using var objects = serializedObject.FindProperty("objects");
            using var iter = objects.Copy();
            iter.NextVisible(true);
            int i = 0;
            while(iter.NextVisible(true))
            {
                bool isSelected = selectionIndices.Contains(i);
                var rect = EditorGUILayout.GetControlRect(false, 16f);
                rect.height = 18;
                var rectLabel = new Rect(rect.x, rect.y, rect.width-20-20-10, rect.height);
                var rectSelectButton = new Rect(rectLabel.xMax, rect.y, 20, rect.height);
                var rectRemoveButton = new Rect(rectSelectButton.xMax+10, rect.y, 20, rect.height);
                if(i%2==1) EditorGUI.DrawRect(rect, backgroundColor);
                if(isSelected) EditorGUI.DrawRect(rect, colorActive);
                EditorGUI.LabelField(rectLabel, EditorGUIUtility.ObjectContent(iter.objectReferenceValue, typeof(Object)));

                // 削除ボタン
                if(GUI.Button(rectRemoveButton, EditorGUIUtility.IconContent("Toolbar Minus")))
                {
                    objects.DeleteArrayElementAtIndex(i);
                    ClearSelection();
                    break;
                }

                // 左クリック
                if(e.button == 0 && rectLabel.Contains(e.mousePosition))
                {
                    // クリック開始点を保持
                    if(e.type == EventType.MouseDown)
                    {
                        clickingIndex = i;
                        e.Use();
                    }

                    // D&Dで外部に持っていく
                    else if(e.type == EventType.MouseDrag && clickingIndex == i && DragAndDrop.objectReferences.Length == 0)
                    {
                        DragAndDrop.PrepareStartDrag();
                        if(selectionIndices.Contains(i))
                            DragAndDrop.objectReferences = selectionIndices.Select(i => SelectionInventoryData.instance.objects[i]).ToArray();
                        else
                            DragAndDrop.objectReferences = new[]{SelectionInventoryData.instance.objects[i]};
                        DragAndDrop.StartDrag("Dragging from Selection Inventory");
                        e.Use();
                    }

                    // クリック開始点と終了点が一致している場合
                    else if((e.type == EventType.MouseUp || e.type == EventType.DragExited) && clickingIndex == i)
                    {
                        var selections2 = Enumerable.Range(Mathf.Min(lastSelection, i), Mathf.Abs(lastSelection - i)+1);
                        // ctrl + shift
                        if(e.control && e.shift)
                        {
                            selectionIndices.AddRange(selections2);
                        }

                        // ctrl
                        else if(e.control)
                        {
                            if(!selectionIndices.Remove(i)) selectionIndices.Add(i);
                        }

                        // shift
                        else if(e.shift)
                        {
                            selectionIndices = selections2.ToList();
                        }

                        // 1つだけ選択しているときにそのオブジェクトをクリックした場合はそこに飛ぶ
                        else if(selectionIndices.Count == 1 && selectionIndices[0] == i)
                        {
                            Selection.activeObject = iter.objectReferenceValue;
                        }

                        // 単体選択
                        else
                        {
                            selectionIndices = new(){i};
                        }

                        if(!e.shift) lastSelection = i;
                        clickingIndex = -1;
                        e.Use();
                    }
                }

                // 右クリック
                else if(e.button == 1 && e.type == EventType.MouseDown && rectLabel.Contains(e.mousePosition))
                {
                    EditorUtility.DisplayCustomMenu(new Rect(e.mousePosition, Vector2.zero), new GUIContent[]{L10n.G("Remove from inventory", null)}, -1, (ci, options, selected) => {
                        if(selected == 0)
                        {
                            using var objects = serializedObject.FindProperty("objects");
                            if(selectionIndices.Contains((int)ci))
                            {
                                foreach(var ind in selectionIndices.OrderByDescending(i=>i))
                                    objects.DeleteArrayElementAtIndex(ind);
                            }
                            else
                            {
                                objects.DeleteArrayElementAtIndex((int)ci);
                            }
                            ClearSelection();
                            if(serializedObject.ApplyModifiedProperties()) SelectionInventoryData.instance.Save();
                        }
                    }, i);
                }

                i++;
            }
            if(e.type == EventType.MouseUp) clickingIndex = -1;

            // Deleteキーで削除
            if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
            {
                foreach(var ind in selectionIndices.OrderByDescending(i=>i))
                    objects.DeleteArrayElementAtIndex(ind);
                ClearSelection();
                e.Use();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUI.enabled = true;

            // 変更を保存
            if(serializedObject.ApplyModifiedProperties()) SelectionInventoryData.instance.Save();

            // D&Dで追加
            if(isDraggingFromOther)
            {
                switch(e.type)
                {
                    case EventType.DragPerform:
                        DragAndDrop.AcceptDrag();
                        Undo.RecordObject(SelectionInventoryData.instance, "Add Objects");
                        SelectionInventoryData.instance.objects.AddRange(items);
                        SelectionInventoryData.instance.Save();
                        e.Use();
                        break;
                    default:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        GUI.Box(fullrect, GUIContent.none, EditorStyles.helpBox);
                        EditorGUI.DrawRect(new Rect(Vector2.zero, new Vector2(fullrect.width/5,fullrect.width/20)){center = fullrect.center}, EditorStyles.label.normal.textColor);
                        EditorGUI.DrawRect(new Rect(Vector2.zero, new Vector2(fullrect.width/20,fullrect.width/5)){center = fullrect.center}, EditorStyles.label.normal.textColor);
                        break;
                }
            }
        }

        void OnSelectionChange()
        {
            currentSelection = Selection.activeObject;
        }
    }
}
