// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/World"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _Albedo("Albedo", 2D) = "white" {}
		[NoScaleOffset] _HeightMap ("HeightMap", 2D) = "white" {}
		_SeaLevel("SeaLevel", Range(0,1)) = 1.0
		_Height("Height", Float) = 1.0
		_NormalStrength("NormalStrength", Float) = 1
	}
		SubShader
		{
			Pass
			{
				Tags
				{
					"RenderType" = "Opaque"
					"RenderPipeline" = "UniversalPipeline"
					"LightMode" = "UniversalForward"
				}

				HLSLPROGRAM
			// use "vert" function as the vertex shader
			#pragma vertex vert
			// use "frag" function as the pixel (fragment) shader
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"



			sampler2D _Albedo;
			sampler2D _HeightMap;
			float4 _HeightMap_TexelSize;
			float _SeaLevel;
			float _Height;

			float _NormalStrength;

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			v2f vert(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
			{
				v2f o;
				float heightMap = tex2Dlod(_HeightMap, float4(uv.xy, 0, 0));
				
				float3 pos = vertex;

				if(heightMap >= _SeaLevel)
					pos = vertex + normal * _Height * (heightMap - 0.5) * 2;

				o.pos = TransformObjectToHClip(pos);
				o.normal = TransformObjectToWorldNormal(normal);
				o.uv = uv;
				return o;
			}		

			//https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.161.8979&rep=rep1&type=pdf
			float3 filterNormal(float2 uv, float strength) 
			{
				float4 h;
				h[0] = tex2D(_HeightMap, uv + _HeightMap_TexelSize.g * float2(0, -1)).r * strength;
				h[1] = tex2D(_HeightMap, uv + _HeightMap_TexelSize.r * float2(-1, 0)).r * strength;
				h[2] = tex2D(_HeightMap, uv + _HeightMap_TexelSize.r * float2( 1, 0)).r * strength;
				h[3] = tex2D(_HeightMap, uv + _HeightMap_TexelSize.g * float2( 0, 1)).r * strength;
				
				float3 n;
				n.x = h[1] - h[2];
				n.y = h[0] - h[3];
				n.z = 2;
				return normalize(n);
			}
		
			float3 frag(v2f i) : SV_Target
			{
				float3 normal = filterNormal(i.uv, _NormalStrength);
				normal = normalize(i.normal + float3(normal.rg, 0));
			

				// sample texture and return it
				float3 color = normalize(tex2D(_Albedo, i.uv) + 0.2);
				float height = tex2D(_HeightMap, i.uv);

				if(height < _SeaLevel)
				{
					normal = i.normal;
					//color = saturate(height + 0.1) * float3(0.4, 0.5, 1);
				}
				else
				{
					//color = float3(height, height, height);
				}

				float3 lighting = dot(_MainLightPosition.xyz, normal);
				return color * lighting;
			}
			ENDHLSL
		}
	}
}