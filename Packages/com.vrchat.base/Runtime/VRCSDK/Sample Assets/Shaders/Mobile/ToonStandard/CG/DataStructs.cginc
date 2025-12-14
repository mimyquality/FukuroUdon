struct appdata
{
    float4 vertex : POSITION;
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    half3 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    VRCHAT_ATLAS_VERTEX_INPUT
};

struct v2f
{
    float4 pos : SV_POSITION;
    float4 uv : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    half3 normal : TEXCOORD2;
    half4 tangent : TEXCOORD3;
    half3 viewDir : TEXCOORD4;
    half3 color : COLOR;

    #if !defined(UNITY_PASS_SHADOWCASTER)
        UNITY_LIGHTING_COORDS(5, 6)
        UNITY_FOG_COORDS(7)
    #endif

    UNITY_VERTEX_OUTPUT_STEREO
    VRCHAT_ATLAS_VERTEX_OUTPUT
};

struct LightVectors
{
    bool lightEnv;
    half3 lightDir;
    half3 lightCol;
    half3 viewDir;
    half3 stereoViewDir;
    half3 fixedVector;
    half3 reflViewDir;
    half3 reflLightDir;
};

struct Surface
{
    half4 albedoMap;
    half3 emissionMap;
    half occlusionMap;
    half matcapMask;
    uint matcapType;
    half matcapStrength;

    uint detailMode;
    half detailMaskMap;
    half4 detailAlbedoMap;

    half3 normalMap;
#if defined(USE_NORMAL_MAPS) && defined(USE_DETAIL_MAPS)
    half3 detailNormalMap;
#endif

    half alpha;
    half shadowBoost;
    half shadowAlbedo;
    half minBrightness;
    bool limitBrightness;

    float roughness;
    half metallic;

    half specularSharpness;
    half reflectance;
    half3 rimColor;
    half rimAlbedoTint;
    half rimIntensity;
    half rimRange;
    half rimSharpness;
    bool rimEnvironmental;
    //half cutoff;
};

struct DotProducts {
    half ndl; // Surface Normal . Light Direction -1 to 1 "raw"
    half ndl01; // Surface Normal . Light Direction 0 to 1 remapped "lambert wrapped"
    half clampedNdl; // Surface Normal . Light Direction 0 to 1 clamped "lambert"
    half vdn; // View Direction . Light Direction
    half svdn; // Stereo View Direction . Light Direction, used for things that need to be "stereo convergent" for VR
    half vdh; // View Direction . halfway Direction
    half ndh; // Surface Normal . halfway Direction
};

struct VertexLightInformation {
    half3 Direction[4];
    half3 ColorFalloff[4];
};