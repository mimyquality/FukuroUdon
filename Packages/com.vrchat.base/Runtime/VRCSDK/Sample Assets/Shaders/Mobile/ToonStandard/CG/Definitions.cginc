// Samplers
sampler2D   _MainTex,
#if defined(USE_EMISSION_MAP)
            _EmissionMap,
#endif
#if defined(USE_OCCLUSION_MAP)
            _OcclusionMap,
#endif
#if defined(USE_NORMAL_MAPS)
            _BumpMap,
#endif
#if defined(USE_SPECULAR)
            _MetallicMap,
            _GlossMap,
#endif
#if defined(USE_DETAIL_MAPS)
            _DetailAlbedoMap,
            _DetailMask,
    #if defined(USE_NORMAL_MAPS)
            _DetailNormalMap,
    #endif
#endif
#if defined(USE_MATCAP)
            _Matcap,
            _MatcapMask,
#endif
            _HueShiftMask,
            _Ramp;

// Properties
VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _MainTex_ST);
VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _Ramp_ST);

#if defined(USE_EMISSION_MAP)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _EmissionMap_ST);
#endif

#if defined(USE_OCCLUSION_MAP)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _OcclusionMap_ST);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _OcclusionMapChannel);
#endif

#if defined(USE_NORMAL_MAPS)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _BumpMap_ST);
#endif

#if defined(USE_SPECULAR)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _MetallicMap_ST);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _MetallicMapChannel);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _GlossMap_ST);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _GlossMapChannel);
#endif

#if defined(USE_DETAIL_MAPS)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _DetailMask_ST);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _DetailMaskChannel);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _DetailAlbedoMap_ST);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _DetailMode);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _DetailUV);
    #if defined(USE_NORMAL_MAPS)
        VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _DetailNormalMap_ST);
    #endif
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _DetailHueShift);
#endif

#if defined(USE_MATCAP)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _MatcapMask_ST);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _MatcapMaskChannel);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _MatcapType);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _MatcapStrength);
#endif

VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _HueShiftMask_ST);
VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _HueShiftMaskChannel);

VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _Color);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _BumpScale);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _VertexColor);

VRCHAT_DEFINE_ATLAS_PROPERTY(half, _ShadowBoost);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _ShadowAlbedo);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _MinBrightness);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _LimitBrightness);

VRCHAT_DEFINE_ATLAS_PROPERTY(half, _HueShift);

VRCHAT_DEFINE_ATLAS_PROPERTY(half3, _RimColor);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _RimAlbedoTint);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _RimIntensity);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _RimRange);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _RimSharpness);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _RimEnvironmental);

//VRCHAT_DEFINE_ATLAS_PROPERTY(half, _Cutoff);
//VRCHAT_DEFINE_ATLAS_PROPERTY(half, _AlphaToMask);

#if defined(USE_EMISSION_MAP)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _EmissionColor);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _EmissionStrength);
    VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _EmissionUV);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _EmissionHueShift);
#endif

#if defined(USE_OCCLUSION_MAP)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _OcclusionStrength);
#endif

#if defined(USE_SPECULAR)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _MetallicStrength);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _GlossStrength);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _SpecularSharpness);
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _Reflectance);
#endif

#if defined(USE_DETAIL_MAPS)
    VRCHAT_DEFINE_ATLAS_PROPERTY(half, _DetailNormalMapScale);
#endif

// Atlas Texture Modes
#if defined(VRCHAT_ATLASING_ENABLED)
    VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_MainTex);
    VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_Ramp);

    #if defined(USE_EMISSION_MAP)
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_EmissionMap);
    #endif

    #if defined(USE_OCCLUSION_MAP)
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_OcclusionMap);
    #endif

    #if defined(USE_NORMAL_MAPS)
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_BumpMap);
    #endif

    #if defined(USE_SPECULAR)
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_MetallicMap);
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_GlossMap);
    #endif

    #if defined(USE_DETAIL_MAPS)
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_DetailAlbedoMap);
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_DetailMask);
        #if defined(USE_NORMAL_MAPS)
            VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_DetailNormalMap);
        #endif
    #endif

    #if defined(USE_MATCAP)
        VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_MatcapMask);
    #endif

    VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_HueShiftMask);
#endif