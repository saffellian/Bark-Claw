using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Digicrafts.PixelWater
{
	public class EditorProperty
	{
		public string label;
		public SerializedProperty serialProperty;
		public GUIContent labelContent;

		public EditorProperty(string label)
		{
			this.label=label;
		}
	}

	public class EditorStyles 
	{

		public static GUIStyle logo;
		public static GUIStyle logoTitle;
		public static GUIStyle sectionTitle;

		public static Texture2D logoImage;
		public static Dictionary<string,Texture2D> icons;

		public static void initImages(string path){
			if(logoImage==null){				
				logoImage=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"logo.png");
				string[] iconsName = new string[]{"pattern-icon","ripple-icon","wave-icon","foam-icon","light-icon"};
				icons = new Dictionary<string,Texture2D>();
				foreach(string name in iconsName){
					icons.Add(name, AssetDatabase.LoadAssetAtPath<Texture2D>(path+name+".png"));
				}
			}
		}

		public static void init(){

			if(logo==null){

				// Logo Title
				logoTitle = new GUIStyle(GUI.skin.GetStyle("Label"));
				logoTitle.fontSize=25;

				//logo
				logo = new GUIStyle(GUI.skin.GetStyle("Label"));
				logo.alignment = TextAnchor.UpperRight;
				logo.stretchWidth=false;
				logo.stretchHeight=false;
				logo.normal.background=logoImage;

				//sectionTitle
				sectionTitle =  new GUIStyle(GUI.skin.GetStyle("Label"));
				sectionTitle.fontStyle=FontStyle.Bold;
				sectionTitle.fontSize=14;
			}
		}

		public static void DrawHeader(string title)
		{
			EditorStyles.initImages("Assets/Digicrafts/PixelWater/Editor/");
			EditorStyles.init();

			/////////////////////////////////////////////////////////////////////////////////
			//// Logo
			EditorGUILayout.Space();
			//			EditorGUILayout.BeginHorizontal();//IAPEditorStyles.logoBackground
			//			EditorGUILayout.LabelField(title,ShaderEditorStyles.logoTitle,GUILayout.Height(40));
			EditorGUILayout.LabelField("",EditorStyles.logo,GUILayout.Height(50));
			//			EditorGUILayout.EndHorizontal();
			//			GUILayout.Box(GUIContent.none,GUILayout.ExpandWidth(true),GUILayout.Height(1));
			EditorGUILayout.Space();
			//// Logo
			/// /////////////////////////////////////////////////////////////////////////////////
		}

		public static void DrawHorizontalLine()
		{
			GUILayout.Box(GUIContent.none,GUILayout.ExpandWidth(true),GUILayout.Height(1));		
		}

		public static void DrawSectionHeader(string title, string icon = null)
		{
			if(EditorStyles.sectionTitle!=null){
			Texture2D iconImage = null;
			if(icon!=null && icons!=null && icons.ContainsKey(icon)) iconImage=icons[icon];
			EditorGUILayout.Space ();
			// Debug.Log("title");
			// Debug.Log(title);
			// Debug.Log("titleEditorStyles");
			// Debug.Log(EditorStyles.sectionTitle);
			if(iconImage==null)
				EditorGUILayout.LabelField(title,EditorStyles.sectionTitle,GUILayout.Height(20));		
			else
				EditorGUILayout.LabelField(new GUIContent(title,iconImage),EditorStyles.sectionTitle,GUILayout.Height(20));		

			DrawHorizontalLine();
			EditorGUILayout.Space ();
			}
		}
	}

	[CustomEditor(typeof(PixelWaterController))]
	public class PixelWaterEditor : UnityEditor.Editor {

		private static string[] ShaderType = new string[]{
			"Unlit","Specular","Specular (One Light)","Reflective","Reflective (One Light)",
			"Unlit(Texture)","Specular(Texture)","Specular(One Light)(Texture)","Reflective(Texture)","Reflective(One Light)(Texture)"};

		private static Dictionary<string,EditorProperty> _properties = new Dictionary<string, EditorProperty>(){
			{"viewCamera",new EditorProperty("View Camera")},
			{"material",new EditorProperty("Material")},
			{"resolution",new EditorProperty("Mesh Resolution")},
			{"scale",new EditorProperty("Scale")},
			{"shaderType",new EditorProperty("ShaderType")}
		};

		void OnEnable()
		{
			// Loop and find property
			foreach(KeyValuePair<string,EditorProperty> kvp in _properties){
				kvp.Value.serialProperty=serializedObject.FindProperty(kvp.Key);
				kvp.Value.labelContent=new GUIContent(kvp.Value.label);
			}
		}

		void PropertyField(string name)
		{
			if(_properties[name]!=null)
				EditorGUILayout.PropertyField(_properties[name].serialProperty,_properties[name].labelContent);
		}

		public override void OnInspectorGUI()
		{
			//Update SerialObject
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			EditorStyles.DrawHeader("");
			PropertyField("viewCamera");
			PropertyField("resolution");
			EditorGUILayout.PropertyField(_properties["scale"].serialProperty);
			PropertyField("material");
			_properties["shaderType"].serialProperty.intValue=EditorGUILayout.Popup(
				_properties["shaderType"].label,
				_properties["shaderType"].serialProperty.intValue,
				ShaderType);

			//Apply the changes to our list
			if(EditorGUI.EndChangeCheck()){				
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}