using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRC.ToonStandard
{
    public partial class Foldouts
    {
        public bool main = true;
        public bool lighting = true;
        public bool specular = true;
        public bool rimlighting = true;
        public bool emission = true;
        public bool occlusion = true;
        public bool detail = true;
        public bool matcap = true;
        public bool outline = true;
        public bool hueShift = true;

        public bool mainTex = false;
        public bool hueShiftMaskTex = false;
        public bool normalTex = false;
        public bool rampTex = false;
        public bool specularTex = false;
        public bool metallicTex = false;
        public bool matcapMaskTex = false;
        public bool emissionTex = false;
        public bool detailMaskTex = false;
        public bool detailAlbedoTex = false;
        public bool detailNormalTex = false;
        public bool occlusionTex = false;
        public bool outlineTex = false;
    }

    public enum BlendModes
    {
        Opaque,
        //Cutout,
        //CutoutPlus, // Alpha To Coverage
        //Transparent,
        //Fade,
        //Additive
    }

#if UNITY_EDITOR

    public class ToonStandardShaderEditor : ShaderGUI
    {
        private const int MARGIN_TOP = 6;
        private const int MARGIN_BOTTOM = 10;

        protected static Dictionary<Material, Foldouts> Foldouts = new Dictionary<Material, Foldouts>();
        protected BindingFlags bindingFlags = BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance |
                                              BindingFlags.Static;

        //protected MaterialProperty _BlendMode;
        protected MaterialProperty _Culling;

        protected MaterialProperty _MainTex;
        protected MaterialProperty _Color;
        //protected MaterialProperty _Cutoff;
        protected MaterialProperty _VertexColor;

        protected MaterialProperty _Ramp;
        protected MaterialProperty _ShadowBoost;
        protected MaterialProperty _ShadowAlbedo;
        protected MaterialProperty _MinBrightness;
        protected MaterialProperty _LimitBrightness;

        protected MaterialProperty _BumpMap;
        protected MaterialProperty _BumpScale;

        protected MaterialProperty _MetallicMap;
        protected MaterialProperty _MetallicMapChannel;
        protected MaterialProperty _MetallicStrength;
        protected MaterialProperty _GlossMap;
        protected MaterialProperty _GlossMapChannel;
        protected MaterialProperty _GlossStrength;
        protected MaterialProperty _Reflectance;
        protected MaterialProperty _SpecularSharpness;

        protected MaterialProperty _Matcap;
        protected MaterialProperty _MatcapMask;
        protected MaterialProperty _MatcapMaskChannel;
        protected MaterialProperty _MatcapType;
        protected MaterialProperty _MatcapStrength;

        protected MaterialProperty _RimColor;
        protected MaterialProperty _RimIntensity;
        protected MaterialProperty _RimRange;
        protected MaterialProperty _RimSharpness;
        protected MaterialProperty _RimEnvironmental;
        protected MaterialProperty _RimAlbedoTint;

        protected MaterialProperty _EmissionMap;
        protected MaterialProperty _EmissionColor;
        protected MaterialProperty _EmissionStrength;
        protected MaterialProperty _EmissionUV;

        protected MaterialProperty _OcclusionMap;
        protected MaterialProperty _OcclusionMapChannel;
        protected MaterialProperty _OcclusionStrength;

        protected MaterialProperty _DetailMode;
        protected MaterialProperty _DetailMask;
        protected MaterialProperty _DetailMaskChannel;
        protected MaterialProperty _DetailAlbedoMap;
        protected MaterialProperty _DetailNormalMap;
        protected MaterialProperty _DetailNormalMapScale;
        protected MaterialProperty _DetailUV;

        protected MaterialProperty _OutlineThickness;
        protected MaterialProperty _OutlineColor;
        protected MaterialProperty _OutlineFromAlbedo;
        protected MaterialProperty _OutlineMask;
        protected MaterialProperty _OutlineMaskChannel;

        protected MaterialProperty _HueShift;
        protected MaterialProperty _DetailHueShift;
        protected MaterialProperty _EmissionHueShift;
        protected MaterialProperty _HueShiftMask;
        protected MaterialProperty _HueShiftMaskChannel;

        protected LocalKeyword USE_SPECULAR;
        protected LocalKeyword USE_MATCAP;
        protected LocalKeyword USE_RIMLIGHT;
        protected LocalKeyword USE_DETAIL_MAPS;
        protected LocalKeyword USE_NORMAL_MAPS;
        protected LocalKeyword USE_OCCLUSION_MAP;
        protected LocalKeyword USE_HUE_SHIFT;

        private int BlendMode = 0;
        private ShadowRamp RampMode = 0;

        protected enum ShadowRamp
        {
            Flat,
            Realistic,
            RealisticSoft,
            RealisticVerySoft,
            Toon2Band,
            Toon3Band,
            Toon4Band,
            Custom
        }

        protected enum DetailMode
        {
            AlphaBlended = 0,
            Additive = 1,
            Multiply = 2,
            MultiplyX2 = 3,
        }

        protected enum UVMap
        {
            UV0 = 0,
            UV1 = 1,
        }

        protected enum MatcapType
        {
            Additive = 0,
            Multiplicative = 1,
        }

        Material material;
        MaterialEditor materialEditor;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            material = materialEditor.target as Material;
            this.materialEditor = materialEditor;

            var isAtlassed = material.IsKeywordEnabled("VRCHAT_ATLASING_ENABLED");
            if (isAtlassed)
            {
                EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("This material is using the Atlased variation. Drive properties with the VRCAvatarAtlasProperties component"));
                EditorGUI.BeginDisabledGroup(true);
            }

            LoadResources();
            LoadMaterialProperties(properties);

            //BlendMode = material.GetInt(_BlendMode.name);
            BlendMode = (int)BlendModes.Opaque;

            SetupFoldoutDictionary(material);

            EditorGUI.BeginChangeCheck();

            DrawMain(materialEditor, material);
            DrawLighting(materialEditor, material);
            if (material.shader.name.Contains("Outline"))
                DrawOutline(materialEditor, material);
            DrawEmission(materialEditor, material);
            DrawHueShift(materialEditor, material);
            DrawOcclusion(materialEditor, material);
            DrawDetail(materialEditor, material);
            DrawSpecular(materialEditor, material);
            DrawMatcap(materialEditor, material);
            DrawRimlighting(materialEditor, material);
            HandleBlendModes(materialEditor, material);

            EditorGUI.EndChangeCheck();

            if (isAtlassed)
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        bool initializedProperties = false;
        private void LoadMaterialProperties(MaterialProperty[] properties)
        {
            if(initializedProperties)
                return;
            initializedProperties = true;

            //Find all material properties listed in the script using reflection, and set them using a loop only if they're of type MaterialProperty.
            //This makes things a lot nicer to maintain and cleaner to look at.
            foreach(var property in GetType().GetFields(bindingFlags))
            {
                if(property.FieldType == typeof(MaterialProperty))
                {
                    property.SetValue(this, FindProperty(property.Name, properties, false));
                }
                else if(property.FieldType == typeof(LocalKeyword))
                {
                    property.SetValue(this, new LocalKeyword(material.shader, property.Name));
                }
            }

            Undo.undoRedoPerformed += RequirePropertyRefresh;
        }
        private void RequirePropertyRefresh()
        {
            initializedProperties = false;
            Undo.undoRedoPerformed -= RequirePropertyRefresh;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            initializedProperties = false;
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
        }

        private void SetupFoldoutDictionary(Material material)
        {
            if (Foldouts.ContainsKey(material))
                return;

            Foldouts toggles = new Foldouts();
            Foldouts.Add(material, toggles);
        }

        private void DrawMain(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.main, "Main", null, ResetMain);
            if(toggles.main)
            {
                GUILayout.Space(MARGIN_TOP);

                DrawTexture(materialEditor, _MainTex.displayName, _MainTex, _Color, ref toggles.mainTex);
                materialEditor.ShaderProperty(_VertexColor, _VertexColor.displayName);

                GUILayout.Space(8);

                Toggle(EditorGUIUtility.TrTextContent("Use Normal Map"), USE_NORMAL_MAPS);

                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_NORMAL_MAPS));
                {
                    DrawTexture(materialEditor, _BumpMap.displayName, _BumpMap, _BumpScale, ref toggles.normalTex);
                }
                EditorGUI.EndDisabledGroup();

                if (material.IsKeywordEnabled(USE_NORMAL_MAPS) && _BumpMap.textureValue == null && _DetailNormalMap.textureValue == null)
                {
                    EditorGUILayout.HelpBox("You should disable 'Use Normal Map' to save performance when not using a Normal Map or Detail Normal Map.", MessageType.Warning);
                }

                /*if ((BlendModes)BlendMode == BlendModes.Cutout)
                {
                    materialEditor.ShaderProperty(_Cutoff, _Cutoff.displayName);
                }*/

                GUILayout.Space(8);

                //materialEditor.ShaderProperty(_BlendMode, _BlendMode.displayName);
                materialEditor.ShaderProperty(_Culling, _Culling.displayName);

                GUILayout.Space(MARGIN_BOTTOM);
            }

            EndGroup();
        }
        void ResetMain()
        {
            //ResetToDefault(material, _BlendMode);
            ResetToDefault(material, USE_NORMAL_MAPS);
            ResetToDefault(material, _Culling);
            ResetToDefault(material, _MainTex);
            ResetToDefault(material, _Color);
            ResetToDefault(material, _BumpMap);
            ResetToDefault(material, _BumpScale);
        }

        private void DrawHueShift(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.hueShift, "Hue Shift", USE_HUE_SHIFT, ResetHueShift);
            if(toggles.hueShift)
            {
                GUILayout.Space(MARGIN_TOP);
                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_HUE_SHIFT));
                {
                    DrawTexture(materialEditor, _HueShiftMask.displayName, _HueShiftMask, null, ref toggles.hueShiftMaskTex, _HueShiftMaskChannel);
                    materialEditor.ShaderProperty(_HueShift, _HueShift.displayName);
                    materialEditor.ShaderProperty(_EmissionHueShift, _EmissionHueShift.displayName);
                    EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_DETAIL_MAPS));
                    {
                        materialEditor.ShaderProperty(_DetailHueShift, _DetailHueShift.displayName);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        void ResetHueShift()
        {
            ResetToDefault(material, _HueShift);
            ResetToDefault(material, _DetailHueShift);
            ResetToDefault(material, _EmissionHueShift);
            ResetToDefault(material, _HueShiftMask);
        }

        private void DrawLighting(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.lighting, "Lighting", null, ResetLighting);
            if(toggles.lighting)
            {
                GUILayout.Space(MARGIN_TOP);

                //Shadows
                if(_Ramp.textureValue == _RampFlat)
                    RampMode = ShadowRamp.Flat;
                else if(_Ramp.textureValue == _RampRealistic)
                    RampMode = ShadowRamp.Realistic;
                else if(_Ramp.textureValue == _RampRealisticSoft)
                    RampMode = ShadowRamp.RealisticSoft;
                else if(_Ramp.textureValue == _RampRealisticVerySoft)
                    RampMode = ShadowRamp.RealisticVerySoft;
                else if(_Ramp.textureValue == _RampToon2Band)
                    RampMode = ShadowRamp.Toon2Band;
                else if(_Ramp.textureValue == _RampToon3Band)
                    RampMode = ShadowRamp.Toon3Band;
                else if(_Ramp.textureValue == _RampToon4Band)
                    RampMode = ShadowRamp.Toon4Band;
                else
                    RampMode = ShadowRamp.Custom;

                EditorGUI.BeginChangeCheck();
                RampMode = (ShadowRamp)EditorGUILayout.EnumPopup(new GUIContent("Shading", "Select how realtime shadows and light probes will affect the look."), RampMode);
                var changed = EditorGUI.EndChangeCheck();
                if(changed && RampMode == ShadowRamp.Custom)
                    _Ramp.textureValue = null;
                if(RampMode == ShadowRamp.Custom)
                    DrawTexture(materialEditor, _Ramp.displayName, _Ramp, null, ref toggles.rampTex);

                if (changed)
                {
                    // if something was changed, update textures and reset scale/offset
                    // needs to be inside `changed` as otherwise we reset on multi-edit
                    switch(RampMode)
                    {
                        case ShadowRamp.Flat:
                            _Ramp.textureValue = _RampFlat;
                            break;
                        case ShadowRamp.Realistic:
                            _Ramp.textureValue = _RampRealistic;
                            break;
                        case ShadowRamp.RealisticSoft:
                            _Ramp.textureValue = _RampRealisticSoft;
                            break;
                        case ShadowRamp.RealisticVerySoft:
                            _Ramp.textureValue = _RampRealisticVerySoft;
                            break;
                        case ShadowRamp.Toon2Band:
                            _Ramp.textureValue = _RampToon2Band;
                            break;
                        case ShadowRamp.Toon3Band:
                            _Ramp.textureValue = _RampToon3Band;
                            break;
                        case ShadowRamp.Toon4Band:
                            _Ramp.textureValue = _RampToon4Band;
                            break;
                        case ShadowRamp.Custom:
                            break;
                    }

                    if (RampMode != ShadowRamp.Custom)
                        _Ramp.textureScaleAndOffset = new Vector4(1, 1, 0, 0);
                }

                GUILayout.Space(8);
                materialEditor.ShaderProperty(_ShadowBoost, _ShadowBoost.displayName);
                materialEditor.ShaderProperty(_ShadowAlbedo, _ShadowAlbedo.displayName);
                materialEditor.ShaderProperty(_MinBrightness, _MinBrightness.displayName);
                materialEditor.ShaderProperty(_LimitBrightness, new GUIContent(_LimitBrightness.displayName, "Limit the maximum brightness of light hitting the material to 1.0. Prevents blown out colors in misconfigured environments."));

                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        void ResetLighting()
        {
            ResetToDefault(material, _Ramp);
            ResetToDefault(material, _ShadowBoost);
            ResetToDefault(material, _ShadowAlbedo);
            ResetToDefault(material, _MinBrightness);
            ResetToDefault(material, _LimitBrightness);
            _Ramp.textureValue = _RampRealisticSoft;
        }

        private void DrawSpecular(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.specular, "Specular", USE_SPECULAR, ResetSpecular);
            if(toggles.specular)
            {
                GUILayout.Space(MARGIN_TOP);
                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_SPECULAR));
                {
                    DrawTexture(materialEditor, _MetallicMap.displayName, _MetallicMap, _MetallicStrength, ref toggles.metallicTex, _MetallicMapChannel);
                    DrawTexture(materialEditor, _GlossMap.displayName, _GlossMap, _GlossStrength, ref toggles.specularTex, _GlossMapChannel);

                    if (material.IsKeywordEnabled(USE_SPECULAR) && ShowPackedTextureRecommendation())
                    {
                        EditorGUILayout.HelpBox(
                            "Consider packing Metallic, Gloss, and Occlusion into a single texture and assign it to all three slots. This will save memory and improve performance.\n" +
                            "Metallic reads the Red channel, Gloss the Alpha channel, and Occlusion the Green channel.", MessageType.Warning);
                        GUILayout.Space(8);
                    }

                    materialEditor.ShaderProperty(_SpecularSharpness, _SpecularSharpness.displayName);
                    materialEditor.ShaderProperty(_Reflectance, new GUIContent(_Reflectance.displayName, "Affected by Gloss Map."));
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        void ResetSpecular()
        {
            ResetToDefault(material, USE_SPECULAR);
            ResetToDefault(material, _MetallicMap);
            ResetToDefault(material, _MetallicStrength);
            ResetToDefault(material, _GlossMap);
            ResetToDefault(material, _GlossStrength);
            ResetToDefault(material, _SpecularSharpness);
            ResetToDefault(material, _Reflectance);
        }
        bool ShowPackedTextureRecommendation()
        {
            var search = _OcclusionMap.textureValue;
            if (_MetallicMap.textureValue != null && search != null && _MetallicMap.textureValue != search)
                return true;
            else
                search = _MetallicMap.textureValue;
            if (_GlossMap.textureValue != null && search != null && _GlossMap.textureValue != search)
                return true;
            return false;
        }

        private void DrawMatcap(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.matcap, "Matcap", USE_MATCAP, ResetMatcap);
            if(toggles.matcap)
            {
                GUILayout.Space(MARGIN_TOP);
                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_MATCAP));
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent(_Matcap.displayName), _Matcap);
                    DrawTexture(materialEditor, _MatcapMask.displayName, _MatcapMask, _MatcapStrength, ref toggles.matcapMaskTex, _MatcapMaskChannel);
                    materialEditor.ShaderProperty(_MatcapType, _MatcapType.displayName);

                    if (material.IsKeywordEnabled(USE_MATCAP) && RampMode != ShadowRamp.Flat)
                    {
                        EditorGUILayout.HelpBox("Not using 'Flat' shading with a Matcap may give unexpected results.", MessageType.Warning);
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        void ResetMatcap()
        {
            ResetToDefault(material, USE_MATCAP);
            ResetToDefault(material, _Matcap);
            ResetToDefault(material, _MatcapMask);
            ResetToDefault(material, _MatcapType);
            ResetToDefault(material, _MatcapStrength);
        }

        private void DrawEmission(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.emission, "Emission", null, ResetEmission);
            if(toggles.emission)
            {
                GUILayout.Space(MARGIN_TOP);
                //EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_EMISSION_MAP));
                {
                    DrawTexture(materialEditor, _EmissionMap.displayName, _EmissionMap, _EmissionColor, ref toggles.emissionTex);
                    EditorGUI.BeginDisabledGroup(_EmissionMap.textureValue == null);
                    {
                        materialEditor.ShaderProperty(_EmissionUV, _EmissionUV.displayName);
                    }
                    EditorGUI.EndDisabledGroup();
                    materialEditor.ShaderProperty(_EmissionStrength, _EmissionStrength.displayName);
                }
                //EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        private void ResetEmission()
        {
            //ResetToDefault(material, USE_EMISSION_MAP);
            ResetToDefault(material, _EmissionMap);
            ResetToDefault(material, _EmissionColor);
            ResetToDefault(material, _EmissionStrength);
            ResetToDefault(material, _EmissionUV);
        }

        private void DrawDetail(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.detail, "Detail", USE_DETAIL_MAPS, ResetDetails);
            if(toggles.detail)
            {
                GUILayout.Space(MARGIN_TOP);
                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_DETAIL_MAPS));
                {
                    materialEditor.ShaderProperty(_DetailMode, _DetailMode.displayName);
                    DrawTexture(materialEditor, _DetailAlbedoMap.displayName, _DetailAlbedoMap, null, ref toggles.detailAlbedoTex);
                    DrawTexture(materialEditor, _DetailMask.displayName, _DetailMask, null, ref toggles.detailMaskTex, _DetailMaskChannel);

                    EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_NORMAL_MAPS));
                    {
                        DrawTexture(materialEditor, _DetailNormalMap.displayName, _DetailNormalMap, _DetailNormalMapScale, ref toggles.detailNormalTex);
                    }
                    EditorGUI.EndDisabledGroup();
                    if (!material.IsKeywordEnabled(USE_NORMAL_MAPS) && material.IsKeywordEnabled(USE_DETAIL_MAPS))
                    {
                        EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("You must turn on 'Use Normal Map' if you want to use a Detail Normal Map."));
                    }

                    materialEditor.ShaderProperty(_DetailUV, new GUIContent(_DetailUV.displayName, "UV channel to use for Detail Texture and Detail Normal Map. Detail Mask always uses UV0."));
                    if (_DetailUV.floatValue != 0f && _DetailMask.textureValue != null)
                    {
                        EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("Note: Detail Mask always uses UV0."));
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        void ResetDetails()
        {
            ResetToDefault(material, USE_DETAIL_MAPS);
            ResetToDefault(material, _DetailMode);
            ResetToDefault(material, _DetailMask);
            ResetToDefault(material, _DetailAlbedoMap);
            ResetToDefault(material, _DetailNormalMap);
            ResetToDefault(material, _DetailNormalMapScale);
            ResetToDefault(material, _DetailUV);
        }

        private void DrawOcclusion(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.occlusion, "Occlusion", USE_OCCLUSION_MAP, ResetOcclusion);
            if(toggles.occlusion)
            {
                GUILayout.Space(MARGIN_TOP);
                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_OCCLUSION_MAP));
                {
                    DrawTexture(materialEditor, _OcclusionMap.displayName, _OcclusionMap, _OcclusionStrength, ref toggles.occlusionTex, _OcclusionMapChannel);
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        private void ResetOcclusion()
        {
            ResetToDefault(material, USE_OCCLUSION_MAP);
            ResetToDefault(material, _OcclusionMap);
            ResetToDefault(material, _OcclusionStrength);
        }

        private void DrawRimlighting(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.rimlighting, "Rim Lighting", USE_RIMLIGHT, ResetRimlighting);
            if(toggles.rimlighting)
            {
                GUILayout.Space(MARGIN_TOP);
                EditorGUI.BeginDisabledGroup(!material.IsKeywordEnabled(USE_RIMLIGHT));
                {
                    materialEditor.ShaderProperty(_RimColor, _RimColor.displayName);
                    materialEditor.ShaderProperty(_RimAlbedoTint, new GUIContent(_RimAlbedoTint.displayName));
                    materialEditor.ShaderProperty(_RimIntensity, _RimIntensity.displayName);
                    materialEditor.ShaderProperty(_RimRange, _RimRange.displayName);
                    materialEditor.ShaderProperty(_RimSharpness, _RimSharpness.displayName);
                    materialEditor.ShaderProperty(_RimEnvironmental, new GUIContent(_RimEnvironmental.displayName, "Scale the rim intensity based on environment lighting."));
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        private void ResetRimlighting()
        {
            ResetToDefault(material, USE_RIMLIGHT);
            ResetToDefault(material, _RimColor);
            ResetToDefault(material, _RimIntensity);
            ResetToDefault(material, _RimRange);
            ResetToDefault(material, _RimSharpness);
            ResetToDefault(material, _RimEnvironmental);
            ResetToDefault(material, _RimAlbedoTint);
        }
        
        private void DrawOutline(MaterialEditor materialEditor, Material material)
        {
            Foldouts toggles = Foldouts[material];
            BeginGroup(ref toggles.outline, "Outline", null, ResetOutline);
            if(toggles.outline)
            {
                GUILayout.Space(MARGIN_TOP);

                EditorGUI.indentLevel -= 1;
                EditorGUILayout.HelpBox("The \"Outline\" variant of Toon Standard is only supported on PC. You can upload a mobile avatar using it, but outlines won't show.", MessageType.Warning);
                GUILayout.Space(4);
                EditorGUI.indentLevel += 1;

                DrawTexture(materialEditor, _OutlineMask.displayName, _OutlineMask, _OutlineThickness, ref toggles.outlineTex, _OutlineMaskChannel);
                materialEditor.ShaderProperty(_OutlineColor, _OutlineColor.displayName);
                materialEditor.ShaderProperty(_OutlineFromAlbedo, new GUIContent(_OutlineFromAlbedo.displayName, "Use the Albedo color for the outline color."));
                GUILayout.Space(MARGIN_BOTTOM);
            }
            EndGroup();
        }
        private void ResetOutline()
        {
            ResetToDefault(material, _OutlineMask);
            ResetToDefault(material, _OutlineThickness);
            ResetToDefault(material, _OutlineColor);
            ResetToDefault(material, _OutlineFromAlbedo);
        }

        private void HandleBlendModes(MaterialEditor materialEditor, Material material)
        {
            switch ((BlendModes)BlendMode)
            {
                case BlendModes.Opaque:
                    {
                        SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One,
                            (int)UnityEngine.Rendering.BlendMode.Zero,
                            (int)UnityEngine.Rendering.RenderQueue.Geometry, 1, 0);
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    }

                /*case BlendModes.Cutout:
                {
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One,
                        (int)UnityEngine.Rendering.BlendMode.Zero,
                        (int)UnityEngine.Rendering.RenderQueue.AlphaTest, 1, 0);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                }
                
                case BlendModes.CutoutPlus:
                {
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One,
                        (int)UnityEngine.Rendering.BlendMode.Zero,
                        (int)UnityEngine.Rendering.RenderQueue.AlphaTest, 1, 1);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                }

                case BlendModes.Transparent:
                {
                    SetBlend(material, (int) UnityEngine.Rendering.BlendMode.One,
                        (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                        (int) UnityEngine.Rendering.RenderQueue.Transparent, 0, 0);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                }
                
                case BlendModes.Fade:
                {
                    SetBlend(material, (int) UnityEngine.Rendering.BlendMode.SrcAlpha,
                        (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                        (int) UnityEngine.Rendering.RenderQueue.Transparent, 0, 0);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                }*/

                default:
                {
                    goto case BlendModes.Opaque;
                }
            }
        }
        private void SetBlend(Material material, int src, int dst, int renderQueue, int zwrite, int alphatocoverage)
        {
            material.SetInt("_SrcBlend", src);
            material.SetInt("_DstBlend", dst);
            material.SetInt("_ZWrite", zwrite);
            material.SetInt("_AlphaToMask", alphatocoverage);
            material.renderQueue = renderQueue;
        }

        #region Helpers
        void Toggle(GUIContent label, LocalKeyword keyword)
        {
            EditorGUI.BeginChangeCheck();
            bool value = material.IsKeywordEnabled(keyword);
            value = EditorGUILayout.Toggle(label, value, GUILayout.Width(16));
            if(EditorGUI.EndChangeCheck())
            {
                foreach (var matobj in materialEditor.targets)
                {
                    if (matobj is Material material)
                    {
                        if(value)
                            material.EnableKeyword(keyword);
                        else
                            material.DisableKeyword(keyword);
                    }
                }
            }
        }
        void BeginGroup(ref bool foldout, string title, LocalKeyword? keyword, System.Action onReset = null)
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.Height(24));
            EditorGUILayout.Space(1);
            EditorGUILayout.BeginHorizontal();

            //Color
            if(Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.x - 16f, rect.y, rect.width + 16, rect.height), new Color(1, 1, 1, 0.1f));

            foldout = EditorGUI.Foldout(rect, foldout, GUIContent.none);

            if(keyword != null)
            {
                Toggle(GUIContent.none, keyword.Value);
            }

            EditorGUILayout.LabelField(title);

            if(onReset != null)
            {
                if(GUILayout.Button("Reset", GUILayout.Width(64)))
                {
                    onReset();
                }
            }

            if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                foldout = !foldout;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        static void EndGroup()
        {
        }
        void ResetToDefault(Material material, MaterialProperty property)
        {
            var index = material.shader.FindPropertyIndex(property.name);
            switch(property.type)
            {
                case MaterialProperty.PropType.Int:
                    property.intValue = material.shader.GetPropertyDefaultIntValue(index);
                    break;
                case MaterialProperty.PropType.Range:
                case MaterialProperty.PropType.Float:
                    property.floatValue = material.shader.GetPropertyDefaultFloatValue(index);
                    break;
                case MaterialProperty.PropType.Color:
                    property.colorValue = material.shader.GetPropertyDefaultVectorValue(index);
                    break;
                case MaterialProperty.PropType.Vector:
                    property.vectorValue = material.shader.GetPropertyDefaultVectorValue(index);
                    break;
                case MaterialProperty.PropType.Texture:
                    property.textureValue = null;
                    property.textureScaleAndOffset = new Vector4(1, 1, 0, 0);
                    break;
            }
        }
        void ResetToDefault(Material _, LocalKeyword property)
        {
            foreach (var matobj in materialEditor.targets)
            {
                if (matobj is Material material)
                    material.SetKeyword(property, false);
            }
        }
        static readonly string[] colorChannelNames = { "R", "G", "B", "A" };
        static void DrawTexture(MaterialEditor materialEditor, string label, MaterialProperty property, MaterialProperty secondaryProperty, ref bool foldout, MaterialProperty colorChannelProperty = null)
        {
            var rect = EditorGUILayout.BeginVertical();
            foldout = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), foldout, GUIContent.none);
            if (colorChannelProperty != null)
            {
                label += " (" + colorChannelNames[(int)colorChannelProperty.floatValue % colorChannelNames.Length] + ")";
            }
            materialEditor.TexturePropertySingleLine(new GUIContent(label), property, secondaryProperty);
            EditorGUILayout.EndVertical();
            if(foldout)
            {
                EditorGUI.indentLevel += 1;
                materialEditor.TextureScaleOffsetProperty(property);
                if (colorChannelProperty != null)
                {
                    materialEditor.ShaderProperty(colorChannelProperty, new GUIContent(colorChannelProperty.displayName));
                }
                EditorGUI.indentLevel -= 1;
                GUILayout.Space(8);
            }
        }
        #endregion

        bool resourcesLoaded = false;
        Texture2D _RampFlat;
        Texture2D _RampRealistic;
        Texture2D _RampRealisticSoft;
        Texture2D _RampRealisticVerySoft;
        Texture2D _RampToon2Band;
        Texture2D _RampToon3Band;
        Texture2D _RampToon4Band;
        private void LoadResources()
        {
            if(resourcesLoaded)
                return;
            resourcesLoaded = true;

            //Load
            _RampFlat = Resources.Load<Texture2D>("VRChat/ShadowRampFlat");
            _RampRealistic = Resources.Load<Texture2D>("VRChat/ShadowRampRealistic");
            _RampRealisticSoft = Resources.Load<Texture2D>("VRChat/ShadowRampRealisticSoft");
            _RampRealisticVerySoft = Resources.Load<Texture2D>("VRChat/ShadowRampRealisticVerySoft");
            _RampToon2Band = Resources.Load<Texture2D>("VRChat/ShadowRampToon2Band");
            _RampToon3Band = Resources.Load<Texture2D>("VRChat/ShadowRampToon3Band");
            _RampToon4Band = Resources.Load<Texture2D>("VRChat/ShadowRampToon4Band");
        }
    }
#endif
}
