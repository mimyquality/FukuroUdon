Shader "Hidden/_lil/FixColor"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        _RemoveAlpha ("Remove Alpha", Int) = 0
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
            uint _RemoveAlpha;

            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            float4 frag(float4 vertex : SV_POSITION) : SV_Target
            {
                float4 col = _MainTex[vertex.xy];
                if(_RemoveAlpha)
                {
                    col.a = 1;
                }
                else
                {
                    if(col.a != 0) col.rgb /= saturate(col.a);
                }
                if(!IsGammaSpace()) col.rgb = LinearToGammaSpace(col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
