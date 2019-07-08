#ifndef DC_PIXELWATER_META_INCLUDED
#define DC_PIXELWATER_META_INCLUDED

#include "UnityStandardMeta.cginc"
#include "_PixelWater_Core.cginc"
#include "_PixelWater_Lighting.cginc"						

fixed4 frag_meta_px (v2f i) : SV_Target
{
	// Colors				
	fixed4 col = _BaseColor;

	// Ripples Color
	#ifdef PIXEL_WATER_TEXTURE
	col=tex2D(_NoiseTex,i.wuv.xy)*col;
	#else
	calculateRippleColor(col,i.wuv.xy);
	#endif

	// Calculate emission
	UnityMetaInput metaIN;
  	UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  	metaIN.Albedo = col.rgb;
  	metaIN.Emission = col.rgb+col.rgb*_EmissionValue;
  	return UnityMetaFragment(metaIN);

	return col;
}

#endif