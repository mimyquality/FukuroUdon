Shader "Hidden/_lil/OutlineMaker"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Color", Color) = (1,1,1,1)
        _OutlineWidth ("Width", Float) = 2
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            Texture2D _MainTex;
            float4 _OutlineColor;
            float _OutlineWidth;
            SamplerState sampler_linear_clamp;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 ClampSample(Texture2D tex, float2 uv)
            {
                float4 col = tex.Sample(sampler_linear_clamp, uv);
                if(uv.x != saturate(uv.x) || uv.y != saturate(uv.y)) col.a = 0;
                return col;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = lerp(i.uv, 0.5, -0.02*_OutlineWidth);
                float outline = 0;
                outline += ClampSample(_MainTex, uv).a;
                outline += ClampSample(_MainTex, uv + float2(-0.01, 0.00)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2( 0.01, 0.00)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2( 0.00,-0.01)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2( 0.00, 0.01)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2(-0.007,-0.007)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2(-0.007, 0.007)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2( 0.007,-0.007)*_OutlineWidth).a;
                outline += ClampSample(_MainTex, uv + float2( 0.007, 0.007)*_OutlineWidth).a;
                outline = saturate(outline);
                float4 col = _OutlineColor;
                col.a = outline;

                float4 tex = ClampSample(_MainTex, uv);
                col.rgb = lerp(col.rgb, tex.rgb, tex.a);
                return col;
            }
            ENDCG
        }
    }
}
