half4 VRChatLightingBRDF(UNITY_POSITION(vpos), v2f i, Surface surface)
{
    UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
    // fix for rare bug where light atten is 0 when there is no directional light in the scene
    #ifdef UNITY_PASS_FORWARDBASE
        if(all(_LightColor0.rgb == 0.0)) { attenuation = 1.0; }
    #endif

#if !defined(UNITY_PASS_SHADOWCASTER)
    half3 worldNormal = i.normal;
    half3 tangent = i.tangent.xyz;
    half3 bitangent = cross(worldNormal, tangent) * sign(i.tangent.w);
    float3 worldPos = i.worldPos;
    GetSurfaceNormals(surface, bitangent, tangent, worldNormal);

    LightVectors lightVectors = PopulateLightingVectors(surface, worldPos, worldNormal, tangent, bitangent);
    DotProducts dotProducts = PopulateLightingDotProducts(lightVectors, worldNormal, tangent, bitangent);

    half4 albedo = surface.albedoMap;
    #if defined(USE_DETAIL_MAPS)
        ApplyDetailMap(surface, /* inout */ albedo);
    #endif
    half3 diffuseColor = albedo.rgb;
    albedo.rgb *= (1.0 - surface.metallic);
    surface.alpha = GetAlpha(surface, i.pos);
    half occlusion = surface.occlusionMap;

    half3 emission = 0;
    #if defined(USE_EMISSION_MAP) && defined(UNITY_PASS_FORWARDBASE) // We don't want emission in shadow or add pass.
        emission = surface.emissionMap;
    #endif

    // minimum brightness has a max of 0.1 so just apply it unconditionally, little risk of blowing anything out
    emission += surface.minBrightness * albedo;

    // f0 is specular color
    half3 f0 = half3(0,0,0);
    half3 fresnel = half3(0,0,0);

    #if defined(USE_SPECULAR)
        f0 = 0.16 * surface.reflectance * surface.reflectance * (1.0 - surface.metallic) + diffuseColor * surface.metallic;
        fresnel = GetSpecularFresnel(dotProducts, f0) * saturate((surface.reflectance + surface.metallic) * 4.0f);
    #endif

    #if defined(USE_MATCAP)
        ApplyMatcap(surface, lightVectors, worldNormal, /* inout */ albedo.rgb);
    #endif

    half3 vertexLightDiff = 0;
    half3 vertexLightSpec = 0;
    PopulateVertexLights(surface, lightVectors, worldPos, worldNormal, tangent, bitangent, f0, albedo, occlusion, vertexLightDiff, vertexLightSpec);

    half3 indirectDiffuse = GetIndirectDiffuse() * occlusion;
    #if defined(UNITY_PASS_FORWARDBASE)
        half3 diffuseBRDF = GetDiffuseBRDFBase(surface, dotProducts, lightVectors, indirectDiffuse, albedo, attenuation, occlusion);
    #else
        half3 diffuseBRDF = GetDiffuseBRDFAdd(surface, dotProducts, lightVectors, indirectDiffuse, albedo, attenuation, occlusion);
    #endif
    half3 specularBRDF = GetSpecularBRDF(surface, dotProducts, lightVectors, attenuation, worldPos, worldNormal, vertexLightSpec, fresnel);
    half3 rimLight = GetRimLight(surface, attenuation, dotProducts.svdn, lightVectors.lightCol, indirectDiffuse);
    rimLight *= LerpWhiteTo(diffuseColor, surface.rimAlbedoTint);

    half3 litFragment = diffuseBRDF + specularBRDF + vertexLightDiff + emission + rimLight;

    half4 finalColor = half4(litFragment, surface.alpha);
    UNITY_APPLY_FOG(i.fogCoord, finalColor);
    return finalColor;
#else
    return half4(0, 0, 0, GetAlpha(surface, i.pos));
#endif
}