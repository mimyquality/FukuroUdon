// Standard Lite GUI. Based on StandardShaderGUI in the built-in Unity shaders, licenced:
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace UnityEditor
{
#if UNITY_EDITOR
    internal class StandardLiteShaderGUI: ShaderGUI
    {
        public enum LightmapType
        {
            Default,
            MonoSH,
            MonoSHNoSpecular
        }

        private static class Styles
        {
            public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");

            public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
            public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Lightprobe Hack", "Use Lightprobes as lights for specularity");
            public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
            public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
            public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");
            public static GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
            public static GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
            public static GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static GUIContent lightmapTypeText = EditorGUIUtility.TrTextContent("Lightmap Type", "Lightmap Type");
            public static GUIContent specularAAText = EditorGUIUtility.TrTextContent("Specular Anti-Aliasing", "Screenspace Specular Anti-Aliasing");
            public static GUIContent specularAAVarianceText = EditorGUIUtility.TrTextContent("Variance", "Screenspace Specular Anti-Aliasing Variance");
            public static GUIContent specularAAThresholdText = EditorGUIUtility.TrTextContent("Threshold", "Screenspace Specular Anti-Aliasing Threshold");

            public static string primaryMapsText = "Main Maps";
            public static string secondaryMapsText = "Secondary Maps";
            public static string forwardText = "Forward Rendering Options";
            public static string renderingMode = "Rendering Mode";
            public static string advancedText = "Advanced Options";
        }

        MaterialProperty albedoMap = null;
        MaterialProperty albedoColor = null;
        MaterialProperty cutoff = null;
        MaterialProperty metallicMap = null;
        MaterialProperty metallic = null;
        MaterialProperty smoothness = null;
        MaterialProperty highlights = null;
        MaterialProperty reflections = null;
        MaterialProperty bumpScale = null;
        MaterialProperty bumpMap = null;
        MaterialProperty occlusionStrength = null;
        MaterialProperty occlusionMap = null;
        MaterialProperty emissionColorForRendering = null;
        MaterialProperty emissionMap = null;
        MaterialProperty detailMask = null;
        MaterialProperty detailAlbedoMap = null;
        MaterialProperty detailNormalMapScale = null;
        MaterialProperty detailNormalMap = null;
        MaterialProperty uvSetSecondary = null;
        MaterialProperty lightmapType = null;
        MaterialProperty specularAA = null;
        MaterialProperty specularAAVariance = null;
        MaterialProperty specularAAThreshold = null;

        MaterialEditor m_MaterialEditor;

        bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props)
        {
            albedoMap = FindProperty("_MainTex", props);
            albedoColor = FindProperty("_Color", props);

            try
            {
                cutoff = FindProperty("_Cutoff", props);
            }
            catch(Exception)
            {
                cutoff = null;
            }

            metallicMap = FindProperty("_MetallicGlossMap", props, false);
            metallic = FindProperty("_Metallic", props, false);
            smoothness = FindProperty("_Glossiness", props);
            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            detailMask = FindProperty("_DetailMask", props);
            detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
            detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
            detailNormalMap = FindProperty("_DetailNormalMap", props);
            uvSetSecondary = FindProperty("_UVSec", props);
            lightmapType = FindProperty("_LightmapType", props, false);
            specularAA = FindProperty("_EnableGeometricSpecularAA", props, true);
            specularAAVariance = FindProperty("_SpecularAAScreenSpaceVariance", props);
            specularAAThreshold = FindProperty("_SpecularAAThreshold", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                // Primary properties
                GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
                DoAlbedoArea(material);
                DoSpecularMetallicArea();
                DoNormalArea();
                m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
                if (occlusionMap.textureValue != null &&
                    metallicMap.textureValue &&
                    occlusionMap.textureValue != metallicMap.textureValue)
                {
                    EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("It's highly reccomended to pack your Occlusion(G) into your Metallic(R) Smoothness(A) map. You can then drop it in both texture slots."));
                }
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
                if ((detailMask.textureValue != null && albedoMap.textureValue && detailMask.textureValue != albedoMap.textureValue) ||
                    (detailMask.textureValue != null && emissionMap.textureValue && detailMask.textureValue != emissionMap.textureValue) ||
                    (detailMask.textureValue != null && detailAlbedoMap.textureValue && detailMask.textureValue != detailAlbedoMap.textureValue) ||
                    (detailMask.textureValue != null && occlusionMap.textureValue && detailMask.textureValue != occlusionMap.textureValue) ||
                    (detailMask.textureValue != null && metallicMap.textureValue && detailMask.textureValue != metallicMap.textureValue))
                {
                    EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("It's highly reccomended to pack your Detail Mask(A) into your Albedo(RGB) or Emission(RGB) maps. You can then drop it in both texture slots."));
                }
                DoEmissionArea(material);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                if (EditorGUI.EndChangeCheck())
                    emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake

                EditorGUILayout.Space();

                // Secondary properties
                GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
                m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
                m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

                // Third properties
                GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
                if (highlights != null)
                    m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                if (reflections != null)
                    m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);

                if (lightmapType != null)
                {
                    m_MaterialEditor.ShaderProperty(lightmapType, Styles.lightmapTypeText);

                    bool hasBakery = true;
                    bool bakeryUsingMonoSH = false;
                    try {
                        // Conditionally retrieve info from Bakery assembly, using reflection. Reflection is always cursed, it's fine.
                        Assembly bakeryAsm = Assembly.Load("BakeryEditorAssembly");
                        Type RenderDirModeType = bakeryAsm.GetType("ftRenderLightmap+RenderDirMode");
                        FieldInfo renderDirModeField = bakeryAsm.GetType("ftRenderLightmap").GetField("renderDirMode");
                        object renderDirMode = renderDirModeField.GetValue(null);
                        object monoshMode = Enum.Parse(RenderDirModeType, "MonoSH");

                        bakeryUsingMonoSH = renderDirMode.Equals(monoshMode);
                    } 
                    catch (BadImageFormatException) {
                        hasBakery = false;
                    }
                    catch (FileNotFoundException) {
                        hasBakery = false;
                    }
                    // catch (Exception e) 
                    // {
                    //     Debug.LogError("Failed to get MonoSH mode from Bakery: " + e);
                    // }

                    if (GetLightmapType(material) == LightmapType.MonoSH ||
                            GetLightmapType(material) == LightmapType.MonoSHNoSpecular)
                    {
                        if (!hasBakery) {
                            EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("MonoSH type mode selected, but Bakery does not appear to be installed."));
                        } else if (!bakeryUsingMonoSH)
                        {
                            EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("MonoSH type mode selected, but Bakery is not set to use MonoSH."));
                        }
                    } else {
                        if (hasBakery && bakeryUsingMonoSH)
                        {
                            EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("Default lightmap mode selected, but Bakery is set to use MonoSH."));
                        }
                    }
                }

                if (specularAA != null)
                {
                    m_MaterialEditor.ShaderProperty(specularAA, Styles.specularAAText);
                    if((int)material.GetFloat("_EnableGeometricSpecularAA") > 0)
                    {
                        int indentation = 2; // align with labels of texture properties
                        if (specularAAVariance != null)
                            m_MaterialEditor.ShaderProperty(specularAAVariance, Styles.specularAAVarianceText, indentation);
                        if (specularAAThreshold != null)
                            m_MaterialEditor.ShaderProperty(specularAAThreshold, Styles.specularAAThresholdText, indentation);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }

            EditorGUILayout.Space();

            // NB renderqueue editor is not shown on purpose: we want to override it based on blend mode
            GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
            m_MaterialEditor.EnableInstancingField();
            m_MaterialEditor.DoubleSidedGIField();
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            MaterialChanged(material);
        }

        void DoNormalArea()
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
            //if (bumpScale.floatValue != 1
                //&& BuildTargetDiscovery.PlatformHasFlag(EditorUserBuildSettings.activeBuildTarget, TargetAttributes.HasIntegratedGPU))
                //if (m_MaterialEditor.HelpBoxWithButton(
                    //EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
                    //EditorGUIUtility.TrTextContent("Fix Now")))
                //{
                    //bumpScale.floatValue = 1;
                //}
        }

        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);

            int indentation = 2;
            if(cutoff != null)
            {
                m_MaterialEditor.ShaderProperty(cutoff, Styles.alphaCutoffText.text, indentation);
            }
        }

        void DoEmissionArea(Material material)
        {
            // Emission for GI?
            if (m_MaterialEditor.EmissionEnabledProperty())
            {
                bool hadEmissionTexture = emissionMap.textureValue != null;

                // Texture and HDR color controls
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering.colorValue.maxColorComponent;
                if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering.colorValue = Color.white;

                // change the GI flag and fix it up with emissive as black if necessary
                m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
            }
        }

        void DoSpecularMetallicArea()
        {
            bool hasGlossMap = false;
            hasGlossMap = metallicMap.textureValue != null;
            m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, metallic);

            int indentation = 2; // align with labels of texture properties
            m_MaterialEditor.ShaderProperty(smoothness, Styles.smoothnessText, indentation);

            ++indentation;
        }

        static LightmapType GetLightmapType(Material material)
        {
            int ch = (int)material.GetFloat("_LightmapType");
            return (LightmapType)(int)ch;
        }

        static void SetMaterialKeywords(Material material)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
            SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
            SetKeyword(material, "_DETAIL", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            if (material.HasProperty("_LightmapType"))
            {
                SetKeyword(material, "_MONOSH_SPECULAR", GetLightmapType(material) == LightmapType.MonoSH);
                SetKeyword(material, "_MONOSH_NOSPECULAR", GetLightmapType(material) == LightmapType.MonoSHNoSpecular);
            }

            if (material.HasProperty("_EnableGeometricSpecularAA"))
            {
                SetKeyword(material, "_ENABLE_GEOMETRIC_SPECULAR_AA", material.GetFloat("_EnableGeometricSpecularAA") > 0);
            }
        }

        static void MaterialChanged(Material material)
        {
            SetMaterialKeywords(material);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
#else // if !UNITY_EDITOR
    internal class StandardLiteShaderGUI {}
#endif // if UNITY_EDITOR
} // namespace UnityEditor
