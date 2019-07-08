using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Digicrafts.PixelWater;

namespace Digicrafts.PixelWater
{
	public class ShaderEditorProperty
	{
		public string title;
		public MaterialProperty prop;

		public ShaderEditorProperty(string title)
		{
			this.title=title;
		}
	}		

	public class PixelWaterShaderEditor : ShaderGUI {

		private bool _inited = false;
		private float _foamEnable = -1;
		private bool _lightingEnable = false;
		private float _min=0;
		private float _max=0;

		private static Dictionary<string,ShaderEditorProperty> _materialProperties=
			new Dictionary<string,ShaderEditorProperty>(){
			// Pattern
			{"_PixelSize",new ShaderEditorProperty("Pixel Size")},
			{"_NoiseTex",new ShaderEditorProperty("Texture")},
			{"_ColorNoiseScale",new ShaderEditorProperty("Noise")},
			{"_BaseColor",new ShaderEditorProperty("Color 1")},
			{"_HighlightColor",new ShaderEditorProperty("Color 2")},
			{"_ColorSteps",new ShaderEditorProperty("Color Steps")},
			{"_FakeShadow",new ShaderEditorProperty("Fake Shadow")},
			{"_EmissionValue",new ShaderEditorProperty("Emssion")},

			{"_OffsetX",new ShaderEditorProperty("OffsetX")},
			{"_OffsetY",new ShaderEditorProperty("OffsetY")},
			// Lighting
			{"_Cube",new ShaderEditorProperty("Reflection Map")},
			{"_Reflection",new ShaderEditorProperty("Reflection")},
			{"_Shininess",new ShaderEditorProperty("Shininess")},
			{"_SpecColor",new ShaderEditorProperty("Specular Color")},
			{"_ShadowValue",new ShaderEditorProperty("Shadow")},
			{"_ShadowColor",new ShaderEditorProperty("Shadow Color")},
			// Foam
			{"_Foam",new ShaderEditorProperty("Enable")},
			{"_FoamColor",new ShaderEditorProperty("Color")},
			{"_FoamSteps",new ShaderEditorProperty("Color Steps")},
			{"_FoamBlend",new ShaderEditorProperty("Blend [in,out]")},
			// Ripples
			{"_Speed",new ShaderEditorProperty("Speed [unit/s]")},
			{"_Height",new ShaderEditorProperty("Height [unit]")},
			{"_Direction",new ShaderEditorProperty("Direction [0-360]")},
			{"_NoiseScale",new ShaderEditorProperty("Noise")},
			// Waves
			{"_WaveSpeed",new ShaderEditorProperty("Speed [unit/s]")},
			{"_WaveHeight",new ShaderEditorProperty("Height [unit]")},
			{"_WaveFrequency",new ShaderEditorProperty("Frequency [s]")},
			{"_WaveLength",new ShaderEditorProperty("Wave Length [unit]")},
			{"_WaveOffset",new ShaderEditorProperty("Offset [unit]")},
			{"_WaveDirection",new ShaderEditorProperty("Direction [0-360]")},
			{"_WaveStretch",new ShaderEditorProperty("Stretch")},
			{"_WaveBend",new ShaderEditorProperty("Bend")},
			//
			{"_NoiseType",new ShaderEditorProperty("_NoiseType")},
			{"_Scale",new ShaderEditorProperty("_Scale")}};

		virtual public void FindProperties (MaterialProperty[] props)
		{				
			if(_inited){

			} else {
				// Loop and find property
				foreach(KeyValuePair<string,ShaderEditorProperty> kvp in _materialProperties){
					kvp.Value.prop=FindProperty(kvp.Key,props,false);
				}
				// check if shader support lighting
				if(_materialProperties["_Shininess"].prop!=null) _lightingEnable=true;
				_inited=true;
			}
		}			

		private void ShaderProperty(MaterialEditor materialEditor, string name, int labelIndent = 0)
		{
			ShaderEditorProperty property = _materialProperties[name];
			if(property.prop!=null)
				materialEditor.ShaderProperty(property.prop,property.title,labelIndent);
		}

		public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
		{						
			FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

//			EditorStyles.DrawHeader("Pixel Water");
			EditorStyles.DrawSectionHeader("Pixel Pattern","pattern-icon");
			ShaderProperty(materialEditor,"_NoiseTex");
			ShaderProperty(materialEditor,"_PixelSize");
			ShaderProperty(materialEditor,"_ColorNoiseScale");
			ShaderProperty(materialEditor,"_BaseColor");
			ShaderProperty(materialEditor,"_HighlightColor");
			ShaderProperty(materialEditor,"_ColorSteps");
			ShaderProperty(materialEditor,"_EmissionValue");
			ShaderProperty(materialEditor,"_FakeShadow");
	ShaderProperty(materialEditor,"_OffsetX");
	ShaderProperty(materialEditor,"_OffsetY");
			EditorStyles.DrawSectionHeader("Ripples","ripple-icon");
			ShaderProperty(materialEditor,"_Height");
			ShaderProperty(materialEditor,"_Speed");
			ShaderProperty(materialEditor,"_Direction");
			ShaderProperty(materialEditor,"_NoiseScale");
			EditorStyles.DrawSectionHeader("Waves","wave-icon");
			ShaderProperty(materialEditor,"_WaveFrequency");
			ShaderProperty(materialEditor,"_WaveLength");
			ShaderProperty(materialEditor,"_WaveHeight");
			ShaderProperty(materialEditor,"_WaveSpeed");
			ShaderProperty(materialEditor,"_WaveDirection");
			ShaderProperty(materialEditor,"_WaveOffset");
			ShaderProperty(materialEditor,"_WaveStretch");
			ShaderProperty(materialEditor,"_WaveBend");
			EditorStyles.DrawSectionHeader("Foam","foam-icon");
			ShaderProperty(materialEditor,"_Foam");
			if(_materialProperties["_Foam"].prop.floatValue<=0) GUI.enabled=false;				
			ShaderProperty(materialEditor,"_FoamColor");
			ShaderProperty(materialEditor,"_FoamSteps");
			//
			if(_materialProperties["_FoamBlend"].prop!=null){
				_min=_materialProperties["_FoamBlend"].prop.vectorValue.x;
				_max=_materialProperties["_FoamBlend"].prop.vectorValue.y;
				EditorGUILayout.MinMaxSlider(new GUIContent(_materialProperties["_FoamBlend"].title),ref _min, ref _max,0,1);
				_materialProperties["_FoamBlend"].prop.vectorValue=new Vector4(_min,_max,0,0);
			}
			GUI.enabled=true;				
			if(_lightingEnable){
				EditorStyles.DrawSectionHeader("Lighting","light-icon");
				EditorGUI.BeginChangeCheck();
				ShaderProperty(materialEditor,"_Cube");
				if(EditorGUI.EndChangeCheck()){
					SetReflectMap();
				}
				ShaderProperty(materialEditor,"_Reflection");
				ShaderProperty(materialEditor,"_Shininess");
				ShaderProperty(materialEditor,"_SpecColor");
				ShaderProperty(materialEditor,"_ShadowValue");
				ShaderProperty(materialEditor,"_ShadowColor");
			}				

			EditorGUILayout.Space();
			if(_foamEnable != _materialProperties["_Foam"].prop.floatValue){
				SetFoamEnable(_materialProperties["_Foam"].prop.floatValue>0);
				_foamEnable = _materialProperties["_Foam"].prop.floatValue;
			}
		}

		private void SetFoamEnable (bool enable)
		{
			if (!SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
				enable=false;
			}
			if(enable){
				Shader.EnableKeyword ("PIXEL_WATER_FOAM"); 
			}else { 
				Shader.DisableKeyword ("PIXEL_WATER_FOAM");
			}
		}

		private void SetReflectMap ()
		{			
			if(_materialProperties["_Cube"].prop.textureValue != null){
				Shader.EnableKeyword ("PIXEL_WATER_REFLECMAP"); 
			}else { 
				Shader.DisableKeyword ("PIXEL_WATER_REFLECMAP");
			}
		}




	}
}