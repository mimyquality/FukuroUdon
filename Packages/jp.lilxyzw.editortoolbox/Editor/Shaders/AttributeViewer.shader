Shader "Hidden/_lil/AttributeViewer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma exclude_renderers metal
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./AttributeViewer-main.hlsl"
            ENDCG
        }

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma exclude_renderers metal
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./AttributeViewer-geometory.hlsl"
            ENDCG
        }
    }

    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./AttributeViewer-main.hlsl"
            ENDCG
        }
    }
}
