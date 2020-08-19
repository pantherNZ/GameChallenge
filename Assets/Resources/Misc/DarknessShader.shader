Shader "Custom/DarknessShader"
{
	Properties
	{
		lightRadius( "Light Radius", Float ) = 10.0
	}

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
			float3 lightSources[MAX_LIGHT_SOURCES];

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
				OUT.texcoord = IN.texcoord * 10.0f;
				return OUT;
			}

			float4 frag( v2f IN ) : SV_Target
			{
				float alpha = 1.0f;

				lightSources[0] = float3( 0.5f, 0.5f, 0.01f );

				for( int i = 0; i < MAX_LIGHT_SOURCES; ++i )
				{
					float diffX = lightSources[i].x - IN.texcoord.x;
					float diffY = lightSources[i].y - IN.texcoord.y;
					float len = sqrt( diffX * diffX + diffY + diffY );

					return float4( 0.0f, 0.0f, 0.0f, len * 2.0f );

					if( len < lightSources[i].z )
						alpha = min( alpha, 0.0f );
				}

				return float4( 0.0f, 0.0f, 0.0f, alpha );
			}

            ENDCG
        }
    }
}
