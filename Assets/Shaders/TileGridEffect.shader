// Draws a grid of tiles on the screen
Shader "Custom/TileGridEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		// The size of each tile on the screen
		_PixelsPerTile ("Pixels Per Tile",Vector) = (16,16,0,0)
		// Color for grid cells
		_GridColor("Grid Color", Color) = (0,0,0,.375)
		//_Curtain("Curtain", float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		ZWrite On
		//ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float2 _PixelsPerTile;
			
			fixed4 _GridColor;
			float _Curtain;

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = v.uv;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,1,1,1);//tex2D(_MainTex, i.uv);

				// Convert from uv range (0-1) to screen range(0-ScreenDims)
				//fixed2 screenPos = i.uv * _ScreenParams.xy;
				// Divide by our pixels per tile so our grid will be scaled by that amount
				//screenPos /= _PixelsPerTile;
				// Halve the result to bring all values to .5 increments (0, .5, 1, 1.5, etc).
				//screenPos = floor(screenPos) * .5;
				
				fixed2 pos = i.vertex;
				pos /= _PixelsPerTile;
				pos = floor(pos) * .5;

				//if( i.uv.x <= _Curtain )
				{
					// Since our numbers are in .5 increments, when we add them together the 
					// fractional part can only be a 0 or .5.
					// Multiplying that result by two would bring it in the range of (0-1)
					// We'll use the alpha to determine the balance between  the grid color 
					// or the screen color on any colored grid cells.
					fixed alphaMultiplier = _GridColor.a * 2;
					fixed t = frac(pos.x + pos.y) * alphaMultiplier;
					col.xyz = lerp(col.xyz, _GridColor.xyz, t);
				}

				return col;
			}
			ENDCG
		}
	}
}
