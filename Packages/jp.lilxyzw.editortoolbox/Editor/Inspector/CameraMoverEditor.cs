using jp.lilxyzw.editortoolbox.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [CustomEditor(typeof(CameraMover))]
    internal class CameraMoverEditor : LocalizedComponentEditor
    {
        private bool isDragging = false;
        private Vector2 lastCursorPos;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox(string.Join("\r\n", new[]{
                L10n.L("W: Front"),
                L10n.L("A: Left"),
                L10n.L("S: Back"),
                L10n.L("D: Right"),
                L10n.L("Q: Up"),
                L10n.L("E: Down"),
                L10n.L("Esc: End"),
                L10n.L("Wheel: Change Scroll Speed")
            }), MessageType.Info);

            EditorGUI.BeginChangeCheck();
            L10n.Toggle("Operate", isDragging);
            if(EditorGUI.EndChangeCheck())
            {
                SetDragging(!isDragging);
            }
            MoveCamera();
        }

        private void MoveCamera()
        {
            if(!isDragging || target is not CameraMover cameraMover) return;
            var transform = cameraMover.transform;
            var e = Event.current;

            // Position
            if(e.type == EventType.KeyDown)
            {
                var distance = Mathf.Min(Time.deltaTime, 0.02f) * cameraMover.moveSpeed;
                switch(e.keyCode)
                {
                    case KeyCode.W:
                        transform.position += transform.forward * distance;
                        break;
                    case KeyCode.A:
                        transform.position -= transform.right * distance;
                        break;
                    case KeyCode.S:
                        transform.position -= transform.forward * distance;
                        break;
                    case KeyCode.D:
                        transform.position += transform.right * distance;
                        break;
                    case KeyCode.Q:
                        transform.position += transform.up * distance;
                        break;
                    case KeyCode.E:
                        transform.position -= transform.up * distance;
                        break;
                    case KeyCode.Escape:
                        SetDragging(false);
                        return;
                }
                e.Use();
            }
            // End
            else if(e.type == EventType.MouseDown)
            {
                SetDragging(false);
                e.Use();
                return;
            }
            // Move Speed
            else if(e.type == EventType.ScrollWheel)
            {
                cameraMover.moveSpeed = Mathf.Max(0, cameraMover.moveSpeed - e.delta.y * 0.1f);
                e.Use();
            }
            // Rotation
            else if(e.type == EventType.Repaint)
            {
                var rotation = 0.01f * cameraMover.sensitivity * (MouseUtils.GetPos() - lastCursorPos);
                var euler = transform.rotation.eulerAngles;
                euler.x += rotation.y;
                euler.y += rotation.x;
                transform.rotation = Quaternion.Euler(euler);
                lastCursorPos = MouseUtils.SetPos(lastCursorPos);
            }

            Repaint();
        }

        private void SetDragging(bool dragging)
        {
            isDragging = dragging;
            lastCursorPos = MouseUtils.GetPos();
            MouseUtils.Hide(isDragging);
        }
    }
}
