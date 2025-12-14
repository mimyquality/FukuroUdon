using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Shader keyword check tool",
        "A tool for shader developers. You can check shader keywords for each shader stage."
    )]
    [DocsHowTo("The shader keywords for <u>the shader selected in the Project window</u> are displayed for each pass and shader stage. This can be used to prevent unnecessary shader keywords from increasing the build size, or to identify cases where a missing shader keyword causes the shader to behave unexpectedly.")]
    [DocsMenuLocation(Common.MENU_HEAD + "Shader Keyword Viewer")]
    internal class ShaderKeywordViewer : EditorWindow
    {
        private static string[] KEYWORDS_BUILTIN;
        public List<bool> expands = new();
        public bool showBuiltin = false;
        public Vector2 scrollPos;
        private List<PassData> passDatas = new();
        private Shader shader;

        [MenuItem(Common.MENU_HEAD + "Shader Keyword Viewer")]
        static void Init() => GetWindow(typeof(ShaderKeywordViewer)).Show();

        void OnGUI()
        {
            if(Selection.activeObject is not Shader && Selection.activeObject is not Material) return;

            if(KEYWORDS_BUILTIN == null) InitializeBuiltinKeywords();

            if(showBuiltin != (showBuiltin = L10n.ToggleLeft("Show builtin keywords", showBuiltin)))
                shader = null;

            //EditorGUILayout.TextArea(EditorJsonUtility.ToJson(Selection.activeObject, true));
            Shader newShader;
            if(Selection.activeObject is Shader s) newShader = s;
            else newShader = (Selection.activeObject as Material).shader;

            if((shader != newShader || passDatas.Count == 0) && newShader)
            {
                shader = newShader;
                passDatas.Clear();
                expands.Clear();
                var serializedObject = new SerializedObject(shader);
                var passes = GetArrayProperties(serializedObject.FindProperty("m_CompileInfo.m_Snippets")).ToArray();
                var names = GetArrayProperties(serializedObject.FindProperty("m_ParsedForm.m_SubShaders")).SelectMany(p => GetArrayProperties(p.FindPropertyRelative("m_Passes"))).Select(p => (p.FindPropertyRelative("m_State.gpuProgramID").intValue, p.FindPropertyRelative("m_State.m_Name").stringValue + p.FindPropertyRelative("m_Name").stringValue + p.FindPropertyRelative("m_UseName").stringValue)).Where(kv => kv.Item1 != -1).ToDictionary(kv => kv.Item1, kv => kv.Item2);
                int i = 0;
                foreach(var pass in passes)
                {
                    var first = pass.FindPropertyRelative("first").intValue;
                    var second = pass.FindPropertyRelative("second");
                    var m_VariantsUserGlobal0 = GetKeywords(second, "m_VariantsUserGlobal0");
                    var m_VariantsUserGlobal1 = GetKeywords(second, "m_VariantsUserGlobal1");
                    var m_VariantsUserGlobal2 = GetKeywords(second, "m_VariantsUserGlobal2");
                    var m_VariantsUserGlobal3 = GetKeywords(second, "m_VariantsUserGlobal3");
                    var m_VariantsUserGlobal4 = GetKeywords(second, "m_VariantsUserGlobal4");

                    var m_VariantsUserLocal0 = GetKeywords(second, "m_VariantsUserLocal0");
                    var m_VariantsUserLocal1 = GetKeywords(second, "m_VariantsUserLocal1");
                    var m_VariantsUserLocal2 = GetKeywords(second, "m_VariantsUserLocal2");
                    var m_VariantsUserLocal3 = GetKeywords(second, "m_VariantsUserLocal3");
                    var m_VariantsUserLocal4 = GetKeywords(second, "m_VariantsUserLocal4");

                    var m_VariantsBuiltin0 = GetKeywords(second, "m_VariantsBuiltin0");
                    var m_VariantsBuiltin1 = GetKeywords(second, "m_VariantsBuiltin1");
                    var m_VariantsBuiltin2 = GetKeywords(second, "m_VariantsBuiltin2");
                    var m_VariantsBuiltin3 = GetKeywords(second, "m_VariantsBuiltin3");
                    var m_VariantsBuiltin4 = GetKeywords(second, "m_VariantsBuiltin4");

                    // multi_compile
                    var m_NonStrippedUserKeywords = second.FindPropertyRelative("m_NonStrippedUserKeywords").stringValue.Split(' ').Where(k => !string.IsNullOrEmpty(k)).Where(k => showBuiltin || !KEYWORDS_BUILTIN.Contains(k)).ToArray();

                    var passData = new PassData(){
                        name = names.ContainsKey(first) && !string.IsNullOrEmpty(names[first]) ? names[first] : $"Pass {i}: {second.FindPropertyRelative("m_AssetPath").stringValue}",
                        index = i,
                        stageDatas = new(),
                        keywordsMultiCompile = string.Join("\r\n", m_NonStrippedUserKeywords)
                    };
                    void AddStage(string[] local, string[] global, string[] builtin, ShaderStage shaderStage)
                    {
                        if(local.Length != 0 || global.Length != 0 || builtin.Length != 0)
                            passData.stageDatas.Add(new(){
                                stage = shaderStage,
                                keywordsBuiltin = string.Join("\r\n", builtin),
                                keywordsGlobal = string.Join("\r\n", global),
                                keywordsLocal = string.Join("\r\n", local)}
                            );
                    }
                    AddStage(m_VariantsUserLocal0, m_VariantsUserGlobal0, m_VariantsBuiltin0, ShaderStage.Vertex);
                    AddStage(m_VariantsUserLocal1, m_VariantsUserGlobal1, m_VariantsBuiltin1, ShaderStage.Fragment);
                    AddStage(m_VariantsUserLocal2, m_VariantsUserGlobal2, m_VariantsBuiltin2, ShaderStage.Hull);
                    AddStage(m_VariantsUserLocal3, m_VariantsUserGlobal3, m_VariantsBuiltin3, ShaderStage.Domain);
                    AddStage(m_VariantsUserLocal4, m_VariantsUserGlobal4, m_VariantsBuiltin4, ShaderStage.Geometry);
                    passDatas.Add(passData);
                    expands.Add(true);
                    i++;
                }
            }

            EditorGUILayout.LabelField(shader.name);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach(var passData in passDatas)
            {
                if(!(expands[passData.index] = EditorGUILayout.Foldout(expands[passData.index], passData.name))) continue;
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach(var stageData in passData.stageDatas) DrawStage(stageData);
                DrawKeywords("# Multi Compile", passData.keywordsMultiCompile);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndScrollView();
        }

        private static void InitializeBuiltinKeywords()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath("dd3ca69b755a7ac4697d46b5fc433619"));
            var so = new SerializedObject(shader);
            KEYWORDS_BUILTIN = GetArrayProperties(so.FindProperty("m_CompileInfo").FindPropertyRelative("m_Snippets")).SelectMany(p => {
                var second = p.FindPropertyRelative("second");
                return GetAllKeywords(second, "m_VariantsUserGlobal0")
                .Union(GetAllKeywords(second, "m_VariantsUserGlobal1"))
                .Union(GetAllKeywords(second, "m_VariantsUserGlobal2"))
                .Union(GetAllKeywords(second, "m_VariantsUserGlobal3"))
                .Union(GetAllKeywords(second, "m_VariantsUserGlobal4"))
                .Union(GetAllKeywords(second, "m_VariantsUserLocal0"))
                .Union(GetAllKeywords(second, "m_VariantsUserLocal1"))
                .Union(GetAllKeywords(second, "m_VariantsUserLocal2"))
                .Union(GetAllKeywords(second, "m_VariantsUserLocal3"))
                .Union(GetAllKeywords(second, "m_VariantsUserLocal4"))
                .Union(GetAllKeywords(second, "m_VariantsBuiltin0"))
                .Union(GetAllKeywords(second, "m_VariantsBuiltin1"))
                .Union(GetAllKeywords(second, "m_VariantsBuiltin2"))
                .Union(GetAllKeywords(second, "m_VariantsBuiltin3"))
                .Union(GetAllKeywords(second, "m_VariantsBuiltin4"))
                .Union(second.FindPropertyRelative("m_NonStrippedUserKeywords").stringValue.Split(' ').Where(k => !string.IsNullOrEmpty(k)));
            }).Distinct().ToArray();
        }

        private void DrawStage(StageData stageData)
        {
            EditorGUILayout.LabelField($"# {stageData.stage} Shader", EditorStyles.boldLabel);
            DrawKeywords(L10n.L("Global Keywords"), stageData.keywordsGlobal);
            DrawKeywords(L10n.L("Local Keywords"), stageData.keywordsLocal);
            if(showBuiltin) DrawKeywords(L10n.L("Builtin Keywords"), stageData.keywordsBuiltin);
            EditorGUILayout.Space();
        }

        private static void DrawKeywords(string label, string keywords)
        {
            if(keywords.Length == 0) return;
            EditorGUILayout.LabelField(label);
            EditorGUILayout.TextArea(keywords);
        }

        private string[] GetKeywords(SerializedProperty second, string propertyName)
        {
            return GetAllKeywords(second, propertyName).Where(k => k != "_" && k != "__").Where(k => showBuiltin || !KEYWORDS_BUILTIN.Contains(k)).ToArray();
        }

        private static IEnumerable<string> GetAllKeywords(SerializedProperty second, string propertyName)
        {
            return GetArrayProperties(second.FindPropertyRelative(propertyName))
                .SelectMany(p => GetArrayProperties(p).Select(p2 => p2.stringValue)).Distinct();
        }

        private static IEnumerable<SerializedProperty> GetArrayProperties(SerializedProperty prop)
        {
            return Enumerable.Range(0, prop.arraySize).Select(i => prop.GetArrayElementAtIndex(i));
        }

        internal class PassData
        {
            public string name;
            public int index;
            public List<StageData> stageDatas;
            public string keywordsMultiCompile;
        }

        internal class StageData
        {
            public ShaderStage stage;
            public string keywordsLocal;
            public string keywordsGlobal;
            public string keywordsBuiltin;
        }

        internal enum ShaderStage
        {
            Vertex,
            Fragment,
            Hull,
            Domain,
            Geometry
        }
    }
}
