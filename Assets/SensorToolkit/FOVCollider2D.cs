using UnityEngine;
using System.Collections;

namespace SensorToolkit
{
    /*
     * A paramemtric shape for creating field of view cones that work with the trigger sensor. Requires a PolygonCollider2D
     * component on the same gameobject. When the script starts it will dynamically create a mesh for the fov cone and
     * assign it to this PolygonCollider2D component.
     */
    [RequireComponent(typeof(PolygonCollider2D))]
    [ExecuteInEditMode]
    public class FOVCollider2D : MonoBehaviour
    {
        [Tooltip("The length of the field of view cone in world units.")]
        public float Length = 5f;

        [Tooltip("The size of the field of view cones base in world units.")]
        public float BaseSize = 0.5f;

        [Range(1f, 360f), Tooltip("The arc angle of the fov cone.")]
        public float FOVAngle = 90f;

        [Range(0, 16), Tooltip("The number of vertices used to approximate the arc of the fov cone. Ideally this should be as low as possible.")]
        public int Resolution = 0;

        // Returns the generated collider mesh so that it can be rendered.
        public Mesh FOVMesh { get { return mesh; } }

        PolygonCollider2D pc;
        Vector2[] pts;
        Mesh mesh;

        void Awake()
        {
            pc = GetComponent<PolygonCollider2D>();
            CreateCollider();
        }

        void OnValidate()
        {
            Length = Mathf.Max(0f, Length);
            BaseSize = Mathf.Max(0f, BaseSize);
            if (pc != null) 
            {
                CreateCollider();
            }
        }

        public void CreateCollider()
        {
            pts = new Vector2[4 + Resolution];

            // Base points
            pts[0] = new Vector3(-BaseSize / 2f, 0f); // Bottom Left
            pts[1] = new Vector3(BaseSize / 2f, 0f);  // Bottom Right

            for (int i = 0; i <= 1+Resolution; i++)
            {
                float a = -FOVAngle / 2f + FOVAngle * ((float)i / (1 + Resolution));
                Vector2 pt = Quaternion.AngleAxis(a, Vector3.forward) * (Vector2.up * Length);
                pts[i + 2] = pt;
            }

            pc.points = pts;

            // Create a mesh, in case we want to render fov cone
            var pts3D = new Vector3[4 + Resolution];
            for (int i = 0; i < pts3D.Length; i++)
            {
                pts3D[i] = pts[i];
            }

            var triangles = new int[(2 + Resolution) * 3];
            for (int i = 0; i < (2+Resolution); i++)
            {
                var ti = i * 3;
                if (i == 0)
                {
                    triangles[ti] = 1;
                    triangles[ti + 1] = 0;
                    triangles[ti + 2] = 2;
                }
                else
                {
                    triangles[ti] = i+1;
                    triangles[ti + 1] = 0;
                    triangles[ti + 2] = i + 2;
                }
            }

            mesh = new Mesh();
            mesh.vertices = pts3D;
            mesh.triangles = triangles;
            mesh.name = "FOV2DColliderPoints";
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach(Vector3 p in pts)
            {
                Gizmos.DrawSphere(transform.TransformPoint(p), 0.1f);
            }
        }
    }
}