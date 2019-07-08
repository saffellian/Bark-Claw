// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef DC_PIXELWATER_LIGHTING_INCLUDED
#define DC_PIXELWATER_LIGHTING_INCLUDED

#include "AutoLight.cginc"
#include "UnityLightingCommon.cginc"

uniform samplerCUBE _Cube;
uniform float _EmissionValue;
uniform float _Shininess;
uniform half _Reflection;
uniform float _ShadowValue;
uniform fixed3 _ShadowColor;

#if UNITY_VERSION >= 540
uniform float4x4 unity_WorldToLight; // transformation             
#else
uniform float4x4 unity_WorldToLight; // transformation  
#endif
uniform sampler2D _LightTexture0; 

struct appdata
{
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
};
struct v2f
{
	float4 pos : SV_POSITION;
	float4 wuv : TEXCOORD0;
					float4 lightPos : TEXCOORD3;
	#if PIXEL_WATER_FOAM
	float4 screenPos : TEXCOORD2; //Screen position of pos
	#endif
	float4 worldPos : TEXCOORD4;
	#if defined(PIXEL_WATER_SHADOW)
	SHADOW_COORDS(8)
	#endif
	LIGHTING_COORDS(7,8)
	UNITY_FOG_COORDS(5)
};
v2f vert (appdata v)
{
	v2f o;
	PX_WATER_CALCULATE_VERTEX(o,v.uv0)
	#if UNITY_VERSION >= 500
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	#else
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	#endif
	o.lightPos = mul(unity_WorldToLight, o.worldPos);

	#if defined(PIXEL_WATER_ADDPASS)
	TRANSFER_VERTEX_TO_FRAGMENT(o);
	#endif

	#if defined(PIXEL_WATER_SHADOW)
	TRANSFER_SHADOW(o)
	#endif

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

inline void addLighting(inout fixed4 col, float3 normal,v2f i)  
{
    float3 normalDirection = UnityObjectToWorldNormal (normal);
    float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);
    float3 lightDirection;
    float attenuation;	
	float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * col.rgb;	 

    if (0.0 == _WorldSpaceLightPos0.w) // directional light?
    {
       attenuation = 1.0; // no attenuation
       lightDirection = normalize(_WorldSpaceLightPos0.xyz);

		// Light Cookie
	//    cookieAttenuation = tex2D(_LightTexture0, i.lightPos.xy).a;
    } 
    else // point or spot light
    {
		float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
		float d = length(vertexToLightSource);
       	#if defined(PIXEL_WATER_ADDPASS)
       	attenuation = LIGHT_ATTENUATION(i) * 1/(0.2*d+0.8*d*d);
		// Add light cookie		
		if (1.0 != unity_WorldToLight[3][3]){ // Check if spot light
			float cookieAttenuation = 0;
			ambientLighting=float3(0,0,0);// Suppress ambient lighting of HDR/overexposure
			if(i.lightPos.z>0){ // In correct direction
				cookieAttenuation = tex2D(_LightTexture0, 
					i.lightPos.xy / i.lightPos.w 
					+ float2(0.5, 0.5)).a;				
			} 
			attenuation*=cookieAttenuation;
		}
		#else
			attenuation = 1/(0.2*d+0.8*d*d);
		#endif
		
		lightDirection = normalize(vertexToLightSource);
		
    }

    
    float3 diffuseReflection = attenuation * col.rgb * _LightColor0.rgb * max(0.0, dot(normalDirection, lightDirection));	
    float3 specularReflection;
    if (dot(normalDirection, lightDirection) < 0.0) 
       // light source on the wrong side?
    {
       specularReflection = float3(0.0, 0.0, 0.0); 
		// no specular reflection
    }
    else  
    {
    //    specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);		
		specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(lightDirection,normalDirection)),_Shininess);		
    }

    fixed3 colorAfterLight = (ambientLighting + diffuseReflection + specularReflection );	
	
	#if defined(PIXEL_WATER_REFLECTIVE)
	if(_Reflection>0) {
		half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
		half3 worldRefl = reflect(-worldViewDir, normalDirection);
		#if PIXEL_WATER_REFLECMAP
		colorAfterLight+=texCUBE (_Cube, worldRefl).rgb*_Reflection;
		#else
        half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
        half3 hdrReflection = DecodeHDR (reflection, unity_SpecCube0_HDR)*_Reflection;
        colorAfterLight+=hdrReflection.rgb;
		#endif
	} 
	#endif

	if(_EmissionValue>0)
		col.rgb=colorAfterLight+col.rgb*_EmissionValue;
	else
		col.rgb=colorAfterLight;
}


fixed4 frag (v2f i) : SV_Target
{

	// Pixel size
	float psize = _PixelSize;
	float scale=_Scale*10;
	float2 savedUV = i.wuv.zw;

	// Pixelate
	if(_PixelSize>0){				
		psize = scale/_PixelSize;
		i.wuv.xy = round(i.wuv.xy * psize)/psize;
		i.wuv.zw = round(i.wuv.zw * psize)/psize;
	} else {
		psize=scale/0.1;
	}

	// #if defined(PIXEL_WATER_ADDPASS)

	// 	// Colors				
	// 	fixed4 col = fixed4(0,0,0,1);

	// #else

		// Colors				
		fixed4 col = _BaseColor;
		// Ripples Color
		#ifdef PIXEL_WATER_TEXTURE
		col=tex2D(_NoiseTex,i.wuv.xy)*col;
		#else
		calculateRippleColor(col,i.wuv.xy);
		#endif

		// apply foam
		#if PIXEL_WATER_FOAM
		calculateFoam(col, i.screenPos);
	    #endif				

	    // Apply Shadow
		#if defined(PIXEL_WATER_SHADOW)
		fixed shadow = clamp(SHADOW_ATTENUATION(i)+1-_ShadowValue,0,1);
		col.rgb = col.rgb + (1-shadow)*_ShadowColor.rgb*_ShadowValue;
		#endif

	// #endif //PIXEL_WATER_ADDPASS

	// Ripples Height
	float h=0;
	if(_Height>0) h = calculateRippleHeight(i.wuv.xy);
	// Waves Height
	if(_WaveHeight>0) h += calculateWaveHeight(savedUV,scale).x;
	// Apply lighting
	float3 normal = calculateNormals(h, NORMAL_UP, i.wuv.xy, savedUV, psize, scale);
	addLighting(col,normal*float3(-1,1,1),i);

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}                   

#endif //DC_PIXELWATER_LIGHTING_INCLUDED