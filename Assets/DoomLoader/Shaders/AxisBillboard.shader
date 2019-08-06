﻿Shader "Doom/AxisBillboard" 
{
	Properties
	{
		[PerRenderData]_MainTex("Texture Image", 2D) = "white" {}
		[PerRenderData]_ScaleX("Scale X", Float) = 1.0
		[PerRenderData]_ScaleY("Scale Y", Float) = 1.0
		[PerRenderData]_Illumination("Self Illumination", Float) = 0.0
		[PerRenderData]_UvTransform("UV Transform", Vector) = (0,0,0,0)
		[PerRenderData]_SectorLight("Sector Light", Float) = 1.0
	}
		
	SubShader
	{
		Tags
		{
			"DisableBatching" = "True"
		}
		
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _ScaleX;
			uniform float _ScaleY;
			uniform float _SectorLight;
			uniform float _Illumination;
			uniform float4 _UvTransform;

			struct vertexInput 
			{
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
	
			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				input.vertex.x *= _ScaleX;
				input.vertex.y *= _ScaleY;

				output.tex = input.tex;
				output.tex.x = lerp(output.tex.x, 1 - output.tex.x, _UvTransform.x);
				output.tex.y = lerp(output.tex.y, 1 - output.tex.y, _UvTransform.y);


				//rotate towards camera keeping y = up
				float3 local = float3(input.vertex.x, input.vertex.y, 0);
				float3 offset = input.vertex.xyz - local;

				float3 upVector = half3(0, 1, 0);
				float3 forwardVector = UNITY_MATRIX_IT_MV[2].xyz;
				float3 rightVector = normalize(cross(forwardVector, upVector));

				float3 position = 0;
				position += local.x * rightVector;
				position += local.y * upVector;
				position += local.z * forwardVector;

				float4 rotated = float4(offset + position, 1);
				output.pos = UnityObjectToClipPos(rotated);
				//-----


				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float4 color = tex2D(_MainTex, float2(input.tex.xy));
				
				if (color.a < .5f)
					discard;
				
				color = color * unity_AmbientSky * max(_SectorLight - _Illumination, 0) + color * _Illumination;

				return color;
			}

			ENDCG
		}

		Pass
		{
			Tags{ "LightMode" = "ForwardAdd" }

			Blend One One
			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag
			#pragma multi_compile_fwdadd

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			uniform sampler2D _MainTex;
			uniform float _ScaleX;
			uniform float _ScaleY;
			uniform float4 _UvTransform;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
				LIGHTING_COORDS(1, 2)
			};

			vertexOutput vert(vertexInput v)
			{
				vertexOutput output;

				v.vertex.x *= _ScaleX;
				v.vertex.y *= _ScaleY;

				output.tex = v.tex;
				output.tex.x = lerp(output.tex.x, 1 - output.tex.x, _UvTransform.x);
				output.tex.y = lerp(output.tex.y, 1 - output.tex.y, _UvTransform.y);

				//rotate towards camera keeping y = up
				float3 local = float3(v.vertex.x, v.vertex.y, 0);
				float3 offset = v.vertex.xyz - local;

				float3 upVector = half3(0, 1, 0);
				float3 forwardVector = UNITY_MATRIX_IT_MV[2].xyz;
				float3 rightVector = normalize(cross(forwardVector, upVector));

				float3 position = 0;
				position += local.x * rightVector;
				position += local.y * upVector;
				position += local.z * forwardVector;

				float4 rotated = float4(offset + position, 1);
				output.pos = UnityObjectToClipPos(rotated);
				//----


				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float4 color = tex2D(_MainTex, float2(input.tex.xy));

				if (color.a < .5f)
					discard;

				float atten = LIGHT_ATTENUATION(input);
				color.rgb *= atten * atten * _LightColor0.rgb;

				return color;
			}

			ENDCG
		}
	}
}