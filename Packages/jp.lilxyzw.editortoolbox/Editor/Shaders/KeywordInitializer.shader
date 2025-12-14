Shader "Hidden/_lil/KeywordInitializer"
{
    SubShader
    {
        CGINCLUDE
        #pragma require geometry
        #pragma require tessellation tessHW
        #pragma vertex vert
        #pragma hull hull
        #pragma domain domain
        #pragma geometry geom
        #pragma fragment frag
        #pragma multi_compile_instancing

        struct str
        {
            float a : TEXCOORD0;
        };

        struct factors {
            float edge[3] : SV_TessFactor;
            float inside : SV_InsideTessFactor;
        };

        factors hullConst(){ return (factors)0; }

        void vert(){}

        void frag(){}

        [domain("tri")]
        [partitioning("integer")]
        [outputtopology("triangle_cw")]
        [patchconstantfunc("hullConst")]
        [outputcontrolpoints(3)]
        void hull(InputPatch<str, 3> input){}

        [domain("tri")]
        void domain(factors hsConst){}

        [maxvertexcount(3)]
        void geom(triangle str input[3], inout TriangleStream<str> outStream){}
        ENDCG

        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            ENDCG
        }

        Pass
        {
            Tags {"LightMode" = "ForwardAdd"}
            CGPROGRAM
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
            ENDCG
        }

        Pass
        {
            Name "SHADOW_CASTER"
            Tags {"LightMode" = "ShadowCaster"}
            CGPROGRAM
            #pragma multi_compile_shadowcaster
            ENDCG
        }

        Pass
        {
            Tags {"LightMode" = "Meta"}
            CGPROGRAM
            #pragma shader_feature EDITOR_VISUALIZATION
            ENDCG
        }
    }
}
