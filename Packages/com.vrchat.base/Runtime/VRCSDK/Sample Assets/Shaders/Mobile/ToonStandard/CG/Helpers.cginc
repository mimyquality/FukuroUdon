float2 SelectUV(float4 fullUV, uint idx)
{
    return idx == 0 ? fullUV.xy : fullUV.zw;
}

half3 MaybeSaturate(half3 value, bool apply)
{
    return apply ? saturate(value) : value;
}

#define SAMPLE_MASK(texName, uv) \
    (tex2D(texName, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(uv, texName))[VRCHAT_GET_ATLAS_PROPERTY(texName##Channel)])

half3 F_Schlick(half3 f0, half vdh, half vdn)
{
    return f0 + (1.0 - f0) * pow((1.0 - vdh) * (1-vdn), 5.0);
}

half V_SmithGGXCorrelated(half NoV, half NoL, half roughness)
{
    half v = 0.5 / lerp(2 * NoL * NoV, NoL + NoV, roughness);
    return saturate(v);
}

#if defined(SHADER_API_MOBILE)
    #define FLOAT_MIN (1e-4 * 0.2f)
#else
    #define FLOAT_MIN 1e-6
#endif

// adapted from UnityStandardBRDF.cginc, but with a better epsilon to prevent precision issues on mobile
float VRC_GGXTerm(float NdotH, float roughness)
{
    float a2 = roughness * roughness;
    float d = (NdotH * a2 - NdotH) * NdotH + 1.0f;
    return UNITY_INV_PI * a2 / (d * d + FLOAT_MIN);
}

void GetSurfaceNormals(Surface surface, inout half3 bitangent, inout half3 tangent, inout half3 normal)
{
    #if defined(USE_DETAIL_MAPS) && defined(USE_NORMAL_MAPS)
        half3 blendedNormal = BlendNormals(surface.normalMap, surface.detailNormalMap);
    #else
        half3 blendedNormal = surface.normalMap;
    #endif
    half3 calcedNormal = normalize(blendedNormal.x * tangent + blendedNormal.y * bitangent + blendedNormal.z * normal);

    half3 bumpedTangent = cross(bitangent, calcedNormal);
    half3 bumpedBitangent = cross(calcedNormal, bumpedTangent);

    normal = calcedNormal;
    tangent = bumpedTangent;
    bitangent = bumpedBitangent;
}

// From Valve's whitepaper on VR Rendering
// https://media.steampowered.com/apps/valve/2015/Alex_Vlachos_Advanced_VR_Rendering_GDC2015.pdf
float GetGeometricSpecularAA(half3 normal)
{
    half3 vNormalWsDdx = ddx(normal.xyz);
    half3 vNormalWsDdy = ddy(normal.xyz);
    float flGeometricRoughnessFactor = pow(saturate(max(dot(vNormalWsDdx.xyz,vNormalWsDdx.xyz), dot(vNormalWsDdy.xyz,vNormalWsDdy.xyz))), 0.333);
    return max(0, flGeometricRoughnessFactor);
}

// stolen^Wadapted from VRChat-Mobile-StandardLite
half shEvaluateDiffuseL1Geomerics(half L0, half3 L1, half3 n)
{
    // avg direction of incoming light and directional brightness
    half3 R1 = 0.5f * L1;
    half lenR1 = length(R1);

    // linear angle between normal and direction 0-1, saturate fix from filamented
    half q = dot(Unity_SafeNormalize(R1), n) * 0.5 + 0.5;
    q = isnan(q) ? 1 : q;
    q = saturate(q);

    return (L0 <= 0.f) ? 0.f : ( 4. * lenR1 * pow(q, (2 * lenR1) / L0 + 1) + ( L0 * (L0 - lenR1) )/(L0 + lenR1)); 
}

half3 shEvalFull(half3 normal)
{
    return half3(
        shEvaluateDiffuseL1Geomerics(unity_SHAr.w, unity_SHAr.xyz, normal),
        shEvaluateDiffuseL1Geomerics(unity_SHAg.w, unity_SHAg.xyz, normal),
        shEvaluateDiffuseL1Geomerics(unity_SHAb.w, unity_SHAb.xyz, normal)
    );
}

// Get the most intense light Dir from probes OR from a light source
half3 GetLightDir(const bool lightEnv, float3 worldPos)
{
    // switch between using probes or actual light direction
    float3 lightDir = lightEnv ? UnityWorldSpaceLightDir(worldPos) : unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz;
    // if the average length of the light probes is null, and we don't have a directional light in the scene, fall back to our fallback lightDir
    #if !defined(POINT) && !defined(SPOT) && !defined(VERTEXLIGHT_ON) 
        if(length(unity_SHAr.xyz*unity_SHAr.w + unity_SHAg.xyz*unity_SHAg.w + unity_SHAb.xyz*unity_SHAb.w) == 0 && length(lightDir) < 0.1)
        {
            return half3(0.577, 0.577, 0.577);
        }
    #endif

    return Unity_SafeNormalize(lightDir);
}

half3 GetLightCol(const bool lightEnv, half3 lightColor, half3 lightDir)
{
    UNITY_BRANCH
    if (lightEnv)
    {
        return lightColor;
    }
    else
    {
        // no realtime light, use ambient or probe
        // calculates brightest SH, vs average in GetIndirectDiffuse ("light" vs "shadow" from probes)
        half4 lightDir4 = float4(lightDir, 1.0);
        return shEvalFull(lightDir) + max(SHEvalLinearL2(lightDir4), 0);
    }
}

// VRC mirror support
float _VRChatMirrorMode;
float3 _VRChatMirrorCameraPos;
half3 GetStereoViewDir(float3 worldPos)
{
    float3 cameraPos = _VRChatMirrorMode ? _VRChatMirrorCameraPos :
    #ifdef USING_STEREO_MATRICES
        (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5f;
    #else
        _WorldSpaceCameraPos;
    #endif

    half3 viewDir = cameraPos - worldPos;
    return normalize(viewDir);
}

#if defined(VERTEXLIGHT_ON)
// Partially lifted vertex light support from XSToon: https://github.com/Xiexe/Xiexes-Unity-Shaders
void Get4VertexLightsColFalloff(inout VertexLightInformation vLight, float3 worldPos, half3 normal)
{
    half4 toLightX = unity_4LightPosX0 - worldPos.x;
    half4 toLightY = unity_4LightPosY0 - worldPos.y;
    half4 toLightZ = unity_4LightPosZ0 - worldPos.z;

    half4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;

    half4 atten = sqrt(1.0 / (1.0 + lengthSq * unity_4LightAtten0));

    vLight.ColorFalloff[0] = saturate(unity_LightColor[0]) * atten.x;
    vLight.ColorFalloff[1] = saturate(unity_LightColor[1]) * atten.y;
    vLight.ColorFalloff[2] = saturate(unity_LightColor[2]) * atten.z;
    vLight.ColorFalloff[3] = saturate(unity_LightColor[3]) * atten.w;
}

void GetVertexLightsDir(inout VertexLightInformation vLights, float3 worldPos)
{
    float3 toLightX = float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x);
    float3 toLightY = float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y);
    float3 toLightZ = float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z);
    float3 toLightW = float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w);

    half3 dirX = normalize(toLightX - worldPos);
    half3 dirY = normalize(toLightY - worldPos);
    half3 dirZ = normalize(toLightZ - worldPos);
    half3 dirW = normalize(toLightW - worldPos);

    vLights.Direction[0] = dirX;
    vLights.Direction[1] = dirY;
    vLights.Direction[2] = dirZ;
    vLights.Direction[3] = dirW;
}
#endif

#if defined(USE_SPECULAR)
half3 GetSpecularFresnel(DotProducts dotProducts, half3 f0)
{
    return F_Schlick(f0, dotProducts.vdh, dotProducts.vdn);
}

half3 GetDirectSpecular(Surface surface, DotProducts d, float3 fresnel)
{
    half3 specular = half3(0,0,0);
    
    #if defined(USE_SPECULAR)
        float rough = max(surface.roughness * surface.roughness, 0.0045);

        float V = V_SmithGGXCorrelated(d.vdn, d.ndl, rough);
        float D = VRC_GGXTerm(d.ndh, rough);
        half3 directSpecular = max(0, (D * V) * fresnel);
        half3 directSpecularSharp = smoothstep(0.5, 0.51, directSpecular);

        specular = lerp(directSpecular, directSpecularSharp, surface.specularSharpness);
    #endif

    return specular;
}
#endif

half3 GetIndirectDiffuse()
{
    half3 ambient = half3(0,0,0);
    #if defined(UNITY_PASS_FORWARDBASE)
        // SH average only, dodges a ShadeSH9 for efficiency but means we approximate lightprobes
        ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    #endif
    return ambient;
}

//Reflection direction, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
half3 GetReflectionUV(half3 direction, float3 position, half4 cubemapPosition, half3 boxMin, half3 boxMax)
{
#if UNITY_SPECCUBE_BOX_PROJECTION
    UNITY_BRANCH
    if (cubemapPosition.w > 0) {
        half3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
        half scalar = min(min(factors.x, factors.y), factors.z);
        direction = direction * scalar + (position - cubemapPosition);
    }
#endif
    return direction;
}

half3 GetBoxProjection (half3 direction, float3 position, half4 cubemapPosition, half3 boxMin, half3 boxMax)
{
#if defined(UNITY_SPECCUBE_BOX_PROJECTION)
    UNITY_BRANCH
    if (cubemapPosition.w > 0) {
        half3 factors =
            ((direction > 0 ? boxMax : boxMin) - position) / direction;
        half scalar = min(min(factors.x, factors.y), factors.z);
        direction = direction * scalar + (position - cubemapPosition);
    }
#endif
    return direction;
}

half3 GetIndirectSpecular(Surface surface, half3 reflDir, float3 worldPos, half3 normal)
{
    half3 spec = half3(0,0,0);
    #if defined(USE_SPECULAR)
        #if defined(UNITY_PASS_FORWARDBASE)
            half3 indirectSpecular;
            Unity_GlossyEnvironmentData envData;
            envData.roughness = surface.roughness;
            envData.reflUVW = GetBoxProjection(
                reflDir, worldPos,
                unity_SpecCube0_ProbePosition,
                unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
            );

            half3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
            half interpolator = unity_SpecCube0_BoxMin.w;
            UNITY_BRANCH
            if (interpolator < 0.99999)
            {
                envData.reflUVW = GetBoxProjection(
                    reflDir, worldPos,
                    unity_SpecCube1_ProbePosition,
                    unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
                );
                half3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube0_HDR, envData);
                indirectSpecular = lerp(probe1, probe0, interpolator);
            }
            else
            {
                indirectSpecular = probe0;
            }
            half horizon = min(1 + dot(reflDir, normal), 1);
            indirectSpecular *= horizon * horizon;
            spec = indirectSpecular;
        #endif
    #endif
    return spec;
}

half3 GetRimLight(Surface surface, half attenuation, half svdn, half3 lightCol, half3 indirectCol)
{
    half3 rimlight = half3(0,0,0);

    #if defined(UNITY_PASS_FORWARDBASE) && !defined(VRCHAT_PASS_OUTLINE)
        UNITY_BRANCH if (USE_RIMLIGHT)
        {
            half rimIntensity = saturate((1 - svdn));
            rimIntensity = smoothstep(surface.rimRange - surface.rimSharpness, surface.rimRange + surface.rimSharpness, rimIntensity);
            rimlight = rimIntensity * surface.rimIntensity * surface.rimColor * (surface.rimEnvironmental ? lightCol + indirectCol : 1);
        }
    #endif
    
    return rimlight;
}

half3 SampleShadowRampTexture(half uv)
{
    return tex2Dlod(_Ramp, float4(VRCHAT_TRANSFORM_ATLAS_TEX_MODE(float2(uv, 0.5), _Ramp), 0, 0));
}

// math adapted from https://gist.github.com/mairod/a75e7b44f68110e1576d77419d608786?permalink_comment_id=3180018#gistcomment-3180018
// rotates in YIQ color space for efficiency
half3 ApplyHue(half3 col, half hueAngle, half mask)
{
    UNITY_BRANCH
    if (hueAngle == 0)
    {
        return col;
    }
    else
    {
        const half3 k = 0.57735;
        half sinAngle = sin(hueAngle);
        half cosAngle = cos(hueAngle);
        half3 shifted = col * cosAngle + cross(k, col) * sinAngle + k * dot(k, col) * (1.0 - cosAngle);
        return lerp(col, shifted, mask);
    }
}

half2 GetVRMatcapUV(half3 worldUp, half3 viewDirection, half3 normalDirection)
{
    half3 worldViewUp = normalize(worldUp - viewDirection * dot(viewDirection, worldUp));
    half3 worldViewRight = normalize(cross(viewDirection, worldViewUp));
    half2 matcapUV = half2(dot(worldViewRight, normalDirection), dot(worldViewUp, normalDirection)) * 0.5 + 0.5;
    return matcapUV;
}

half GetAlpha(Surface s, half3 vpos)
{
    half alpha = 1;
    
    #if defined(_ALPHATEST_ON)
        if(_AlphaToMask == 1)
        {
            #if defined(UNITY_PASS_SHADOWCASTER)
                clip(s.albedoMap.a - s.cutoff);
            #else
                alpha = s.albedoMap.a * s.alpha;
            #endif
        }
        else
        {
            clip(s.albedoMap.a - s.cutoff);	
        }
    #endif

    #if defined(_ALPHAPREMULTIPLY_ON)
        alpha = s.albedoMap.a;
        #if defined(UNITY_PASS_SHADOWCASTER)
            half dither = GetDither(vpos.xy);
            clip(alpha - dither);
        #endif
    #endif

    #if defined(_ALPHABLEND_ON)
        alpha = s.alpha;
        #if defined(UNITY_PASS_SHADOWCASTER)
            half dither = GetDither(vpos.xy);
            clip(alpha - dither);
        #endif
    #endif

    return alpha;
}

#if defined(USE_DETAIL_MAPS)
void ApplyDetailMap(Surface surface, inout half4 albedo)
{
    half mask = surface.detailMaskMap;
    switch (surface.detailMode)
    {
        case 0: // AlphaBlended
            albedo.rgb = lerp(albedo.rgb, surface.detailAlbedoMap.rgb, surface.detailAlbedoMap.a * mask);
            break;
        case 1: // Additive
            albedo.rgb += surface.detailAlbedoMap.rgb * mask;
            break;
        case 2: // Multiply
            albedo.rgb *= LerpWhiteTo(surface.detailAlbedoMap.rgb, mask);
            break;
        case 3: // MultiplyX2
            albedo.rgb *= LerpWhiteTo(surface.detailAlbedoMap.rgb * unity_ColorSpaceDouble.rgb, mask);
            break;
    }
}
#endif

LightVectors PopulateLightingVectors(Surface surface, float3 worldPos, half3 worldNormal, half3 tangent, half3 bitangent)
{
    LightVectors lv = (LightVectors)0;

    lv.lightEnv = any(_WorldSpaceLightPos0.xyz);
    lv.lightDir = GetLightDir(lv.lightEnv, worldPos);
    lv.lightCol = GetLightCol(lv.lightEnv, _LightColor0.rgb, lv.lightDir);
    lv.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
    lv.stereoViewDir = GetStereoViewDir(worldPos);
    lv.fixedVector = normalize(lv.lightDir + lv.viewDir);
    lv.reflViewDir = reflect(-lv.viewDir, worldNormal);
    lv.reflLightDir = reflect(lv.lightDir, worldNormal);

    return lv;
}

LightVectors PopulateVertexLightingVectors(LightVectors sharedLightVectors, half3 lightDir, half3 lightCol, half3 worldNormal)
{
    LightVectors lv = (LightVectors)0;

    lv.lightEnv = sharedLightVectors.lightEnv;
    lv.lightDir = lightDir;
    lv.lightCol = lightCol;
    lv.viewDir = sharedLightVectors.viewDir;
    lv.stereoViewDir = sharedLightVectors.stereoViewDir;
    lv.fixedVector = normalize(lv.lightDir + lv.viewDir);
    lv.reflViewDir = sharedLightVectors.reflViewDir;
    lv.reflLightDir = reflect(lv.lightDir, worldNormal);

    return lv;
}

DotProducts PopulateLightingDotProducts(LightVectors lv, half3 worldNormal, half3 tangent, half3 bitangent)
{
    DotProducts d = (DotProducts)0;

    d.ndl = dot(lv.lightDir, worldNormal); // -1 to 1
    d.ndl01 = d.ndl * 0.5 + 0.5; // 0 to 1 remapped
    d.clampedNdl = saturate(d.ndl); // 0 to 1 clamped
    d.vdn = abs(dot(lv.viewDir, worldNormal));
    d.svdn = abs(dot(lv.stereoViewDir, worldNormal));
    d.vdh = dot(lv.viewDir, lv.fixedVector);
    d.ndh = dot(worldNormal, lv.fixedVector);

    return d;
}

#if defined(UNITY_PASS_FORWARDBASE)
half3 GetDiffuseBRDFBase(Surface surface, DotProducts dotProducts, LightVectors lightVectors, half3 indirectDiffuse, half3 albedo, half attenuation, half occlusion)
{
    // attenuation is on shadow ramp
    half3 ramp = SampleShadowRampTexture(dotProducts.ndl01 * attenuation);
    half3 brightness = ramp * occlusion;

    // don't just multiply brightness, this tends to look bad for toon-style skin and materials
    // instead, color the shadows with albedo + detail
    brightness = min(1, brightness + surface.shadowBoost);
    half3 coloredContribution = lerp(albedo.rgb * surface.shadowAlbedo, 1, brightness);

    // clamp light intensity to 0-1 if _LimitBrightness is set
    return albedo * MaybeSaturate((coloredContribution * lightVectors.lightCol) + indirectDiffuse, surface.limitBrightness) * surface.alpha;
}
#endif

#if defined(UNITY_PASS_FORWARDADD) || defined(VERTEXLIGHT_ON)
half3 GetDiffuseBRDFAdd(Surface surface, DotProducts dotProducts, LightVectors lightVectors, half3 indirectDiffuse, half3 albedo, half attenuation, half occlusion)
{
    // attenuation is on brightness
    half3 ramp = SampleShadowRampTexture(dotProducts.ndl01);
    half3 brightness = ramp * attenuation * occlusion;

    // in add passes we need to pre-multiply attenuation so we start at 0 intensity
    brightness = min(1, brightness + surface.shadowBoost * attenuation);
    half3 coloredContribution = lerp(albedo.rgb * surface.shadowAlbedo * attenuation, 1, brightness);

    // clamp light intensity to 0-1 if _LimitBrightness is set
    return albedo * MaybeSaturate((coloredContribution * lightVectors.lightCol), surface.limitBrightness) * surface.alpha;
}
#endif

half3 GetSpecularBRDF(Surface surface, DotProducts dotProducts, LightVectors lv, half attenuation, float3 worldPos, half3 worldNormal, half3 vertexLightSpec, half3 fresnel)
{
    half3 specularBRDF = 0;
    #if defined(USE_SPECULAR)
        half3 directSpecular = GetDirectSpecular(surface, dotProducts, fresnel) * attenuation * dotProducts.clampedNdl * lv.lightCol;
        half3 indirectSpecular = GetIndirectSpecular(surface, lv.reflViewDir, worldPos, worldNormal) * fresnel;
        specularBRDF = max(0, (indirectSpecular + directSpecular + vertexLightSpec));

        #if defined(_ALPHAPREMULTIPLY_ON)
            specularBRDF *= surface.alpha;
        #endif
    #endif
    return specularBRDF;
}

#if defined(USE_MATCAP)
void ApplyMatcap(Surface surface, LightVectors lv, half3 worldNormal, inout half3 albedo)
{
    const half3 upVector = half3(0,1,0);
    half2 matcapUV = GetVRMatcapUV(upVector, lv.viewDir, worldNormal);
    half3 matcap = tex2D(_Matcap, matcapUV).rgb;
    half strength = surface.matcapStrength * surface.matcapMask;
    switch (surface.matcapType)
    {
        case 0: // Additive (base pass only)
#if defined(UNITY_PASS_FORWARDBASE)
            albedo += matcap * strength;
#endif
            break;
        case 1: // Multiplicative
            albedo *= lerp(1, matcap, strength);
            break;
    }
}
#endif

void PopulateVertexLights(Surface surface, LightVectors lv, float3 worldPos, half3 worldNormal, half3 tangent, half3 bitangent, half3 f0, half3 albedo, half occlusion, inout half3 vertexLightDiff, inout half3 vertexLightSpec)
{
#if defined(VERTEXLIGHT_ON)
    VertexLightInformation vLight = (VertexLightInformation)0;
    LightVectors vLightVectors = (LightVectors)0;
    DotProducts vDotProducts = (DotProducts)0;
    
    Get4VertexLightsColFalloff(/* inout */ vLight, worldPos, worldNormal);
    GetVertexLightsDir(/* inout */ vLight, worldPos);

    UNITY_LOOP
    for(int i = 0; i < 4; i++)
    {
        vLightVectors = PopulateVertexLightingVectors(lv, vLight.Direction[i], vLight.ColorFalloff[i], worldNormal);
        vDotProducts = PopulateLightingDotProducts(vLightVectors, worldNormal, tangent, bitangent);

        // run the full BRDF, just pretend it's a FORWARDADD pass - our lighting model is cheap enough that this should be fine
        vertexLightDiff += GetDiffuseBRDFAdd(surface, vDotProducts, vLightVectors, 0, albedo, vLight.ColorFalloff[i], occlusion);

        #if defined(USE_SPECULAR)
            half3 vFresnel = GetSpecularFresnel(vDotProducts, f0);
            half3 vLspec = GetDirectSpecular(surface, vDotProducts, vFresnel) * vDotProducts.clampedNdl;
            vertexLightSpec += vLspec * vLight.ColorFalloff[i];
        #endif
    }
#else
    vertexLightDiff = 0;
    vertexLightSpec = 0;
#endif
}