struct v2f_outline
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    half3 color : TEXCOORD1;

    UNITY_FOG_COORDS(2)

    UNITY_VERTEX_OUTPUT_STEREO
    VRCHAT_ATLAS_VERTEX_OUTPUT
};

VRCHAT_DEFINE_ATLAS_PROPERTY(half, _OutlineThickness);
VRCHAT_DEFINE_ATLAS_PROPERTY(half3, _OutlineColor);
VRCHAT_DEFINE_ATLAS_PROPERTY(half, _OutlineFromAlbedo);

sampler2D _OutlineMask;
VRCHAT_DEFINE_ATLAS_PROPERTY(half4, _OutlineMask_ST);
VRCHAT_DEFINE_ATLAS_PROPERTY(uint, _OutlineMaskChannel);
VRCHAT_DEFINE_ATLAS_TEXTUREMODE(_OutlineMask);

// When an avatar scales really small, the outline (since it's calculated in world space) becomes really thick.
// This function clamps the outline scale based on the screen-space length of the normal offset, avoiding that
// issue at the cost of a bit of a fudge factor chosen to accomodate the typical avatar scale range in VRChat.
float4 GetScreenSpaceClampedOffsetClipPosition(float3 worldPos, float3 offsetWorldPos, float thickness)
{
    float4 clipPos = UnityWorldToClipPos(float4(worldPos, 1));
    float4 offsetClipPos = UnityWorldToClipPos(float4(offsetWorldPos, 1));
    float dist = distance(clipPos.xy / clipPos.w, offsetClipPos.xy / offsetClipPos.w);
    if (dist <= 0.0001f) // avoid division by zero
        return offsetClipPos;
    float clampedDistance = min(dist, 0.05f * thickness);
    return lerp(clipPos, offsetClipPos, clampedDistance / dist);
}

v2f_outline vert_outline (appdata v)
{
    v2f_outline o = (v2f_outline)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VRCHAT_ATLAS_INITIALIZE_VERTEX_OUTPUT(v, o);
    VRCHAT_SETUP_ATLAS_INDEX_POST_VERTEX(o);

    uint maskChannel = VRCHAT_GET_ATLAS_PROPERTY(_OutlineMaskChannel);
    float mask = tex2Dlod(_OutlineMask, half4(v.uv, 0, 0))[maskChannel];
    float thickness = VRCHAT_GET_ATLAS_PROPERTY(_OutlineThickness);

    if (thickness <= 0)
    {
        o.pos = float4(0, 0, 0, 0);
        return o;
    }

    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
    float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
    float3 offsetWorldPos = worldPos + worldNormal * thickness * mask * 0.01;

    o.pos = GetScreenSpaceClampedOffsetClipPosition(worldPos, offsetWorldPos, thickness);
    o.uv = v.uv;

    UNITY_BRANCH if (VRCHAT_GET_ATLAS_PROPERTY(_VertexColor))
        o.color = v.color;
    else
        o.color = half3(1, 1, 1);

    UNITY_TRANSFER_FOG(o, o.pos);

    return o;
}

half4 frag_outline (v2f_outline i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    VRCHAT_SETUP_ATLAS_INDEX_POST_VERTEX(i);

    half3 outlineColor = VRCHAT_GET_ATLAS_PROPERTY(_OutlineColor);
    half outlineFromAlbedo = VRCHAT_GET_ATLAS_PROPERTY(_OutlineFromAlbedo);

    half3 color = outlineColor;

    UNITY_BRANCH if (outlineFromAlbedo)
    {
        half3 albedo = tex2D(_MainTex, VRCHAT_TRANSFORM_ATLAS_TEX_MODE(i.uv, _MainTex)).rgb;
        albedo *= VRCHAT_GET_ATLAS_PROPERTY(_Color).rgb;
        albedo *= i.color;
        color = lerp(color, albedo, outlineFromAlbedo);
    }

    half4 finalColor = half4(color, 1);
    UNITY_APPLY_FOG(i.fogCoord, finalColor);
    return finalColor;
}