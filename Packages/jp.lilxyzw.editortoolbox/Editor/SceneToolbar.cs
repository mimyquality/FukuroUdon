using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    [Overlay(
        typeof(SceneView),
        ID,
        "lilEditorToolbox",
        true,
        defaultDisplay = true,
        defaultDockZone = DockZone.TopToolbar,
        defaultLayout = Layout.HorizontalToolbar
    )]
    internal class SceneToolbar : ToolbarOverlay
    {
        internal const string ID = "jp.lilxyzw.editortoolbox.SceneToolbar";
        SceneToolbar() : base(MSAAButton.ID){}
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    internal class MSAAButton : ToolbarToggle
    {
        internal const string ID = "jp.lilxyzw.editortoolbox.MSAAButton";
        private static readonly Dictionary<SceneView, RenderTexture> prevRTs = new();
        private static readonly HashSet<MSAAButton> buttons = new();

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            SceneView.duringSceneGui += UpdateMSAA;
        }

        MSAAButton()
        {
            style.fontSize = 10;
            style.unityFontStyleAndWeight = FontStyle.Bold;
            text = "MSAA";
            value = EditorToolboxSettings.instance.enableMSAA;
            this.RegisterValueChangedCallback(e => {
                EditorToolboxSettings.instance.enableMSAA = e.newValue;
                foreach(var button in buttons) button.value = e.newValue;
                EditorToolboxSettings.Save();
            });
            buttons.Add(this);
        }

        private static void UpdateMSAA(SceneView sceneView)
        {
            if(EditorToolboxSettings.instance.enableMSAA)
            {
                var rt = sceneView.camera.targetTexture;
                if(!rt) return;
                prevRTs.TryGetValue(sceneView, out var rtPrev);
                if(rt == rtPrev) return;

                rt.Release();
                rt.antiAliasing = QualitySettings.antiAliasing;
                rt.Create();
                prevRTs[sceneView] = rt;
            }
            else
            {
                var rt = sceneView.camera.targetTexture;
                if(!rt || !prevRTs.TryGetValue(sceneView, out var rtPrev)) return;
                if(rt == rtPrev)
                {
                    rt.Release();
                    rt.antiAliasing = 1;
                    rt.Create();
                }
                prevRTs.Remove(sceneView);
            }
        }
    }
}
