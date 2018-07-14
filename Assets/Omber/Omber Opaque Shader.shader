﻿Shader "Unlit/Omber/Opaque Shader"
{
    // The Omber Opaque Shader renders shapes with no transparency, so no
    // alpha blending is needed and shapes can be rendered in any order since
    // the depth buffer will ensure that closer objects remain in front.
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
        Cull Off

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
                fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                // TODO: Make a shader for shapes without textures?
				fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.color;
				return col;
			}
			ENDCG
		}
	}
}
