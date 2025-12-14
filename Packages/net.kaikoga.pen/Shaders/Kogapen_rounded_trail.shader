// rounded_trail shader, but in world space
// - Source: rounded_trail.unitypackage
// - at: https://phi16.github.io/VRC_storage/#rounded_trail
// - by: https://twitter.com/phi16_
// License: CC0

Shader "Unlit/Kogapen_rounded_trail"
{
	Properties
	{
		_Color ("Solid Color", Color) = (1,1,1,1)
		_Invisible ("Invisible Length", Float) = 1.0
		_Width ("Width", Float) = 0.03
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float d : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			fixed4 _Color;
			float _Width;
			float _Invisible;
			
			v2g vert (appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.color = _Color * v.color;
				o.uv = v.uv;
				return o;
			}

			[maxvertexcount(10)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> stream) {
				g2f o;
				if(IN[0].uv.x + IN[2].uv.x > IN[1].uv.x * 2) return;
				o.color = IN[0].color;
				float3 p1 = IN[0].vertex.xyz;
				float3 p2 = IN[1].vertex.xyz;
				float3 v = p2 - p1;
				float3 v1 = p1 - _WorldSpaceCameraPos;
				float3 v2 = p2 - _WorldSpaceCameraPos;
				
				float size = _Width / 2;

				float3 n1 = normalize(cross(v, v1)) * size;
				float3 n2 = normalize(cross(v, v2)) * size;

				if(length(v) < _Invisible) {
					o.d = 0;
					o.uv = float2(-1,-1);
					o.vertex = UnityObjectToClipPos(float4(p1 + n1, 0));
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					o.uv = float2(-1,1);
					o.vertex = UnityObjectToClipPos(float4(p1 - n1, 0));
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					o.uv = float2(1,-1);
					o.vertex = UnityObjectToClipPos(float4(p2 + n2, 0));
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					o.uv = float2(1,1);
					o.vertex = UnityObjectToClipPos(float4(p2 - n2, 0));
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					stream.RestartStrip();
				}

				size *= 2;
				float3 nu = normalize(cross(UNITY_MATRIX_V[0].xyz, v1)) * size;
				float3 nv = normalize(cross(UNITY_MATRIX_V[1].xyz, v1)) * size;
				float4 b1 = float4(nu * 0 + nv * 1, 0);
				float4 b2 = float4(nu * -0.866 + nv * -0.5, 0);
				float4 b3 = float4(nu * 0.866 + nv * -0.5, 0);

				o.d = 1;
				if(IN[1].uv.x >= 0.999999) {
					// Actually we should calculate billboard size (nu, nv) using v2, but approximate using v1 ~= c2 instead
					o.uv = float2(0,1);
					o.vertex = UnityObjectToClipPos(p2 + b1);
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					o.uv = float2(-0.866,-0.5);
					o.vertex = UnityObjectToClipPos(p2 + b2);
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					o.uv = float2(0.866,-0.5);
					o.vertex = UnityObjectToClipPos(p2 + b3);
					UNITY_TRANSFER_FOG(o, o.vertex);
					stream.Append(o);
					stream.RestartStrip();
				}

				o.uv = float2(0,1);
				o.vertex = UnityObjectToClipPos(p1 + b1);
				UNITY_TRANSFER_FOG(o, o.vertex);
				stream.Append(o);
				o.uv = float2(-0.866,-0.5);
				o.vertex = UnityObjectToClipPos(p1 + b2);
				UNITY_TRANSFER_FOG(o, o.vertex);
				stream.Append(o);
				o.uv = float2(0.866,-0.5);
				o.vertex = UnityObjectToClipPos(p1 + b3);
				UNITY_TRANSFER_FOG(o, o.vertex);
				stream.Append(o);
				stream.RestartStrip();
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				float l = length(i.uv);
				clip(- min(i.d - 0.5, l - 0.5));
				fixed4 col = i.color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}

	/*
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct g2f
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float d : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _Color;
			float _Width;
			float _Invisible;
			
			g2f vert (appdata v)
			{
				g2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				o.color = _Color * v.color;
				o.uv = v.uv;
				o.d = 0;
				return o;
			}

			fixed4 frag (g2f i) : SV_Target
			{
				float l = length(i.uv);
				clip(- min(i.d - 0.5, l - 0.5));
				fixed4 col = i.color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	*/

	Fallback "Unlit/KogapenInstancedUnlit"
}
