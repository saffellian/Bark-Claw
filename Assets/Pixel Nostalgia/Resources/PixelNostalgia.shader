Shader "Hidden/PixelNostalgia"
{
Properties
{
	_MainTex ("Texture", 2D) = "white" {}
}
SubShader
{

CGINCLUDE

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
            
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
			
uniform sampler2D _MainTex;
uniform sampler2D ascii;

uniform float4 bitsPerChannel;
uniform float3 screenSize;
uniform sampler2D orderedBayer;
uniform int levels;

float4 Dither_Ordered(float4 rgba, float levels, float2 screenLoc)
{
    float bayer = tex2D(orderedBayer, screenLoc).r;
    bayer = bayer * 2.0 - 1.0;
    return float4(rgba.rgb + bayer / (levels - 1.0), rgba.a);
}

float4 Dither(float3 color, const float2 uv, float levels)
{
    return Dither_Ordered(float4(color, 1.0), levels, (uv * screenSize.xy) / screenSize.z);
}
ENDCG

Pass    // Do dithering, and do bit compression
{
    // No culling or depth
    Cull Off ZWrite Off ZTest Always
            
    CGPROGRAM
    fixed4 frag (v2f i) : SV_Target
    {
		if (bitsPerChannel.w < 0.0f)
		{
			float3 imageCol = tex2D(_MainTex, i.uv).rgb;

			float3 offset = (1.0f / bitsPerChannel) * 255.0f;
			offset = ceil(offset) / floor(offset);

			float4 c = Dither(imageCol / bitsPerChannel, i.uv, levels);
			c.rgb = (floor(c * 255.0f) / 255.0f) * offset;
			c.rgb *= bitsPerChannel;
			return c;
		}

		float3 imageCol = tex2D(_MainTex, i.uv).rgb;
		imageCol = dot(imageCol.rgb, float3(0.2126729, 0.7151522, 0.0721750)).xxx;

		float3 offset = (1.0f / bitsPerChannel.w) * 255.0f;
		offset = ceil(offset) / floor(offset);
		bitsPerChannel.xyz = bitsPerChannel.www;

		float4 c = Dither(imageCol / bitsPerChannel, i.uv, levels);
		c.rgb = (floor(c * 255.0f) / 255.0f) * offset;
		c.rgb *= bitsPerChannel;
		return c;
    }
    ENDCG
}

Pass    // Do bit compression only
{
    // No culling or depth
    Cull Off ZWrite Off ZTest Always
            
    CGPROGRAM
    fixed4 frag (v2f i) : SV_Target
    {
		if (bitsPerChannel.w < 0.0f)
		{
			float4 imageCol = tex2D(_MainTex, i.uv);

			float3 offset = (1.0f / bitsPerChannel) * 255.0f;
			offset = ceil(offset) / floor(offset);

			float4 c = float4(imageCol.rgb / bitsPerChannel, imageCol.a);
			c.rgb = (floor(c * 255.0f) / 255.0f) * offset;
			c.rgb *= bitsPerChannel;

			return c;
		}

		float4 imageCol = tex2D(_MainTex, i.uv);
		imageCol.rgb = dot(imageCol.rgb, float3(0.2126729, 0.7151522, 0.0721750)).xxx;

		float3 offset = (1.0f / bitsPerChannel.w) * 255.0f;
		offset = ceil(offset) / floor(offset);
		bitsPerChannel.xyz = bitsPerChannel.www;

		float4 c = float4(imageCol.rgb / bitsPerChannel, imageCol.a);
		c.rgb = (floor(c * 255.0f) / 255.0f) * offset;
		c.rgb *= bitsPerChannel;
		return c;
    }
    ENDCG
}

Pass    // Do ascii
{
    // No culling or depth
    Cull Off ZWrite Off ZTest Always
                
    CGPROGRAM

    #define ASCII_WIDTH_HEIGHT 16.0f
    #define ASCII_CHAR_COUNT 8.0f

    #define LUMINANCE float3(0.299f, 0.587f, 0.114f)

    fixed4 frag (v2f i) : SV_Target
    {
        float4 c = tex2D(_MainTex, i.uv);

        // 16 is the height of the ascii (it's square)
        float2 asciiUV = frac(i.uv * screenSize.xy);
        // Typically there are 8 in the LUT
        float widthX = 1.0f / ASCII_CHAR_COUNT;
        
        float luminance = dot(c.rgb, LUMINANCE);
        luminance = min(floor(luminance * ASCII_CHAR_COUNT), ASCII_CHAR_COUNT - 1.0f) / ASCII_CHAR_COUNT;

        float3 asciiCol = tex2D(ascii, float2(luminance + asciiUV.x * widthX, asciiUV.y)).rgb;
        c.rgb = asciiCol * c.rgb;

	    return c;
    }
    ENDCG
}

Pass    // Do CRT
{
    // No culling or depth
    Cull Off ZWrite Off ZTest Always
                
    CGPROGRAM

    #define res (float2(screenSize.x/1.0, screenSize.y/1.0))

    // sRGB to Linear.
    // Assuing using sRGB typed textures this should not be needed.
    float ToLinear1(float c){return(c<=0.04045)?c/12.92:pow((c+0.055)/1.055,2.4);}
    float3 ToLinear(float3 c){return float3(ToLinear1(c.r),ToLinear1(c.g),ToLinear1(c.b));}

    // Linear to sRGB.
    // Assuing using sRGB typed textures this should not be needed.
    float ToSrgb1(float c) { return(c<0.0031308 ? c*12.92 : 1.055*pow(c,0.41666) - 0.055); }
    float3 ToSrgb(float3 c) { return float3(ToSrgb1(c.r),ToSrgb1(c.g),ToSrgb1(c.b)); }

    // Hardness of scanline.
    //  -8.0 = soft
    // -16.0 = medium
    #define hardScan -8.0

    // Hardness of pixels in scanline.
    // -2.0 = soft
    // -4.0 = hard
    #define hardPix -3.0

    // Display warp.
    // 0.0 = none
    // 1.0/8.0 = extreme
    #define warp float2(1.0 / 32.0, 1.0 / 24.0)

    // Nearest emulated sample given floating point position and texel offset.
    // Also zero's off screen.
    float3 Fetch(float2 pos, float2 off)
    {
        pos = floor(pos * res + off) / res;
        if (max(abs(pos.x - 0.5), abs(pos.y - 0.5))>0.5) return 0.0;
        return ToLinear(tex2D(_MainTex, float4(pos.xy, 0.0f, -16.0f)).rgb);
    }

    // Distance in emulated pixels to nearest texel.
    float2 Dist(float2 pos) { pos = pos*res; return -((pos - floor(pos)) - float2(0.5, 0.5)); }

    // 1D Gaussian.
    float Gaus(float pos,float scale) { return exp2(scale*pos*pos); }

    // 3-tap Gaussian filter along horz line.
    float3 Horz3(float2 pos, float off)
    {
        float3 b = Fetch(pos, float2(-1.0,off));
        float3 c = Fetch(pos, float2(0.0,off));
        float3 d = Fetch(pos, float2(1.0,off));
        float dst = Dist(pos).x;
        // Convert distance to weight.
        float scale = hardPix;
        float wb = Gaus(dst - 1.0,scale);
        float wc = Gaus(dst + 0.0,scale);
        float wd = Gaus(dst + 1.0,scale);
        // Return filtered sample.
        return (b*wb + c*wc + d*wd) / (wb + wc + wd);
    }

    // 5-tap Gaussian filter along horz line.
    float3 Horz5(float2 pos,float off)
    {
        float3 a = Fetch(pos, float2(-2.0,off));
        float3 b = Fetch(pos, float2(-1.0,off));
        float3 c = Fetch(pos, float2(0.0,off));
        float3 d = Fetch(pos, float2(1.0,off));
        float3 e = Fetch(pos, float2(2.0,off));
        float dst = Dist(pos).x;
        // Convert distance to weight.
        float scale = hardPix;
        float wa = Gaus(dst - 2.0,scale);
        float wb = Gaus(dst - 1.0,scale);
        float wc = Gaus(dst + 0.0,scale);
        float wd = Gaus(dst + 1.0,scale);
        float we = Gaus(dst + 2.0,scale);
        // Return filtered sample.
        return (a*wa + b*wb + c*wc + d*wd + e*we) / (wa + wb + wc + wd + we);
    }

    // Return scanline weight.
    float Scan(float2 pos, float off)
    {
        float dst = Dist(pos).y;
        return Gaus(dst + off, hardScan);
    }

    // Allow nearest three lines to effect pixel.
    float3 Tri(float2 pos)
    {
        float3 a = Horz3(pos,-1.0);
        float3 b = Horz5(pos, 0.0);
        float3 c = Horz3(pos, 1.0);
        float wa = Scan(pos,-1.0);
        float wb = Scan(pos, 0.0);
        float wc = Scan(pos, 1.0);
        return a*wa + b*wb + c*wc;
    }

    // Distortion of scanlines, and end of screen alpha.
    float2 Warp(float2 pos)
    {
        pos = pos*2.0 - 1.0;
        pos *= float2(1.0 + (pos.y*pos.y)*warp.x, 1.0 + (pos.x*pos.x)*warp.y);
        return pos*0.5 + 0.5;
    }


    #define maskDark    0.5
    #define maskLight   1.5

    // Shadow mask.
    float3 Mask(float2 pos)
    {
        pos.x += pos.y * 3.0;
        float3 mask = maskDark;
        pos.x = frac(pos.x / 6.0);
        if (pos.x < 0.333) mask.r = maskLight;
        else if (pos.x < 0.666) mask.g = maskLight;
        else mask.b = maskLight;
        return mask;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        float2 pos = Warp(i.uv);
        float2 screenUV = i.uv * screenSize.xy * 3.0;

        float3 c = Tri(pos) * Mask(screenUV);
        
        //if (uv.x < 0 || uv.y < 0 || uv.x > 1 || uv.y > 1)
        //    return 0;

        c = ToSrgb(c) * 1.5;

	    return float4(c, 1);
    }
    ENDCG
}

}}
