Shader "Digicrafts/PixelWater/Unlit(Texture)"
{
	Properties
	{		

		// Pattern/Colors
		_NoiseTex ("_NoiseTex", 2D) = "white" {}
		_PixelSize ("_PixelSize", Range(0,10)) = 0.2
		_BaseColor ("_BaseColor", Color) = (0.25,0.5,0.7,0.8)
//		_HighlightColor ("_HighlightColor", Color) = (0.114, 0.325, 0.506,1)
//		_ColorSteps ("_ColorSteps", Range (0, 64)) = 64
//		_ColorNoiseScale ("_NoiseScale", Range(1,100)) = 10
//		_FakeShadow ("_FakeShadow", Range (0, 1)) = 0
		_OffsetX ("_OffsetX", Range (-10, 10)) = 0
		_OffsetY ("_OffsetY", Range (-10, 10)) = 0

		// Ripples
		_Speed ("_Speed",Range(0,5)) = 0.1
		_Height ("_Height",Range(0,10)) = 0.3
		_Direction ("_WaveDirection",Range(0,360)) = 0
		_NoiseScale ("_NoiseScale", Range(1,100)) = 10

		// Wave
		_WaveSpeed ("_WaveSpeed",Range(0,10)) = 5
		_WaveHeight ("_WaveHeight",Range(0,10)) = 1.5
		_WaveDirection ("_WaveDirection",Range(0,360)) = 0
		_WaveFrequency ("_WaveFrequency",Range(0,30)) = 5
		_WaveLength ("_WaveLength",Range(0,20)) = 10
		_WaveOffset ("_WaveOffset",Float) = 0
		_WaveStretch ("_WaveStretch",Range(0.1,0.9)) = 0.5
		_WaveBend ("_WaveBend",Range(0,0.5)) = 0.1

		// Foam
		[Toggle(PIXEL_WATER_FOAM)]_Foam ("_Foam",Float) = 0
		_FoamColor ("_FoamColor", Color) = (1,1,1,1)
		_FoamBlend ("_FoamBlend", Vector) = (0.2,0.8,0)
		_FoamSteps ("_FoamSteps",Range(0,8)) = 3

		_NoiseType ("_NoiseType",Range(0,5)) = 0
		[HideInInspector]_Scale ("_Scale",Float) = 1
		[HideInInspector]_Debug ("_Debug",Range(0,10)) = 0	

	}
	SubShader
	{
		Tags { 
		"Queue"="Transparent" 
		"RenderType" = "Transparent" 
		}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			// make fog work
			#pragma multi_compile_fog
			#pragma shader_feature __ PIXEL_WATER_FOAM
			#define PIXEL_WATER_TEXTURE
			#include "_PixelWater_Core.cginc"

			uniform float _FakeShadow;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv0 : TEXCOORD0;
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 wuv : TEXCOORD0;
				#if PIXEL_WATER_FOAM
				float4 screenPos : TEXCOORD2; //Screen position of pos
				#endif
				UNITY_FOG_COORDS(5)
			};

			v2f vert (appdata v)
			{
				v2f o;
				PX_WATER_CALCULATE_VERTEX(o,v.uv0)
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
				
			fixed4 frag (v2f i) : SV_Target
			{

				// Colors				
				fixed4 col = _BaseColor;

				// Pixel size
				float psize = _PixelSize;
				float scale=_Scale*10;
				float4 savedUV = i.wuv;

				// Pixelate
				if(_PixelSize>0){				
					psize = scale/_PixelSize;
					i.wuv.xy = round(i.wuv.xy * psize)/psize;
					i.wuv.zw = round(i.wuv.zw * psize)/psize;
				} else {
					psize=scale/0.1;
				}

				// Ripples Color
				col=tex2D(_NoiseTex,i.wuv.xy)*col;

				if(_FakeShadow>0){
					float h = 0;//calculateRippleHeight(savedUV.xy);
					if(_WaveHeight>0){
						h = h + calculateWaveHeight(savedUV.zw,scale).x;
					}
					h=h/(_Height+_WaveHeight);
					col.rgb = col.rgb - col.rgb*clamp(lerp(0.5,0,h),0,0.5)*_FakeShadow;
				}

				// apply foam
				#if PIXEL_WATER_FOAM
				calculateFoam(col, i.screenPos);
	            #endif				

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
Fallback "Transparent/Diffuse"
CustomEditor "Digicrafts.PixelWater.PixelWaterShaderEditor"
}
