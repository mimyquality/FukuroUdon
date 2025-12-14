struct appdata
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2g
{
    float4 rootCS   : TEXCOORD0;
    float4 endCS    : TEXCOORD1;
    float4 orthCS   : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

struct g2f
{
    float4 vertex : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

uint _AVOutputMode;

v2g vert(appdata v)
{
    v2g o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    const float length = 0.02;
    o.rootCS = UnityObjectToClipPos(v.positionOS);

    if(_AVOutputMode == 27) o.endCS = o.rootCS + mul(UNITY_MATRIX_VP, float4(UnityObjectToWorldDir(v.tangentOS.xyz) * length, 0));
    else                    o.endCS = o.rootCS + mul(UNITY_MATRIX_VP, float4(UnityObjectToWorldNormal(v.normalOS)   * length, 0));

    float2 n = normalize(o.endCS.xy / o.endCS.w - o.rootCS.xy / o.rootCS.w);
    o.orthCS = float4(
        -n.y / _ScreenParams.x,
        n.x / _ScreenParams.y,
        0,
        0
    );

    return o;
}

[maxvertexcount(12)]
void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
{
    if(_AVOutputMode != 24 && _AVOutputMode != 27) return;
    g2f o;
    UNITY_TRANSFER_INSTANCE_ID(input[0], o);

    [unroll]
    for(int i = 0; i < 3; i++)
    {
        o.vertex = input[i].rootCS - input[i].orthCS * input[i].rootCS.w;
        outStream.Append(o);
        o.vertex = input[i].endCS  - input[i].orthCS * input[i].endCS.w;
        outStream.Append(o);
        o.vertex = input[i].rootCS + input[i].orthCS * input[i].rootCS.w;
        outStream.Append(o);
        o.vertex = input[i].endCS  + input[i].orthCS * input[i].endCS.w;
        outStream.Append(o);
        outStream.RestartStrip();
    }
}

float4 frag() : SV_Target
{
    return float4(0.05,0.15,0.7,1);
}