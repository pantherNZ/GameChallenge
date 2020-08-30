Shader "Custom/DarknessShader"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert alpha
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			#define MAX_LIGHT_SOURCES 5

			// X, Y (screen coord), Z = radius
			float4 lightSources[MAX_LIGHT_SOURCES];
			int lights = 0;
			float aspectRatio;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
			};

			v2f vert( appdata_t IN )
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos( IN.vertex );
				OUT.texcoord = float2( IN.texcoord.x, IN.texcoord.y / aspectRatio );
				return OUT;
			}

			float4 circle( float2 uv, float2 pos, float radius, float3 color ) 
			{
				float d = length( pos - uv ) - max( 0.0f, radius - 0.1f );
				float t = clamp( d, 0.0f, 1.0f );
				return float4( color, smoothstep( 0.0f, 0.1f, t ) );
			}

			float4 frag( v2f IN ) : SV_Target
			{
				float alpha = 1.0f;

				for( int i = 0; i < lights; ++i )
				{
					float4 c = circle( IN.texcoord, lightSources[i].xy, lightSources[i].z, float3( 0.0f, 0.0f, 0.0f ) );
					alpha *= c.w;
				}

				return float4( 0.0f, 0.0f, 0.0f, alpha );
			}

            ENDCG
        }
    }
}
