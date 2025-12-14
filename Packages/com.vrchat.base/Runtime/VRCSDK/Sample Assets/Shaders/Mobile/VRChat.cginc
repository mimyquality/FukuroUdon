#ifndef VRCHAT_INCLUDED
#define VRCHAT_INCLUDED

#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"

#if defined(UNITY_SHOULD_SAMPLE_SH) || defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    #define UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
#endif

#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
    #define SAMPLE_TEXTURE2D(tex,samplertex,coord) tex.Sample (samplertex,coord)
    #define SAMPLE_TEXTURE2D_GRAD(tex,samplertex,coord,ddx,ddy) tex.SampleGrad(samplertex, coord, ddx, ddy)
    #define TEXTURE2D_PARAM(textureName, samplerName) Texture2D textureName, SamplerState samplerName
    #define TEXTURE2D_ARGS(textureName, samplerName) textureName, samplerName
#else
    #define SAMPLE_TEXTURE2D(tex,samplertex,coord) tex2D (tex,coord)
    #define SAMPLE_TEXTURE2D_GRAD(tex,samplertex,coord,ddx,ddy) tex2Dgrad(tex, coord, ddx, ddy) 
    #define TEXTURE2D_PARAM(textureName, samplerName) sampler2D textureName
    #define TEXTURE2D_ARGS(textureName, samplerName) textureName
#endif

// Use our squeezed BRDF on mobile
// In general we want FLOAT_MIN to be the smallest value such that (1.0f + FLOAT_MIN) != FLOAT_MIN
#if defined(SHADER_API_MOBILE)
    #define VRC_BRDF_PBS BRDF2_VRC_PBS 
    #define FLOAT_MIN 1e-4
#else
    #define VRC_BRDF_PBS UNITY_BRDF_PBS
    #define FLOAT_MIN 1e-6
#endif

#if defined(_BICUBIC)
    float BakeryBicubic_w0(float a)
    {
        return (1.0f/6.0f)*(a*(a*(-a + 3.0f) - 3.0f) + 1.0f);
    }

    float BakeryBicubic_w1(float a)
    {
        return (1.0f/6.0f)*(a*a*(3.0f*a - 6.0f) + 4.0f);
    }

    float BakeryBicubic_w2(float a)
    {
        return (1.0f/6.0f)*(a*(a*(-3.0f*a + 3.0f) + 3.0f) + 1.0f);
    }

    float BakeryBicubic_w3(float a)
    {
        return (1.0f/6.0f)*(a*a*a);
    }

    float BakeryBicubic_g0(float a)
    {
        return BakeryBicubic_w0(a) + BakeryBicubic_w1(a);
    }

    float BakeryBicubic_g1(float a)
    {
        return BakeryBicubic_w2(a) + BakeryBicubic_w3(a);
    }

    float BakeryBicubic_h0(float a)
    {
        return -1.0f + BakeryBicubic_w1(a) / (BakeryBicubic_w0(a) + BakeryBicubic_w1(a)) + 0.5f;
    }

    float BakeryBicubic_h1(float a)
    {
        return 1.0f + BakeryBicubic_w3(a) / (BakeryBicubic_w2(a) + BakeryBicubic_w3(a)) + 0.5f;
    }

    // Bicubic
    float4 SampleTexture2DBicubicFilter(TEXTURE2D_PARAM(tex, smp), float2 coord)
    {
        #if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
            float width, height;
            tex.GetDimensions(width, height);
            float4 texelSize = float4(width, height, 1/width, 1/height);
        #else
            float2 theSize = textureSize(tex, 0);            
            float4 texelSize = float4(
                float(theSize.x), 
                float(theSize.y), 
                1/float(theSize.x), 
                1/float(theSize.y));;
        #endif

        float x = coord.x * texelSize.z;
        float y = coord.y * texelSize.z;

        x -= 0.5f;
        y -= 0.5f;

        float px = floor(x);
        float py = floor(y);

        float fx = x - px;
        float fy = y - py;

        float g0x = BakeryBicubic_g0(fx);
        float g1x = BakeryBicubic_g1(fx);
        float h0x = BakeryBicubic_h0(fx);
        float h1x = BakeryBicubic_h1(fx);
        float h0y = BakeryBicubic_h0(fy);
        float h1y = BakeryBicubic_h1(fy);

        return     BakeryBicubic_g0(fy) * ( g0x * SAMPLE_TEXTURE2D(tex, smp, (float2(px + h0x, py + h0y) * texelSize.x))   +
                              g1x * SAMPLE_TEXTURE2D(tex, smp, (float2(px + h1x, py + h0y) * texelSize.x))) +
                   BakeryBicubic_g1(fy) * ( g0x * SAMPLE_TEXTURE2D(tex, smp, (float2(px + h0x, py + h1y) * texelSize.x))   +
                              g1x * SAMPLE_TEXTURE2D(tex, smp, (float2(px + h1x, py + h1y) * texelSize.x)));
    }
    #define MAYBE_BICUBIC_SAMPLE(texture, smp, uv) SampleTexture2DBicubicFilter(TEXTURE2D_ARGS(texture, smp), uv)
#else
    #define MAYBE_BICUBIC_SAMPLE(texture, smp, uv) SAMPLE_TEXTURE2D(texture, smp, uv)
#endif

inline half3 VRC_SafeNormalize(half3 value)
{
    float lenSqr = max((float)dot(value, value), FLOAT_MIN);
    return value * (half) rsqrt(lenSqr);
}

inline half shEvaluateDiffuseL1Geomerics(half L0, half3 L1, half3 n)
{
    // avg direction of incoming light
    half3 R1 = 0.5f * L1;

    // directional brightness
    half lenR1 = length(R1);

    // linear angle between normal and direction 0-1, saturate fix from filamented
    half q = dot(VRC_SafeNormalize(R1), n) * 0.5 + 0.5;
    q = isnan(q) ? 1 : q;
    q = saturate(q);

    // power for q
    // lerps from 1 (linear) to 3 (cubic) based on directionality
    //half p = 1.0f + 2.0f * lenR1 / L0;

    // dynamic range constant
    // should vary between 4 (highly directional) and 0 (ambient)
    //half a = (1.0f - lenR1 / L0) / (1.0f + lenR1 / L0);

    // negative ambient fix, if L0 <= 0, return 0
    //return (L0 <= 0.f) ? 0.f : (L0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p)));

    // optimized reordering. thanks wolfram
    return (L0 <= 0.f) ? 0.f : ( 4. * lenR1 * pow(q, (2 * lenR1) / L0 + 1) + ( L0 * (L0 - lenR1) )/(L0 + lenR1)); 
}

inline half shEvaluateDiffuseL1Normalized(half L0, half3 L1, half3 n)
{
    return shEvaluateDiffuseL1Geomerics(1, L1 / L0, n);
}

float PerceptualSmoothnessToRoughness(float perceptualSmoothness)
{
    float perceptualRoughness = SmoothnessToPerceptualRoughness(perceptualSmoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return roughness;
}

float RoughnessToPerceptualSmoothness(float roughness)
{
    float perceptualRoughness = RoughnessToPerceptualRoughness(roughness);
    return 1.0 - perceptualRoughness;
}

float ProjectedSpaceNormalFiltering(half perceptualSmoothness, float variance, float threshold)
{
    float roughness = PerceptualSmoothnessToRoughness(perceptualSmoothness);
    // Ref: Stable Geometric Specular Antialiasing with Projected-Space NDF Filtering - https://yusuketokuyoshi.com/papers/2021/Tokuyoshi2021SAA.pdf
    float squaredRoughness = roughness * roughness;
    float projRoughness2 = squaredRoughness / (1.0 - squaredRoughness);
    float filteredProjRoughness2 = saturate(projRoughness2 + min(2.0 * variance, threshold * threshold));
    squaredRoughness = filteredProjRoughness2 / (filteredProjRoughness2 + 1.0f);

    return RoughnessToPerceptualSmoothness(sqrt(squaredRoughness));
}

// Reference: Error Reduction and Simplification for Shading Anti-Aliasing
// Specular antialiasing for geometry-induced normal (and NDF) variations: Tokuyoshi / Kaplanyan et al.'s method.
// This is the deferred approximation, which works reasonably well so we keep it for forward too for now.
// screenSpaceVariance should be at most 0.5^2 = 0.25, as that corresponds to considering
// a gaussian pixel reconstruction kernel with a standard deviation of 0.5 of a pixel, thus 2 sigma covering the whole pixel.
float GeometricNormalVariance(float3 geometricNormalWS, float screenSpaceVariance)
{
    float3 deltaU = ddx(geometricNormalWS);
    float3 deltaV = ddy(geometricNormalWS);

    return screenSpaceVariance * (dot(deltaU, deltaU) + dot(deltaV, deltaV));
}

float ProjectedSpaceGeometricNormalFiltering(float perceptualSmoothness, float3 geometricNormalWS, float screenSpaceVariance, float threshold)
{
    float variance = GeometricNormalVariance(geometricNormalWS, screenSpaceVariance);
    return ProjectedSpaceNormalFiltering(perceptualSmoothness, variance, threshold);
}

//#define OLD_GGX_TERM
#if !defined (OLD_GGX_TERM)
inline half ComputeSpecularGGX(half3 nL1, half3 viewDir, half3 normalWorld, half smoothness)
{
    nL1 = VRC_SafeNormalize(nL1);

    half3 halfDir = VRC_SafeNormalize(nL1 - viewDir);
    half nh = saturate(dot(normalWorld, halfDir));

    half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);//* sqrt(focus));
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

    half lh = saturate(dot(nL1, halfDir));
    // ------------------------
    // Specular term
    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155

    half a = roughness;
    half a2 = a*a;

    half d = nh * nh * (a2 - 1.f) + 1.00001f;
    half specularTerm = a2 / (max(0.1f, lh*lh) * (a + 0.5f) * (d * d) * 4);

#if defined (SHADER_API_MOBILE)
    // on mobiles (where half actually means something) denominator have risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
    specularTerm = specularTerm - FLOAT_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
#endif

    return specularTerm;
}
#else
inline half ComputeSpecularGGX(half3 nL1, half3 viewDir, half3 normalWorld, half smoothness)
{
    nL1 = VRC_SafeNormalize(nL1);
    half3 halfDir = VRC_SafeNormalize(nL1 - viewDir);
    half nh = saturate(dot(normalWorld, halfDir));
    half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness );//* sqrt(focus));
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return GGXTerm(nh, roughness);
}
#endif

#if defined(_MONOSH)
// MonoSH by Bakery Lightmapper https://assetstore.unity.com/packages/tools/level-design/bakery-gpu-lightmapper-122218
inline void BakeryMonoSH(out half3 diffuseColor, out half3 specularContrib, float2 lmUV, half3 normalWorld, half3 viewDir, half smoothness, half occlusion)
{
    half3 dominantDir = MAYBE_BICUBIC_SAMPLE(unity_LightmapInd, samplerunity_Lightmap, lmUV).xyz;;
    half3 L0 = DecodeLightmap(MAYBE_BICUBIC_SAMPLE(unity_Lightmap, samplerunity_Lightmap, lmUV));

    half3 nL1 = dominantDir * 2 - 1;
    half3x3 L1 = half3x3(nL1.x * L0, nL1.y * L0, nL1.z * L0) * 2;

    half lumaL0 = dot(L0, 1);
    half3 lumaL1 = mul(L1, half3(1, 1, 1));
    half lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, lumaL1, normalWorld);

    half3 sh = L0 + mul(normalWorld, L1);
    half regularLumaSH = dot(sh, 1);
    sh *= lerp(1, lumaSH / regularLumaSH, saturate(regularLumaSH*16));

    diffuseColor = max(sh, 0.0);

    #if defined(_LMSPEC)
        half L1len = length(mul(L1, half3(1, 1, 1)));
        half focus = L1len / (length(L0) + L1len);
        half specularTerm = ComputeSpecularGGX(nL1, viewDir, normalWorld, smoothness * focus); 
        
        sh = L0 + mul(nL1, L1);

        specularContrib = max(specularTerm * sh, 0.0);

        // Reflection Probes use occlusion, direct lights don't. MonoSH and Specular Hack are both somewhere in between,
        // so we use focus to split the difference - 1.0 is direct, 0.0 is reflection probe, so we invert.
        specularContrib *= LerpOneTo(occlusion, 1 - focus);
    #else
        specularContrib = 0;
    #endif
}
#endif

inline UnityGI UnityGI_BaseVRC(UnityGIInput data, half occlusion, half3 normalWorld, half3 eyeVec, half smoothness, half hasReflProbe)
{
    UnityGI o_gi;

    // Base pass with Lightmap support is responsible for handling ShadowMask / blending here for performance reason
    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
        half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
        float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
        float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
        data.atten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
    #endif

    o_gi.light = data.light;
    o_gi.light.color *= data.atten;

    #if defined(LIGHTMAP_ON)
        #if defined(_MONOSH)
            BakeryMonoSH(o_gi.indirect.diffuse, o_gi.indirect.specular, data.lightmapUV.xy, normalWorld, eyeVec, smoothness, occlusion);
        #else
            // Baked lightmaps

            half3 bakedColor = half3(1.0, 1.0, 1.0);
            half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy);
            #if defined(FORCE_UNITY_DLDR_LIGHTMAP_ENCODING)
            bakedColor = DecodeLightmapDoubleLDR(bakedColorTex, unity_Lightmap_HDR);
            #elif defined(FORCE_UNITY_RGBM_LIGHTMAP_ENCODING)
            bakedColor = DecodeLightmapRGBM(bakedColorTex, unity_Lightmap_HDR);
            #elif defined(FORCE_UNITY_LIGHTMAP_FULL_HDR_ENCODING)
            bakedColor = bakedColorTex;
            #else
            bakedColor = DecodeLightmap(bakedColorTex);
            #endif

            // Can be set if the renderer has a valid lightmap but the shader doesn't use it
            #if !defined(UNITY_LIGHTMAP_NONE)
                #if defined(DIRLIGHTMAP_COMBINED)
                fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);
                o_gi.indirect.diffuse = DecodeDirectionalLightmap(bakedColor, bakedDirTex, normalWorld);
                #else // not directional lightmap
                o_gi.indirect.diffuse = bakedColor;
                #endif
            #else
            o_gi.indirect.diffuse = 1;
            #endif

            o_gi.indirect.specular = 0;
        #endif
        o_gi.indirect.diffuse *= occlusion;
    #elif defined(UNITY_SHOULD_SAMPLE_SH)
        o_gi.indirect.diffuse.r = shEvaluateDiffuseL1Geomerics(unity_SHAr.w, unity_SHAr.xyz, normalWorld);
        o_gi.indirect.diffuse.g = shEvaluateDiffuseL1Geomerics(unity_SHAg.w, unity_SHAg.xyz, normalWorld);
        o_gi.indirect.diffuse.b = shEvaluateDiffuseL1Geomerics(unity_SHAb.w, unity_SHAb.xyz, normalWorld);

        #if !defined(_SPECULARHIGHLIGHTS_OFF)
            UNITY_BRANCH
            #if !defined(_GLOSSYREFLECTIONS_OFF)
            if(!any(o_gi.light.color) && !hasReflProbe)
            #else
            if(!any(o_gi.light.color))
            #endif
            {
                half3 L0rgb = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
                half3x3 L1rgb = half3x3(unity_SHAr.x, unity_SHAg.x, unity_SHAb.x,
                                     unity_SHAr.y, unity_SHAg.y, unity_SHAb.y,
                                     unity_SHAr.z, unity_SHAg.z, unity_SHAb.z);
                half3 L1 = unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz;

                half3 dominantDir = VRC_SafeNormalize(L1);

                // Light can be anywhere from 'fully sparse' to 'completely focused' based on how much of it is L0 or L1rgb. 
                half L1len = length(L1);
                half focus = L1len / (length(L0rgb) + L1len);
                half specularTerm = ComputeSpecularGGX(dominantDir, eyeVec, normalWorld, smoothness * focus);
        
                // L0 + L1, the total light energy expected, is the same over the whole mesh. This is a problem with specular highlights
                // as they have a second peak in the negative direction - normally hidden by the fact that light energy there is normally zero.
                // Multiplying by non-linear diffuse gives satisfactory results, though isn't particularly physically accurate.
                // The brightness vs ground truth (a reflection probe) is too low though... closest we can get appears to be 
                // a dimensionless version, shEvaluateDiffuseL1Geometrics but applied to just the ratio.
                half energyFactor = shEvaluateDiffuseL1Normalized(dot(L0rgb, 1), L1, normalWorld);
                half3 sh = (L0rgb + mul(dominantDir, L1rgb)) * energyFactor;
                    
                o_gi.indirect.specular = max(specularTerm * sh, 0.0); 

                // Reflection Probes use occlusion, direct lights don't. MonoSH and Specular Hack are both somewhere in between,
                // so we use focus to split the difference - 1.0 is direct, 0.0 is reflection probe, so we invert.
                o_gi.indirect.specular *= LerpOneTo(occlusion, 1 - focus);
            }
            else
            {
                o_gi.indirect.specular = 0;
            }
        #else 
            o_gi.indirect.specular = 0;
        #endif
        o_gi.indirect.diffuse += data.ambient;
        o_gi.indirect.diffuse *= occlusion;
    #else
        o_gi.indirect.specular = 0;
        o_gi.indirect.diffuse = 0;
    #endif

    return o_gi;
}

struct SurfaceOutputStandardVRC
{
    fixed3 Albedo;      // base (diffuse or specular) color
    float3 Normal;      // tangent space normal, if written
    half3 Emission;
    half Metallic;      // 0=non-metal, 1=metal
    // Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
    // Everywhere in the code you meet smoothness it is perceptual smoothness
    half Smoothness;    // 0=rough, 1=smooth
    half Occlusion;
    bool SpecularAA;
    half SpecularAAVariance;
    half SpecularAAThreshold;
    fixed Alpha;        // alpha for transparencies
    half MinimumBrightness; // minimum brightness regardless of lighting
};

struct SurfaceOutputVRC 
{
    fixed3 Albedo;
    fixed3 Normal;
    fixed3 Emission;
    half Specular;
    fixed Gloss;
    fixed Alpha;
};

// Based on BRDF2_Unity_PBS
// Modified here to re-use calculations for MonoSH and squeeze out all use cases not covered by VRC
half4 BRDF2_VRC_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir,
    UnityLight light, UnityIndirect gi)
{
    half3 color = gi.diffuse * diffColor;
    #if !defined(_SPECULARHIGHLIGHTS_OFF) || !defined(_GLOSSYREFLECTIONS_OFF)
        half nv = saturate(dot(normal, viewDir));
        half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
        // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)
        half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness );//* sqrt(focus));
        half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

        // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
        // 1-x^3*(0.6-0.08*x)   approximation for 1/(x^4+1)
        half surfaceReduction = (0.6-0.08*perceptualRoughness);

        surfaceReduction = 1.0 - roughness*perceptualRoughness*surfaceReduction;

        color += surfaceReduction * gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);
    #endif

    // The DIRECTIONAL static branch exists, but in practice it seems like unity never switches back to the non-DIRECTIONAL variant once it switches
    // Not worth branching for a single dotproduct, i.e. if no specularity
#if !defined(_SPECULARHIGHLIGHTS_OFF)
    UNITY_BRANCH
    if(any(light.color))
#else
    if (true)
#endif
    {
        half nl = saturate(dot(normal, light.dir));
        half3 mergedContrib = diffColor * nl;
        #if !defined(_SPECULARHIGHLIGHTS_OFF)
            half specularTerm = ComputeSpecularGGX(light.dir, -viewDir, normal, smoothness);
            mergedContrib += max(specularTerm * specColor, 0.0);
        #endif

        color += light.color * mergedContrib;
    }
    // Original BRDF's color function. 
    // Interestingly, it doesn't appear as though fresnel is applied to specular highlights caused by lights?
    // half3 color =   (diffColor + specularTerm * specColor) * light.color * nl
    //                 + gi.diffuse * diffColor
    //                 + surfaceReduction * gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);

    return half4(color, 1);
}

// executed second
inline half4 LightingStandardVRC(SurfaceOutputStandardVRC s, float3 viewDir, UnityGI gi)
{
    s.Normal = normalize(s.Normal);
    UNITY_BRANCH if (s.SpecularAA)
        s.Smoothness = ProjectedSpaceGeometricNormalFiltering(s.Smoothness, s.Normal, s.SpecularAAVariance, s.SpecularAAThreshold);
    half3 specularColor;
    half oneMinusReflectivity;
    s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, /*out*/ specularColor, /*out*/ oneMinusReflectivity);

    // shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    // this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
    half outputAlpha;
    s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

    half4 c = VRC_BRDF_PBS(s.Albedo, specularColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
    c.a = outputAlpha;

    #ifndef _DEBUG_VRC
        return c;
    #else
        return debugCol;
    #endif
}

half3 VRC_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn, out half hasReflProbe)
{
    half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;

// TODO: CAUTION: remap from Morten may work only with offline convolution, see impact with runtime convolution!
// For now disabled
#if 0
    float m = PerceptualRoughnessToRoughness(perceptualRoughness); // m is the real roughness parameter
    const float fEps = 1.192092896e-07F;        // smallest such that 1.0+FLT_EPSILON != 1.0  (+1e-4h is NOT good here. is visibly very wrong)
    float n =  (2.0/max(fEps, m*m))-2.0;        // remap to spec power. See eq. 21 in --> https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf

    n /= 4;                                     // remap from n_dot_h formulatino to n_dot_r. See section "Pre-convolved Cube Maps vs Path Tracers" --> https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html

    perceptualRoughness = pow( 2/(n+2), 0.25);      // remap back to square root of real roughness (0.25 include both the sqrt root of the conversion and sqrt for going from roughness to perceptualRoughness)
#else
    // MM: came up with a surprisingly close approximation to what the #if 0'ed out code above does.
    perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
#endif

    half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    half3 R = glossIn.reflUVW;
    half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);
    hasReflProbe = rgbm.a;

    return DecodeHDR(rgbm, hdr);
}

inline half3 UnityGI_IndirectSpecularVRC(UnityGIInput data, half occlusion, Unity_GlossyEnvironmentData glossIn, out half hasReflProbe)
{
    half3 specular;

    #if defined(_GLOSSYREFLECTIONS_OFF)
        specular = unity_IndirectSpecColor.rgb;
        hasReflProbe = 0;
    #else
        #if defined(UNITY_SPECCUBE_BOX_PROJECTION)
            // we will tweak reflUVW in glossIn directly (as we pass it to Unity_GlossyEnvironment twice for probe0 and probe1), so keep original to pass into BoxProjectedCubemapDirection
            half3 originalReflUVW = glossIn.reflUVW;
            glossIn.reflUVW = BoxProjectedCubemapDirection (originalReflUVW, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
        #endif

        half3 env0 = VRC_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn, hasReflProbe);
        #if defined(UNITY_SPECCUBE_BLENDING)
            const float kBlendFactor = 0.99999;
            float blendLerp = data.boxMin[0].w;
            UNITY_BRANCH
            if (blendLerp < kBlendFactor)
            {
                half secondReflProbe = 0;
                half3 env1 = VRC_GlossyEnvironment (UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0), data.probeHDR[1], glossIn, secondReflProbe);
                hasReflProbe += secondReflProbe;
                specular = lerp(env1, env0, blendLerp);
            }
            else
            {
                specular = env0;
            }
        #else
            specular = env0;
        #endif
    #endif
    return specular * occlusion;
}

inline void VRC_ApplyMinBrightness(inout UnityGI gi, half minBright)
{
    gi.indirect.diffuse = max(gi.indirect.diffuse, minBright);
}

// executed first
inline void LightingStandardVRC_GI(SurfaceOutputStandardVRC s, UnityGIInput data, inout UnityGI gi)
{
    Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
    half hasReflProbe = 0;
    half3 indirectSpecular = UnityGI_IndirectSpecularVRC(data, s.Occlusion, g, /* out */ hasReflProbe);
    gi = UnityGI_BaseVRC(data, s.Occlusion, s.Normal, -data.worldViewDir, s.Smoothness, hasReflProbe);
    VRC_ApplyMinBrightness(gi, s.MinimumBrightness);
    gi.indirect.specular += indirectSpecular;
}

inline fixed4 UnityLambertVRCLight (SurfaceOutputVRC s, UnityLight light)
{
    fixed diff = max (0, dot (s.Normal, light.dir));

    fixed4 c;
    c.rgb = s.Albedo * light.color * diff;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingLambertVRC (SurfaceOutputVRC s, UnityGI gi)
{
    fixed4 c;
    c = UnityLambertVRCLight (s, gi.light);

    #if defined(UNITY_LIGHT_FUNCTION_APPLY_INDIRECT)
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif

    return c;
}

inline void LightingLambertVRC_GI (
    SurfaceOutputVRC s,
    UnityGIInput data,
    inout UnityGI gi)
{
    gi = UnityGI_BaseVRC(data, 1.0, s.Normal, half3(0, 0, 0), half(0), 0);
}


#endif
