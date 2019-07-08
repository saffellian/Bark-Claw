// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef DC_PIXELWATER_CORE_INCLUDED
#define DC_PIXELWATER_CORE_INCLUDED

#include "UnityCG.cginc"

///////// Constants //////////

static const fixed RADIAN = 0.0174532925;
static const fixed PIm2 = 1.571295;
static const fixed PI = 3.14159;
static const fixed PIx2 = 6.28318;
static const fixed PIx4 = 12.56636;
static const float3 NORMAL_UP = float3(0,1,0);

///////// Properties //////////

// Pattern
uniform float _PixelSize;
uniform fixed4 _BaseColor;
uniform fixed4 _HighlightColor;
uniform float _ColorSteps;
uniform float _ColorNoiseScale;
uniform float _OffsetX;
uniform float _OffsetY;
// Ripples
uniform float _Speed;
uniform float _Height;
uniform float _Direction;
uniform float _NoiseScale;
// Wave
uniform float _WaveSpeed;
uniform float _WaveHeight;
uniform float _WaveDirection;
uniform float _WaveFrequency;
uniform float _WaveLength;
uniform half _WaveOffset;
uniform half _WaveStretch;
uniform half _WaveBend;
// Foam
uniform fixed4 _FoamColor;
uniform float _FoamSteps;
uniform float2 _FoamBlend;
// Texture
#ifdef PIXEL_WATER_TEXTURE
uniform sampler2D _NoiseTex;
uniform float4 _NoiseTex_ST;
#endif

// Misc.
uniform int _NoiseType;
uniform float _Scale;
uniform bool _Debug;
uniform sampler2D _CameraDepthTexture; //Depth Texture

///////// Utility Funtions //////////

inline float2 rotateUV(float2 uv, float angle)
{
	float a = angle*RADIAN;//180*PI;
	float cosine = cos(a);
	float sine = sin(a);
//	float x = uv.x*cosine - uv.y*sine;
//	float y = uv.y*cosine + uv.x*sine;
	return float2(uv.x*cosine - uv.y*sine,uv.y*cosine + uv.x*sine);
}

// Standard 2x2 hash algorithm.
inline float2 hash22(float2 p) {         
    // Faster, but probaly doesn't disperse things as nicely as other methods.
    float n = sin(dot(p, float2(41, 289)));
    p = frac(float2(2097152, 262144)*n);
    return cos(p*6.283 + _Time.y);//_Time.y*
}

inline float calculateNoise(float2 p){
    
    float2 s = floor(p + (p.x+p.y)*0.3660254); // Skew the current point.
    p -= s - (s.x+s.y)*0.2113249; // Vector to unskewed base vertice.
    
    // Clever way to perform an "if" statement to determine which of two triangles we need.
    float i = step(p.x, p.y); 
    
    float2 ioffs = float2(1.0 - i, i); // Vertice offset, based on above.
    
    // Vectors to the other two triangle vertices.
    float2 p1 = p - ioffs + 0.2113249, p2 = p - 0.5773502; 
    
    // Vector to hold the falloff value of the current pixel with respect to each vertice.
    float3 d = max(0.5 - float3(dot(p, p), dot(p1, p1), dot(p2, p2)), 0.0); // Range [0, 0.5]
    
    // Determining the weighted contribution of each random gradient vector for each point...
    // Something to that effect, anyway.
    float3 w = float3(dot(hash22(s + 0.0), p), dot(hash22(s +  ioffs), p1), dot(hash22(s + 1.0), p2)); 
    
    // Combining the vectors above to produce a simplex noise value. Explaining why the vector
    // "d" needs to be cubed (at least) could take a while, but it has to do with differentiation.
    // If you take out one of the "d"s, you'll see that it needs to be cubed to work.
    return 0.5 + dot(w, d*d*d)*12.; //(2*2*2*1.5)  Range [0, 1]... Hopefully. Needs more attention.

}


///////// Core Funtions //////////

inline float2 calculateWaveHeight(float2 uvo,float scale)
{					
	float wave = 0;
// Original
//	float wl = _WaveLength/10.0/_Scale;
//	float period = _WaveSpeed*_WaveFrequency/10.0/_Scale;
// Optimise
	float v = sin(uvo.y*PI);
	float uv = uvo.x + (v-1)*_WaveBend;
	float period = _WaveSpeed*_WaveFrequency/scale;
	float t = frac(uv/period)/(_WaveLength/_WaveSpeed/_WaveFrequency);
	if(t<1) {
		if(t<_WaveStretch)
			t=t/_WaveStretch*0.5;
		else
			t=1-(1-t)/(1-_WaveStretch)*0.5;		
		wave=_WaveHeight*(1-cos(t*PIx2))/2;
		wave=wave*v*_WaveBend*2 + wave*(1-_WaveBend*2);
	}
	return float2(wave,t);
}
inline float calculateTotalWaveHeight(float2 uv1,float2 uv2,float scale)
{				
	return calculateNoise(uv1*_NoiseScale*_Scale)*_Height+calculateWaveHeight(uv2,scale).x;
}

inline float3 calculateNormals(float h, float3 normal, float2 uv1, float2 uv2, float psize, float scale)
{		
	float dd = 1/psize;
	float h1 = calculateTotalWaveHeight(float2(uv1.x+dd,uv1.y),float2(uv2.x+dd,uv2.y),scale);
	float h2 = calculateTotalWaveHeight(float2(uv1.x,uv1.y+dd),float2(uv2.x,uv2.y+dd),scale);
	dd=dd*scale;
	float3 nn = float3((h1-h)/dd,0,(h2-h)/dd);
	return normalize(normal-nn);
//	return (h1-h)/dd;
}
inline void calculateRippleColor(inout fixed4 col, float2 wuv)
{
	// Ripples
	float rippleColor = calculateNoise(wuv.xy*_ColorNoiseScale*_Scale);
	float savedRippleColor = rippleColor;//save the value
	if(_ColorSteps>0){
		if(_ColorSteps<=64) {
				rippleColor = floor(rippleColor*_ColorSteps)/_ColorSteps;//Calculate the color steps
		}
		col=lerp(col,_HighlightColor,rippleColor);
	}
}
inline float calculateRippleHeight(float2 wuv)
{	
	return calculateNoise(wuv.xy*_NoiseScale*_Scale)*_Height;
}
inline float calculateCameraDiff(float4 screenPos)
{
	//Get the distance to the camera from the depth buffer for this point
	float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos)));                                                          	
    //If the two are similar, then there is an object intersecting with our object
    return saturate(sceneZ-screenPos.w);
}
inline void calculateFoam(inout fixed4 col,float4 screenPos)
{
	// Foam
	float diff = calculateCameraDiff(screenPos);	
	diff=diff/_FoamBlend.y;
	if(diff < 1) {
		float d = diff;
		float alpha = _FoamColor.a*(smoothstep(0,_FoamBlend.x,diff)-smoothstep(0.9,1,diff));
		diff=saturate(diff/(1-(sin(_Time.x*2*_WaveSpeed*PI)*0.45+0.45)));
		diff=cos(diff*PIm2);
		if(_FoamSteps>0)
			diff=round(diff*_FoamSteps)/_FoamSteps;// quantize the colors
		if(d<0.5) {
			col.a = alpha*col.a;
			col.rgb = lerp(col.rgb,_FoamColor.rgb,diff*_FoamColor.a);
		} else {
			col.rgb = lerp(col.rgb,_FoamColor.rgb,diff*saturate(alpha));
		}
	}
}

///////// Macro //////////

#if PIXEL_WATER_FOAM
	#define PX_WATER_CALCULATE_SCREENPOS o.screenPos = ComputeScreenPos(o.pos);
#else
	#define PX_WATER_CALCULATE_SCREENPOS 
#endif


#ifdef PIXEL_WATER_TEXTURE
	#define PX_WATER_SETUV o.wuv.xy = TRANSFORM_TEX(v.uv0,_NoiseTex);
#else
	#define PX_WATER_SETUV 
#endif

#if UNITY_VERSION >= 540
// Calculate uv, height and screen pos
#define PX_WATER_CALCULATE_VERTEX(o,uv) \
	uv.x+=_OffsetX;\
	uv.y+=_OffsetY;\
	o.wuv = float4(uv,uv);\
	PX_WATER_SETUV\
	float scale=_Scale*10;\
	o.wuv.xy = rotateUV(o.wuv.xy-0.5,_Direction);\
	o.wuv.zw = rotateUV(o.wuv.zw-0.5,_WaveDirection)+0.5;\
	o.wuv.x += _Time.y*_Speed/scale;\
	o.wuv.z += (_Time.y*_WaveSpeed+_WaveOffset)/scale;\
	v.vertex.y += calculateWaveHeight(o.wuv.zw,scale)+calculateRippleHeight(o.wuv.xy);\
	o.pos = UnityObjectToClipPos(v.vertex);\
	PX_WATER_CALCULATE_SCREENPOS
#else	
#define PX_WATER_CALCULATE_VERTEX(o,uv) \
	uv.x+=_OffsetX;\
	uv.y+=_OffsetY;\
	o.wuv = float4(uv,uv);\
	PX_WATER_SETUV\
	float scale=_Scale*10;\
	o.wuv.xy = rotateUV(o.wuv.xy-0.5,_Direction);\
	o.wuv.zw = rotateUV(o.wuv.zw-0.5,_WaveDirection)+0.5;\
	o.wuv.x += _Time.y*_Speed/scale;\
	o.wuv.z += (_Time.y*_WaveSpeed+_WaveOffset)/scale;\
	v.vertex.y += calculateWaveHeight(o.wuv.zw,scale)+calculateRippleHeight(o.wuv.xy);\
	o.pos = UnityObjectToClipPos(v.vertex);\
	PX_WATER_CALCULATE_SCREENPOS
#endif


#endif //DC_PIXELWATER_CORE_INCLUDED

