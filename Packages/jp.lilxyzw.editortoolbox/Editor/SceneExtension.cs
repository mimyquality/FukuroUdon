using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Scene View Extensions",
        "A button to turn on anti-aliasing in the scene view and a camera mode (Vertex Attribute) has been added to check the vertex data of the model.",
        "",
        "> [!WARNING]",
        "> Some functions do not work in the Metal environment because the camera mode extension uses a geometry shader.",
        "",
        "## MSAA",
        "",
        "Click the MSAA button on the toolbar above the Scene View to toggle anti-aliasing on and off. Anti-aliasing is always applied, allowing you to adjust materials in a way that is close to how they will actually appear.",
        "",
        "## Camera Modes",
        "",
        "This extension function will be enabled when you click the camera mode change button (which switches to Wireframe display, etc.) on the toolbar above the Scene View and select Vertex Attribute. A popup will appear in the upper left of the Scene View that allows you to switch the vertex data to be displayed, so you can change this to check the desired data."
    )]
    internal class SceneExtension
    {
        private static Shader shaderAttributeViewer = Shader.Find("Hidden/_lil/AttributeViewer");
        private static readonly int propMode = Shader.PropertyToID("_AVOutputMode");

        private const string SECTION_NAME = "lil";
        private const string MODE_ATTRIBUTE = "Vertex Attribute";

        private static readonly ModeInfo[] MODES =
        {
            new ModeInfo( 0, "UV/UV0 xy"),
            new ModeInfo( 1, "UV/UV0 zw"),
            new ModeInfo( 2, "UV/UV1 xy"),
            new ModeInfo( 3, "UV/UV1 zw"),
            new ModeInfo( 4, "UV/UV2 xy"),
            new ModeInfo( 5, "UV/UV2 zw"),
            new ModeInfo( 6, "UV/UV3 xy"),
            new ModeInfo( 7, "UV/UV3 zw"),
            new ModeInfo( 8, "UV/UV4 xy"),
            new ModeInfo( 9, "UV/UV4 zw"),
            new ModeInfo(10, "UV/UV5 xy"),
            new ModeInfo(11, "UV/UV5 zw"),
            new ModeInfo(12, "UV/UV6 xy"),
            new ModeInfo(13, "UV/UV6 zw"),
            new ModeInfo(14, "UV/UV7 xy"),
            new ModeInfo(15, "UV/UV7 zw"),
            new ModeInfo(16, "Local Position"),
            new ModeInfo(17, "Vertex Color/RGB"),
            new ModeInfo(18, "Vertex Color/R"),
            new ModeInfo(19, "Vertex Color/G"),
            new ModeInfo(20, "Vertex Color/B"),
            new ModeInfo(21, "Vertex Color/A"),
            new ModeInfo(22, "Vector/Local Normal"),
            new ModeInfo(23, "Vector/World Normal"),
            new ModeInfo(24, "Vector/World Normal (Line)", true),
            new ModeInfo(25, "Vector/Local Tangent"),
            new ModeInfo(26, "Vector/World Tangent"),
            new ModeInfo(27, "Vector/World Tangent (Line)", true),
            new ModeInfo(28, "Vector/Tangent W"),
            new ModeInfo(29, "Vertex ID"),
            new ModeInfo(30, "Face Orientation")
        };

        private static GUIContent[] ActiveModeNames;
        private static ModeInfo[] ActiveModes;

        readonly struct ModeInfo
        {
            public readonly int Index;
            public readonly string Name;
            public readonly bool GeometryRequired;

            public ModeInfo(int index, string name, bool geometryRequired = false)
            {
                Name = name;
                Index = index;
                GeometryRequired = geometryRequired;
            }
        }

        // index in the ActiveModes array
        private static int currentModeIndex = -1;

        [InitializeOnLoadMethod]
        private static void InitializeCameraMode()
        {
            SceneView.AddCameraMode(MODE_ATTRIBUTE, SECTION_NAME);
            EditorApplication.delayCall += SetupSceneViews;

            SceneView.duringSceneGui += view =>
            {
                var mode = view.cameraMode;
                if(mode.section != SECTION_NAME) return;
                switch(mode.name)
                {
                    case MODE_ATTRIBUTE:
                        if(currentModeIndex == -1)
                        {
                            var currentMode = Shader.GetGlobalInt(propMode);
                            currentModeIndex = System.Array.FindIndex(ActiveModes, x => x.Index == currentMode);
                            if (currentModeIndex == -1)
                                currentModeIndex = 0;
                        }
                        Handles.BeginGUI();
                        EditorGUI.BeginChangeCheck();
                        currentModeIndex = EditorGUI.Popup(new Rect(0,0,120,16), currentModeIndex, ActiveModeNames);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Shader.SetGlobalInt(propMode, ActiveModes[currentModeIndex].Index);
                        }
                        Handles.EndGUI();
                        break;
                }
            };
        }

        private static void SetupSceneViews()
        {
            if(shaderAttributeViewer == null)
            {
                shaderAttributeViewer = Shader.Find("Hidden/_lil/AttributeViewer");
            }

            if (ActiveModes == null && shaderAttributeViewer != null)
            {
                var geometryAvailable = shaderAttributeViewer.passCount == 2;
                ActiveModes = geometryAvailable ? MODES : MODES.Where(x => !x.GeometryRequired).ToArray();

                ActiveModeNames = ActiveModes.Select(x => new GUIContent(x.Name)).ToArray();
            }

            foreach(SceneView view in SceneView.sceneViews)
            {
                view.onCameraModeChanged += mode =>
                {
                    if(mode.section != SECTION_NAME)
                    {
                        view.SetSceneViewShaderReplace(null, null);
                        return;
                    }
                    switch(mode.name)
                    {
                        case MODE_ATTRIBUTE:
                            view.SetSceneViewShaderReplace(shaderAttributeViewer, null);
                            break;
                    }
                };
            }
        }
    }
}
