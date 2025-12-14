v2f vert (appdata v)
{
    v2f o = (v2f)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VRCHAT_ATLAS_INITIALIZE_VERTEX_OUTPUT(v, o);
    VRCHAT_SETUP_ATLAS_INDEX_POST_VERTEX(o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv.xy = v.uv;
    o.uv.zw = v.uv1;

    UNITY_BRANCH if (VRCHAT_GET_ATLAS_PROPERTY(_VertexColor))
        o.color = v.color;
    else
        o.color = half4(1, 1, 1, 1);

    #if !defined(UNITY_PASS_SHADOWCASTER)
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
        half3 wnormal = UnityObjectToWorldNormal(v.normal);
        half3 tangent = UnityObjectToWorldDir(v.tangent.xyz);
        o.normal = wnormal;
        o.tangent.xyz = tangent;
        o.tangent.w = v.tangent.w * unity_WorldTransformParams.w;
        
        UNITY_TRANSFER_SHADOW(o, o.uv);
        UNITY_TRANSFER_FOG(o, o.pos);
    #else
        TRANSFER_SHADOW_CASTER_NOPOS(o, o.pos);
    #endif

    return o;
}

half4 frag (v2f i, uint facing : SV_IsFrontFace) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    VRCHAT_SETUP_ATLAS_INDEX_POST_VERTEX(i);

    Surface surface = (Surface)0;

    float2 uv0 = i.uv.xy;

    //surface.cutoff = material.cutoff;

    surface.albedoMap = tex2D(_MainTex, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(uv0, _MainTex))
        * VRCHAT_GET_ATLAS_PROPERTY(_Color)
        * half4(i.color, 1);

    #if !defined(UNITY_PASS_SHADOWCASTER)
        half hueShiftMask = 0;
        UNITY_BRANCH if (USE_HUE_SHIFT)
        {
            hueShiftMask = SAMPLE_MASK(_HueShiftMask, uv0);
            surface.albedoMap.rgb = ApplyHue(surface.albedoMap.rgb, VRCHAT_GET_ATLAS_PROPERTY(_HueShift), hueShiftMask);
        }
    #endif

    #if defined(USE_NORMAL_MAPS)
        half bumpScale = VRCHAT_GET_ATLAS_PROPERTY(_BumpScale);
        surface.normalMap = UnpackScaleNormal(tex2D(_BumpMap, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(uv0, _BumpMap)), bumpScale);
    #else
        surface.normalMap = half3(0, 0, 1);
    #endif

    if (!facing)
    {
        // flip normal direction for backfaces
        surface.normalMap = -surface.normalMap;
    }

    surface.shadowBoost = VRCHAT_GET_ATLAS_PROPERTY(_ShadowBoost);
    surface.shadowAlbedo = VRCHAT_GET_ATLAS_PROPERTY(_ShadowAlbedo);
    surface.minBrightness = VRCHAT_GET_ATLAS_PROPERTY(_MinBrightness);
    surface.limitBrightness = VRCHAT_GET_ATLAS_PROPERTY(_LimitBrightness);

    #if defined(USE_EMISSION_MAP)
        uint emissionUVidx = VRCHAT_GET_ATLAS_PROPERTY(_EmissionUV);
        float2 emissionUV = SelectUV(i.uv, emissionUVidx);
        surface.emissionMap = tex2D(_EmissionMap, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(emissionUV, _EmissionMap)).rgb
            * VRCHAT_GET_ATLAS_PROPERTY(_EmissionColor)
            * VRCHAT_GET_ATLAS_PROPERTY(_EmissionStrength);
        UNITY_BRANCH if (USE_HUE_SHIFT)
            surface.emissionMap.rgb = ApplyHue(surface.emissionMap.rgb, VRCHAT_GET_ATLAS_PROPERTY(_EmissionHueShift), hueShiftMask);
    #endif

    #if defined(USE_OCCLUSION_MAP)
        surface.occlusionMap = lerp(1, SAMPLE_MASK(_OcclusionMap, uv0), VRCHAT_GET_ATLAS_PROPERTY(_OcclusionStrength));
    #else
        surface.occlusionMap = 1;
    #endif

    #if defined(UNITY_PASS_FORWARDBASE) && !defined(VRCHAT_PASS_OUTLINE)
        UNITY_BRANCH if (USE_RIMLIGHT)
        {
            surface.rimColor = VRCHAT_GET_ATLAS_PROPERTY(_RimColor);
            surface.rimAlbedoTint = VRCHAT_GET_ATLAS_PROPERTY(_RimAlbedoTint);
            surface.rimIntensity = VRCHAT_GET_ATLAS_PROPERTY(_RimIntensity);
            surface.rimRange = 1 - VRCHAT_GET_ATLAS_PROPERTY(_RimRange);
            surface.rimSharpness = VRCHAT_GET_ATLAS_PROPERTY(_RimSharpness);
            surface.rimEnvironmental = VRCHAT_GET_ATLAS_PROPERTY(_RimEnvironmental);
        }
    #endif

    #if defined(USE_DETAIL_MAPS)
        uint detailUVidx = VRCHAT_GET_ATLAS_PROPERTY(_DetailUV);
        float2 detailUV = SelectUV(i.uv, detailUVidx);
        surface.detailMode = VRCHAT_GET_ATLAS_PROPERTY(_DetailMode);
        surface.detailMaskMap = SAMPLE_MASK(_DetailMask, uv0); // always use uv0 for mask
        surface.detailAlbedoMap = tex2D(_DetailAlbedoMap, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(detailUV, _DetailAlbedoMap));
        #if defined(USE_NORMAL_MAPS)
            half detailNormalMapScale = VRCHAT_GET_ATLAS_PROPERTY(_DetailNormalMapScale) * surface.detailMaskMap;
            surface.detailNormalMap = UnpackScaleNormal(tex2D(_DetailNormalMap, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(detailUV, _DetailNormalMap)), detailNormalMapScale);
        #endif
        UNITY_BRANCH if (USE_HUE_SHIFT)
            surface.detailAlbedoMap.rgb = ApplyHue(surface.detailAlbedoMap.rgb, VRCHAT_GET_ATLAS_PROPERTY(_DetailHueShift), hueShiftMask);
    #endif

    #if defined(USE_SPECULAR)
        surface.metallic = SAMPLE_MASK(_MetallicMap, uv0) * VRCHAT_GET_ATLAS_PROPERTY(_MetallicStrength);
        float glossMap = SAMPLE_MASK(_GlossMap, uv0);
        float smoothness = glossMap * VRCHAT_GET_ATLAS_PROPERTY(_GlossStrength);
        surface.roughness = 1 - max(smoothness, GetGeometricSpecularAA(i.normal));
        surface.reflectance = glossMap * VRCHAT_GET_ATLAS_PROPERTY(_Reflectance);
        surface.specularSharpness = VRCHAT_GET_ATLAS_PROPERTY(_SpecularSharpness);
    #endif

    #if defined(USE_MATCAP)
        surface.matcapMask = SAMPLE_MASK(_MatcapMask, uv0);
        surface.matcapType = VRCHAT_GET_ATLAS_PROPERTY(_MatcapType);
        surface.matcapStrength = VRCHAT_GET_ATLAS_PROPERTY(_MatcapStrength);
    #endif

    return VRChatLightingBRDF(i.pos, i, surface);
}