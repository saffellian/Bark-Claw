Shader "Digicrafts/PixelWater/Specular"
{
	Properties
	{		
		// Pattern/Colors
		_PixelSize ("_PixelSize", Range(0,10)) = 0.2
		_BaseColor ("_BaseColor", Color) = (0.25,0.5,0.7,0.8)
		_HighlightColor ("_HighlightColor", Color) = (0.114, 0.325, 0.506,1)
		_ColorSteps ("_ColorSteps", Range (0, 64)) = 64
		_ColorNoiseScale ("_NoiseScale", Range(1,100)) = 10
		_EmissionValue ("_EmissionValue",Range(0,1)) = 0
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

		// Lighting
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
		_SpecColor ("Specular Material Color", Color) = (0.2,0.2,0.2,1)

		_NoiseType ("_NoiseType",Range(0,5)) = 0
		[HideInInspector]_Scale ("_Scale",Float) = 1
		[HideInInspector]_Debug ("_Debug",Range(0,10)) = 0	

	}
	SubShader
	{		
		Pass
		{
			Tags { 
			"Queue"="Transparent" 
			"RenderType" = "Transparent" 
			"LightMode"="ForwardBase"
			}
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			// make fog work
			#pragma multi_compile_fog
			#pragma shader_feature __ PIXEL_WATER_FOAM
			#include "_PixelWater_Core.cginc"
			#include "_PixelWater_Lighting.cginc"						
			ENDCG
		}
		Pass
		{
			Tags { 
			"Queue"="Transparent" 
			"RenderType" = "Transparent" 
			"LightMode"="ForwardAdd"
			}
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#define PIXEL_WATER_ADDPASS
			#include "_PixelWater_Core.cginc"
			#include "_PixelWater_Lighting.cginc"						
			ENDCG
		}
        Pass
        {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull Off
            CGPROGRAM
            			
            #include "_PixelWater_Meta.cginc"
			#pragma vertex vert_meta
            #pragma fragment frag_meta_px

            ENDCG
        }
	}
//Fallback "Mobile/VertexLit"
CustomEditor "Digicrafts.PixelWater.PixelWaterShaderEditor"
}
