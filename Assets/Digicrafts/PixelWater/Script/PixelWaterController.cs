using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PixelWaterController : MonoBehaviour {

	// Public properties

	[Range(5,200)]
	public int resolution = 20;
	public float scale 	= 1;
	public int shaderType = 0;
	public Camera viewCamera;
	public Material material;

	// Private
	// #if UNITY_EDITOR
	private int _lastShaderType = 0;
	private int _lastResolution = 0;
	private float _lastScale = 1;
	// #endif

	void Awake() {

		// Check camera
		if(viewCamera==null) {			
			Camera.main.depthTextureMode=DepthTextureMode.Depth;
		} else {			
			viewCamera.depthTextureMode=DepthTextureMode.Depth;
		}

		#if UNITY_EDITOR
		_lastShaderType=shaderType;
		UpdateMesh();
		#endif
	}

	#if UNITY_EDITOR
	// Editor update
	void OnValidate() {	
		Debug.Log("OnValidate");		
		UpdateMesh();
	}

	// Menu Item
	[MenuItem ("GameObject/3D Object/Pixel Water",false)]
	static void CreatePixelWater () {
		GameObject go = new GameObject("PixelWater");
		go.AddComponent<PixelWaterController>();
	}
	#endif

	// Helpers

	private void UpdateMesh()
	{
		#if UNITY_EDITOR

		// Get the MeshFilter or create
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if(meshFilter==null){
			meshFilter=gameObject.AddComponent<MeshFilter>();		
			UnityEditorInternal.ComponentUtility.MoveComponentUp(meshFilter);
		}
		// Create the mesh
		if(meshFilter.sharedMesh==null){			
			meshFilter.sharedMesh = CreateMesh(null,(resolution),10*scale,10*scale);
		} else {
			if(meshFilter.sharedMesh.name=="Plane" || meshFilter.sharedMesh.name=="Cube"){
				meshFilter.sharedMesh = CreateMesh(null,(resolution),10*scale,10*scale);				
			} else {				
				if(_lastResolution!=resolution || _lastScale!=scale){					
					meshFilter.sharedMesh.Clear();
					meshFilter.sharedMesh = CreateMesh(meshFilter.sharedMesh,(resolution),10*scale,10*scale);					
				}
			}
		} 

		// Save last resolution
		_lastResolution=resolution;
		_lastScale=scale;

		// Get or create the renderer
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		if(renderer==null){
			renderer=gameObject.AddComponent<MeshRenderer>();
			UnityEditorInternal.ComponentUtility.MoveComponentUp(renderer);
		}			
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		renderer.receiveShadows = true;

		if(material==null){			
			// Create default material
			if(renderer.sharedMaterial==null){
				Material mat =AssetDatabase.LoadAssetAtPath<Material>("Assets/Digicrafts/PixelWater/Materials/preset-sea-unlit.mat");
				material=renderer.sharedMaterial=mat;
			}			

		} else {
			// Assign back the material
			if(renderer.sharedMaterial!=this.material) 
				renderer.sharedMaterial=this.material;
		}			
			
		if(_lastShaderType!=shaderType){			
			// Update Shader
			if(renderer!=null){
				if(shaderType==0)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Unlit");
				else if(shaderType==1)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Specular");
				else if(shaderType==3)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Reflective");
				else if(shaderType==2)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Specular (One Light)");
				else if(shaderType==4)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Reflective (One Light)");
				else if(shaderType==5)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Unlit(Texture)");
				else if(shaderType==6)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Specular(Texture)");
				else if(shaderType==7)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Reflective(Texture)");
				else if(shaderType==8)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Specular(One Light)(Texture)");
				else if(shaderType==9)
					renderer.sharedMaterial.shader = Shader.Find ("Digicrafts/PixelWater/Reflective(One Light)(Texture)");

			}					
			_lastShaderType=shaderType;
		}			

		renderer.sharedMaterial.SetFloat("_Scale",scale);

		#endif
	}		

	private Mesh CreateMesh(Mesh mesh, int size, float width, float length)
	{

		if(mesh==null) {
			mesh = new Mesh();
			mesh.name="PixelWater";
		}
		if(size<2)size=2;

		int resX = size+1; // 2 minimum
		int resZ = size+1;

		#region Vertices		
		Vector3[] vertices = new Vector3[ resX * resZ ];
		for(int z = 0; z < resZ; z++)
		{
			// [ -length / 2, length / 2 ]
			float zPos = (.5f - (float)z / (resZ - 1)) * length;
			for(int x = 0; x < resX; x++)
			{
				// [ -width / 2, width / 2 ]
				float xPos = (.5f-(float)x / (resX - 1)) * width;
				vertices[ x + z * resX ] = new Vector3( xPos, 0f, zPos );
			}
		}
		#endregion

		#region Normales
		Vector3[] normales = new Vector3[ vertices.Length ];
		for( int n = 0; n < normales.Length; n++ ){
			normales[n] = Vector3.up;
		}
		#endregion

		#region UVs		
		Vector2[] uvs = new Vector2[ vertices.Length ];
		for(int v = 0; v < resZ; v++)
		{
			for(int u = 0; u < resX ; u++)
			{
				uvs[ u + v * resX ] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
			}
		}
		#endregion

		#region Triangles
		int nbFaces = (resX - 1) * (resZ - 1);
		int[] triangles = new int[ nbFaces * 6 ];
		int t = 0;
		for(int face = 0; face < nbFaces; face++ )
		{
			// Retrieve lower left corner from face ind
			int i = face % (resX - 1) + (face / (resZ - 1) * resX);
			triangles[t++] = i + resX + 1;
			triangles[t++] = i + 1;
			triangles[t++] = i;

			triangles[t++] = i + resX;	
			triangles[t++] = i + resX + 1;
			triangles[t++] = i ; 

		}
		#endregion


		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		;

		return mesh;
	}
}
