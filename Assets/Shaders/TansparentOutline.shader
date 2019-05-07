// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Screw/Alpha Outline" {
	Properties{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.15)) = .005
		_OutlineOffset("Outline Offset", Vector) = (0, 0, 0)
		_MainTex("Base (RGB)", 2D) = "white" { }
		_Alpha("Alpha", Range(0,1)) = 1
	}

		CGINCLUDE
#include "UnityCG.cginc"

			struct appdata {
			half4 vertex : POSITION;
			half3 normal : NORMAL;
			half2 texcoord : TEXCOORD0;
		};

		struct v2f {
			half4 pos : POSITION;
			half2 uv : TEXCOORD0;
			half3 normalDir : NORMAL;
		};

		uniform half4 _Color;
		uniform half _Outline;
		uniform half4 _OutlineColor;

		ENDCG

			SubShader{
				Tags { "Queue" = "Transparent" }

				Pass {
					Name "STENCIL"
					ZWrite Off
					ZTest Always
					ColorMask 0

					Stencil {
						Ref 2
						Comp always
						Pass replace
						ZFail decrWrap
					}

					CGPROGRAM

					#pragma vertex vert2
					#pragma fragment frag

					v2f vert2(appdata v)
					{
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);

						return o;
					}

					half4 frag(v2f i) : COLOR
					{
						return _Color;
					}

					ENDCG


				}

				Pass {
					Name "OUTLINE"
					Tags { "LightMode" = "Always" }
					Cull Off
					ZWrite Off
					ColorMask RGB

					Blend SrcAlpha OneMinusSrcAlpha

					Stencil {
						Ref 2
						Comp NotEqual
						Pass replace
						ZFail decrWrap
					}

					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					half3 _OutlineOffset;

					v2f vert(appdata v) {
						v2f o;

						half3 vertex = v.vertex.xyz;
						vertex -= _OutlineOffset;
						vertex.x *= _Outline + 1;
						vertex.y *= _Outline + 1;
						vertex.z *= _Outline + 1;
						vertex += _OutlineOffset;
						o.pos = UnityObjectToClipPos(half4(vertex, v.vertex.w));

						return o;
					}

					half _Alpha;
					half4 frag(v2f i) :COLOR {
						return half4(_OutlineColor.rgba);
					}
					ENDCG
				}

				Pass {
					Name "BASE"
					ZWrite On
					ZTest LEqual
					Blend SrcAlpha OneMinusSrcAlpha

					CGPROGRAM

					#pragma vertex vert2
					#pragma fragment frag

					v2f vert2(appdata v)
					{
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);
						o.uv = v.texcoord;
						o.normalDir = normalize(mul(half4(v.normal, 0), unity_WorldToObject).xyz);

						return o;
					}

					uniform sampler2D _MainTex;
					uniform half4 _LightColor0;
					half _Alpha;

					half4 frag(v2f i) : COLOR
					{
						half4 c = tex2D(_MainTex, i.uv) * _Color;
										c.a = _Color.a;
										c.a = _Alpha;
										return c;

										half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
										half diffuse = max(0.4, dot(i.normalDir, lightDirection));
										half3 tex = tex2D(_MainTex, i.uv).rgb;

										half3 color = diffuse * _LightColor0.rgb * tex * _Color.rgb;
										return half4(color, _Alpha);
									}

									ENDCG
								}

		}

			Fallback Off
}