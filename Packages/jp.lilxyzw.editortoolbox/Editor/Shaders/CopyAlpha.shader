Shader "Hidden/_lil/CopyAlpha"
{
    Properties
    {
        _TextureMain ("Texture", 2D) = "white" {}
        _TextureBack ("Texture", 2D) = "white" {}
        clipBottom ("clipBottom", Int) = 0
        clipTop ("clipTop", Int) = 0
    }
    SubShader
    {
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

            Texture2D _TextureMain;
            Texture2D _TextureBack;
            SamplerState sampler_linear_clamp;
            float4 _TextureMain_ST;
            int clipBottom;
            int clipTop;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * _TextureMain_ST.xy + _TextureMain_ST.zw;
                float4 main = _TextureMain.Sample(sampler_linear_clamp, uv);
                if(uv.x != saturate(uv.x) || uv.y != saturate(uv.y)) main.a = 0;
                if(clipBottom == 1) main.a *= _TextureBack.Sample(sampler_linear_clamp, float2(i.uv.x, min(0.5, i.uv.y))).a;
                if(clipTop == 1)    main.a *= _TextureBack.Sample(sampler_linear_clamp, float2(i.uv.x, max(0.5, i.uv.y))).a;
                if(!IsGammaSpace()) main.rgb = LinearToGammaSpace(main.rgb);
                return main;
            }
            ENDCG
        }
    }
}
