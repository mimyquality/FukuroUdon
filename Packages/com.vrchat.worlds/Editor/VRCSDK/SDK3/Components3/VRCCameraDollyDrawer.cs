using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDK3.Components;

namespace VRC.SDK3.Editor
{
    [InitializeOnLoad]
    public static class VRCCameraDollyDrawer
    {
        private static readonly Color[] SplineColors = 
        {
            new(0f, 0.7f, 0.7f),
            new(1f, 0.5f, 0f),
            new(0.6f, 0f, 0.8f),
            new(0.6f, 0.4f, 0.2f),
            new(0.2f, 0.6f, 0.9f),
            new(0.9f, 0.6f, 0.7f),
            new(0.3f, 0.3f, 0.7f),
            Color.yellow,
            new(0.8f, 0.3f, 0.3f),
            Color.magenta,
        };
        
        private static VRCCameraDollyAnimation ANIMATION;

        static VRCCameraDollyDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
        
        static void OnSceneGUI(SceneView sceneView)
        {
            // don't draw if gizmos are off
            if (!SceneView.currentDrawingSceneView.drawGizmos) return;

            Transform[] selection = Selection.transforms;
            if (selection == null || selection.Length == 0) return;

            // check each selected object
            foreach (Transform selected in selection)
            {
                VRCCameraDollyPath path;

                // if an animation is selected, draw all of its paths
                if (selected.TryGetComponent(out ANIMATION))
                {
                    if (ANIMATION.Paths == null) return;

                    // draw paths
                    int pathIndex;
                    for (pathIndex = 0; pathIndex < ANIMATION.Paths.Length; pathIndex++)
                    {
                        path = ANIMATION.Paths[pathIndex];
                        if (path == null) continue;

                        DrawPath(ANIMATION, path);
                        DrawPoints(ANIMATION, path);
                    }
                }

                else
                {
                    // if a path is selected, draw it
                    if (selected.TryGetComponent(out path))
                        ANIMATION = path.GetComponentInParent<VRCCameraDollyAnimation>();
                
                    // if a point is selected, also draw its path
                    else if (selected.TryGetComponent(out VRCCameraDollyPathPoint point))
                    {
                        ANIMATION = point.GetComponentInParent<VRCCameraDollyAnimation>();
                        path = point.GetComponentInParent<VRCCameraDollyPath>();
                    }

                    // if selected object isn't an animation, path, or point, don't draw anything
                    else return;
                
                    DrawPath(ANIMATION, path);
                    DrawPoints(ANIMATION, path);
                }
            }
        }

        private static void DrawPath(VRCCameraDollyAnimation animation, VRCCameraDollyPath path)
        {
            if (!IsPathValid(animation, path)) return;

            int index = path.transform.GetSiblingIndex();
            Handles.color = animation.gameObject.activeInHierarchy 
                ? SplineColors[index % SplineColors.Length] 
                : Color.gray;

            var points = path.Points
                .Where(p => p != null)
                .Select(p => p.transform.position)
                .ToList();

            VRCCameraDollyAnimation.VRCCameraDollyPathType pathType = animation.PathType;
            if (points.Count < 4)
            {
                pathType = VRCCameraDollyAnimation.VRCCameraDollyPathType.Linear;
            }

            if (pathType == VRCCameraDollyAnimation.VRCCameraDollyPathType.Linear)
            {
                for (int i = 0; i <= points.Count - 2; i++)
                {
                    Handles.DrawLine(points[i], points[i+1], 5);
                }
            }
            else
            {
                Vector3 prev = GetPointOnSpline(points, 0f, pathType);
                for (int i = 1; i <= VRCCameraDollyAnimation.RESOLUTION; i++)
                {
                    float t = i / (float)VRCCameraDollyAnimation.RESOLUTION;
                    Vector3 curr = GetPointOnSpline(points, t, pathType);
                    Handles.DrawLine(prev, curr, 5);
                    prev = curr;
                }
            }

            // draw path label in the path's color
            Handles.Label(points[0] + new Vector3(0,0.3f,0), $"Path {index+1}", 
                new GUIStyle { fontSize = 25, normal = new GUIStyleState { textColor = Handles.color } });
        }

        private static void DrawPoints(VRCCameraDollyAnimation animation, VRCCameraDollyPath path)
        {
            if (!IsPathValid(animation, path)) return;

            for (int i = 0; i < path.Points.Length; i++)
            {
                var point = path.Points[i];
                if (point == null) continue;

                if (animation.PathType != VRCCameraDollyAnimation.VRCCameraDollyPathType.Linear &&
                    path.Points.Length >= 4 && (i == 0 || i == path.Points.Length - 1))
                {
                    Handles.color = Color.red;
                    Handles.DrawWireCube(point.transform.position, new Vector3(0.15f, 0.15f, 0.15f));
                    Handles.Label(point.transform.position + new Vector3(0,0.1f,0), $"{(i == 0 ? "Start" : "End")} Anchor");

                    // anchors show a dotted line
                    Handles.color = Color.blue;
                    if (i == 0)
                        Handles.DrawDottedLine(point.transform.position, path.Points[1].transform.position, 5);
                    else
                        Handles.DrawDottedLine(path.Points[i - 1].transform.position, point.transform.position,
                            5);
                }
                else
                {
                    Vector3 pos = point.transform.position;
                    Quaternion rot = point.transform.rotation;
                    
                    Handles.color = Color.grey;
                    Handles.SphereHandleCap(0, pos, rot, 0.1f, EventType.Repaint);
                    Handles.Label(point.transform.position + new Vector3(0,0.1f,0), $"Point {i+1}");
                }
            }
        }

        static bool IsPathValid(VRCCameraDollyAnimation animation, VRCCameraDollyPath path)
        {
            if (animation == null || 
                path == null || 
                path.Points == null || 
                path.Points.Length < 2) 
                return false;
            
            return true;
        }
        
        static Vector3 GetPointOnSpline(List<Vector3> points, float t, VRCCameraDollyAnimation.VRCCameraDollyPathType pathType)
        {
            return pathType switch
            {
                VRCCameraDollyAnimation.VRCCameraDollyPathType.Linear => Linear(points, t),
                VRCCameraDollyAnimation.VRCCameraDollyPathType.Loose => CatmullRom(points, t),
                VRCCameraDollyAnimation.VRCCameraDollyPathType.Fitted => BSpline(points, t),
                _ => Vector3.zero
            };
        }

        static Vector3 Linear(List<Vector3> pts, float t)
        {
            int i = Mathf.FloorToInt(t * (pts.Count - 1));
            i = Mathf.Clamp(i, 0, pts.Count - 2);
            float localT = t * (pts.Count - 1) - i;
            return Vector3.Lerp(pts[i], pts[i + 1], localT);
        }

        static Vector3 CatmullRom(List<Vector3> points, float t)
        {
            if (points.Count < 4) return Linear(points, t);

            int numSections = points.Count - 3;
            float u = t * numSections;
            int i = Mathf.Clamp(Mathf.FloorToInt(u), 0, numSections - 1);
            u -= i;

            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1];
            Vector3 p2 = points[i + 2];
            Vector3 p3 = points[i + 3];

            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * u +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * u * u +
                (-p0 + 3f * p1 - 3f * p2 + p3) * u * u * u
            );
        }

        private static Vector3 BSpline(List<Vector3> points, float t)
        {
            if (points.Count < 4) return Linear(points, t);

            int numSections = points.Count - 3;
            float u = t * numSections;
            int i = Mathf.Clamp(Mathf.FloorToInt(u), 0, numSections - 1);
            u -= i;

            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1];
            Vector3 p2 = points[i + 2];
            Vector3 p3 = points[i + 3];
            
            return GetBSplinePoint(p0, p1, p2, p3, u);
        }
        
        private static Vector3 GetBSplinePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float b0 = (-t3 + 3f * t2 - 3f * t + 1f) / 6f;
            float b1 = (3f * t3 - 6f * t2 + 4f) / 6f;
            float b2 = (-3f * t3 + 3f * t2 + 3f * t + 1f) / 6f;
            float b3 = t3 / 6f;
            Vector3 position = b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
            return position;
        }
    }

    [CustomEditor(typeof(VRCCameraDollyAnimation))]
    public class VRCCameraDollyAnimationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Collect Paths & Points"))
            {
                var animation = (VRCCameraDollyAnimation)target;
                SerializedObject animationSO = new SerializedObject(animation);
                SerializedProperty pathsProp = animationSO.FindProperty("Paths");
                pathsProp.ClearArray();

                int pathIndex = 0;
                foreach (Transform path in animation.transform)
                {
                    if (!path.TryGetComponent<VRCCameraDollyPath>(out var pathComponent)) continue;

                    SerializedObject pathSO = new SerializedObject(pathComponent);
                    SerializedProperty pointsProp = pathSO.FindProperty("Points");
                    pointsProp.ClearArray();

                    int pointIndex = 0;
                    foreach (Transform point in path)
                    {
                        if (!point.TryGetComponent<VRCCameraDollyPathPoint>(out var pointComponent)) continue;
                        if (pointComponent != null)
                        {
                            pointsProp.InsertArrayElementAtIndex(pointIndex);
                            pointsProp.GetArrayElementAtIndex(pointIndex).objectReferenceValue = pointComponent;
                            pointIndex++;
                        }
                    }

                    pathSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(pathComponent);

                    pathsProp.InsertArrayElementAtIndex(pathIndex);
                    pathsProp.GetArrayElementAtIndex(pathIndex).objectReferenceValue = pathComponent;
                    pathIndex++;
                }

                animationSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(animation);
                EditorSceneManager.MarkSceneDirty(animation.gameObject.scene);
            }
        }
    }
    
    [CustomEditor(typeof(VRCCameraDollyPath))]
    public class VRCCameraDollyPathEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Collect Points"))
            {
                var path = (VRCCameraDollyPath)target;
                SerializedObject pathSO = new SerializedObject(path);
                SerializedProperty pointsProp = pathSO.FindProperty("Points");
                pointsProp.ClearArray();

                int pointIndex = 0;
                foreach (Transform point in path.transform)
                {
                    if (!point.TryGetComponent<VRCCameraDollyPathPoint>(out var pointComponent)) continue;
                    if (pointComponent != null)
                    {
                        pointsProp.InsertArrayElementAtIndex(pointIndex);
                        pointsProp.GetArrayElementAtIndex(pointIndex).objectReferenceValue = pointComponent;
                        pointIndex++;
                    }
                }

                pathSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(path);
                EditorSceneManager.MarkSceneDirty(path.gameObject.scene);
            }
        }
    }
}