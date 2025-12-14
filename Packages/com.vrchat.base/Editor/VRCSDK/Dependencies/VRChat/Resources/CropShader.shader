Shader "Hidden/VRChat/CropShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TargetWidth("Target Width", Float) = 800
        _TargetHeight("Target Height", Float) = 600
        _ForceLinear("Force Linear", Int) = 0
        _ForceGamma("Force Gamma", Int) = 0
        [ToggleUI]_CenterCrop("Center Crop", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D<float4> _MainTex;
            SamplerState sampler_MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _TargetWidth;
            float _TargetHeight;
            int _CenterCrop;
            int _ForceLinear;
            int _ForceGamma;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float sourceWidth = _MainTex_TexelSize.z;
                float sourceHeight = _MainTex_TexelSize.w;
                float sourceAspect = sourceWidth / sourceHeight;
                float targetAspect = _TargetWidth / _TargetHeight;

                float2 uv = i.uv;
                float2 correctiveScale = 0;
                float mask = 1;

                if (abs(sourceAspect - targetAspect) < 0.001)
                {
                    float4 col = _MainTex.Sample(sampler_MainTex, uv);
                    if (_ForceGamma)
                    {
                        col.rgb = pow(col.rgb, 1.0/2.2);
                    }
                    if (_ForceLinear)
                    {
                        col.rgb = pow(col.rgb, 2.2);
                    }
                    return col;
                }

                float2 normalizedResolution = float2(sourceWidth / targetAspect, sourceHeight);
                
                if (_CenterCrop ? (normalizedResolution.x < normalizedResolution.y) : (normalizedResolution.x > normalizedResolution.y))
                {
                    correctiveScale = float2(1, normalizedResolution.y / normalizedResolution.x);
                }
                else
                {
                    correctiveScale = float2(normalizedResolution.x / normalizedResolution.y, 1);
                }

                uv = ((uv - 0.5) / correctiveScale) + 0.5;
                float2 uvPadding = (1 / float2(sourceWidth, sourceHeight)) * 0.1;
                float2 uvfwidth = fwidth(uv.xy);
                float2 maxFactor = smoothstep(uvfwidth + uvPadding + 1, uvPadding + 1, uv.xy);
                float2 minFactor = smoothstep(-uvfwidth - uvPadding, -uvPadding, uv.xy);
                mask = maxFactor.x * maxFactor.y * minFactor.x * minFactor.y;
                
                float4 col = _MainTex.Sample(sampler_MainTex, uv);
                col.rgb = lerp(col.rgb, float3(0.5,0.5,0.5), 1 - mask);
                if (_ForceGamma)
                {
                    col.rgb = pow(col.rgb, 1.0/2.2);
                }
                if (_ForceLinear)
                {
                    col.rgb = pow(col.rgb, 2.2);
                }
                return col;
            }
            ENDCG
        }
    }
}
