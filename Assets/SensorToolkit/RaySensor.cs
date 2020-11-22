using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SensorToolkit
{
    /*
     * Detects GameObjects along a ray, it's defined by it's length, which physics layers it detects Objects on and which physics layers obstructs
     * its path. The ray sensor can be queried for the RayCastHit objects associated with each object it detects, so that it's possible to get the
     * point of contact, surface normal etc. As well as this the ray sensor can be queried for the collider that blocked it's path.
     *
     * If the DetectsOnLayers layermask is a subset of the ObstructedByLayers layermask then the ray sensor will use the RayCast method as an
     * optmization. Otherwise it will use the RayCastAll method.
     */
    [ExecuteInEditMode]
	public class RaySensor : Sensor
    {
        // Specified whether the ray sensor will pulse automatically each frame or will be updated manually by having its Pulse() method called when needed.
        public enum UpdateMode { EachFrame, Manual }

        [Tooltip("The detection range in world units.")]
        public float Length = 5f;

        [Tooltip("The radius of the ray, with values above zero the sensor will do a spherecast")]
        public float Radius = 0f;

        [Tooltip("A layermask for colliders that will block the ray sensors path.")]
        public LayerMask ObstructedByLayers;

        [Tooltip("A layermask for colliders that are detected by the ray sensor.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("In Collider mode the sensor detects GameObjects attached to colliders. In RigidBody mode it detects the RigidBody GameObject attached to colliders.")]
        public SensorMode DetectionMode;

        [Tooltip("What direction does the ray sensor point in.")]
        public Vector3 Direction = Vector3.forward;

        [Tooltip("Is the Direction parameter in world space or local space.")]
        public bool WorldSpace = false;

        [Tooltip("Should the sensor pulse each frame automatically or will it be pulsed manually.")]
        public UpdateMode SensorUpdateMode;

        [Tooltip("The initial size of the buffer used when calling Physics.RaycastNonAlloc or Physics.SphereCastNonAlloc.")]
        public int InitialBufferSize = 20;

        [Tooltip("When set true the buffer used with Physics.RaycastNonAlloc is expanded if its not sufficiently large.")]
        public bool DynamicallyIncreaseBufferSize = true;

        public int CurrentBufferSize { get; private set; }

        // Returns a list of all detected GameObjects in no particular order.
		public override List<GameObject> DetectedObjects
        {
            get
            {
                detectedObjects.Clear();
                var detectedEnumerator = detectedObjectsInternal.GetEnumerator();
                while (detectedEnumerator.MoveNext()) 
                {
                    var go = detectedEnumerator.Current;
                    if (go != null && go.activeInHierarchy) {
                        detectedObjects.Add(go);
                    }
                }
                return detectedObjects;
            }
        }

        // Returns a list of all detected GameObjects in order of distance from the sensor. This distance is given by the RaycastHit.dist for each GameObject.
        public override List<GameObject> DetectedObjectsOrderedByDistance { get { return DetectedObjects; } }

        // Returns a list of all RaycastHit objects, each one is associated with a GameObject in the detected objects list.
        public List<RaycastHit> DetectedObjectRayHits { get { return new List<RaycastHit>(detectedObjectHits.Values); } }

        // Returns the Collider that obstructed the ray sensors path, or null if it wasn't obstructed.
        public Collider ObstructedBy { get { return obstructionRayHit.collider; } }

        // Returns the RaycastHit data for the collider that obstructed the rays path.
		public RaycastHit ObstructionRayHit { get { return obstructionRayHit; } }

        // Returns true if the ray sensor is being obstructed and false otherwise
        public bool IsObstructed { get { return isObstructed && ObstructedBy != null; } }

        // Event fired at the time the sensor is obstructed when before it was unobstructed
        [SerializeField]
        public SensorEventHandler OnObstruction;

        // Event fired at the time the sensor is unobstructed when before it was obstructed
        [SerializeField]
        public SensorEventHandler OnClear;

        // Event fired each time the sensor is pulsed. This is used by the editor extension and you shouldn't have to subscribe to it yourself.
        public delegate void SensorUpdateHandler();
        public event SensorUpdateHandler OnSensorUpdate;

        Vector3 direction { get { return WorldSpace ? Direction.normalized : transform.rotation * Direction.normalized; } }
        RayDistanceComparer distanceComparer = new RayDistanceComparer();

        bool isObstructed = false;
		RaycastHit obstructionRayHit;
		Dictionary<GameObject, RaycastHit> detectedObjectHits = new Dictionary<GameObject, RaycastHit>();
        HashSet<GameObject> previousDetectedObjects = new HashSet<GameObject>();
        List<GameObject> detectedObjectsInternal = new List<GameObject>();
        List<GameObject> detectedObjects = new List<GameObject>();
        RaycastHit[] hitsBuffer;

        // Returns true if the passed GameObject appears in the sensors list of detected gameobjects
        public override bool IsDetected(GameObject go)
		{
			return detectedObjectHits.ContainsKey(go);
		}

        // Pulse the ray sensor
        public override void Pulse()
        {
            if (isActiveAndEnabled) testRay();
        }

        // detectedGameObject should be a GameObject that is detected by the sensor. In this case it will return
        // the Raycasthit data associated with this object.
        public RaycastHit GetRayHit(GameObject detectedGameObject)
		{
			RaycastHit val;
            if (!detectedObjectHits.TryGetValue(detectedGameObject, out val))
            {
                Debug.LogWarning("Tried to get the RaycastHit for a GameObject that isn't detected by RaySensor.");
            }
			return val;
		}

        protected override void Awake()
        {
            base.Awake();

            CurrentBufferSize = 0;

            if (OnObstruction == null) 
            {
                OnObstruction = new SensorEventHandler();
            }

            if (OnClear == null) 
            {
                OnClear = new SensorEventHandler();
            }
        }

        void OnEnable()
		{
            clearDetectedObjects();
            previousDetectedObjects.Clear();
        }

        void Update()
        {
            if (Application.isPlaying && SensorUpdateMode == UpdateMode.EachFrame) testRay();
        }

        bool layerMaskIsSubsetOf(LayerMask lm, LayerMask subsetOf)
        {
            return ((lm | subsetOf) & (~subsetOf)) == 0;
        }

		void testRay()
		{
            var canDetectMultiple = !layerMaskIsSubsetOf(DetectsOnLayers, ObstructedByLayers) && (IgnoreList == null || IgnoreList.Count == 0);
            clearDetectedObjects();
            if (!canDetectMultiple && (IgnoreList == null || IgnoreList.Count == 0))
            {
                testRaySingle();
            }
            else
            {
                testRayMulti();
            }

            obstructionEvents();
            detectionEvents();

            if (OnSensorUpdate != null) OnSensorUpdate();
		}

        void obstructionEvents()
        {
            if (isObstructed && obstructionRayHit.collider == null)
            {
                isObstructed = false;
                OnClear.Invoke(this);
            }
            else if (!isObstructed && obstructionRayHit.collider != null)
            {
                isObstructed = true;
                OnObstruction.Invoke(this);
            }
        }

        void detectionEvents()
        {
            // Any GameObjects still in previousDetectedObjects are no longer detected
            var lostDetectionEnumerator = previousDetectedObjects.GetEnumerator();
            while (lostDetectionEnumerator.MoveNext())
            {
                OnLostDetection.Invoke(lostDetectionEnumerator.Current, this);
            }

            previousDetectedObjects.Clear();
            for (int i = 0; i < detectedObjectsInternal.Count; i++)
            {
                previousDetectedObjects.Add(detectedObjectsInternal[i]);
            }
        }

        void testRaySingle()
        {
            Ray ray = new Ray(transform.position, direction);
			RaycastHit hit;
            if (Radius > 0) 
            {
                if (Physics.SphereCast(ray, Radius, out hit, Length, ObstructedByLayers)) 
                {
                    if ((1 << hit.collider.gameObject.layer & DetectsOnLayers) != 0) 
                    {
                        addRayHit(hit);
                    }
                    obstructionRayHit = hit;
                }
            } 
            else
            {
                if (Physics.Raycast(ray, out hit, Length, ObstructedByLayers)) 
                {
                    if ((1 << hit.collider.gameObject.layer & DetectsOnLayers) != 0) 
                    {
                        addRayHit(hit);
                    }
                    obstructionRayHit = hit;
                }
            }
        }

		void testRayMulti()
		{
			Ray ray = new Ray(transform.position, direction);
			LayerMask combinedLayers = DetectsOnLayers | ObstructedByLayers;
            RaycastHit[] hits;
            int numberOfHits;
            
            if (Radius > 0) 
            {
                prepareHitsBuffer();
                hits = hitsBuffer;
                numberOfHits = Physics.SphereCastNonAlloc(ray, Radius, hits, Length, combinedLayers);
                if (numberOfHits == CurrentBufferSize)
                {
                    if (DynamicallyIncreaseBufferSize)
                    {
                        CurrentBufferSize *= 2;
                        testRayMulti();
                        return;
                    }
                    else
                    {
                        logInsufficientBufferSize();
                    }
                }
            }
            else
            {
                prepareHitsBuffer();
                hits = hitsBuffer;
                numberOfHits = Physics.RaycastNonAlloc(ray, hits, Length, combinedLayers);
                if (numberOfHits == CurrentBufferSize)
                {
                    if (DynamicallyIncreaseBufferSize)
                    {
                        CurrentBufferSize *= 2;
                        testRayMulti();
                        return;
                    } 
                    else
                    {
                        logInsufficientBufferSize();
                    }
                }
            }

            System.Array.Sort(hits, 0, numberOfHits, distanceComparer);

            for (int i = 0; i < numberOfHits; i++)
            {
                var hit = hits[i];
                if ((1 << hit.collider.gameObject.layer & DetectsOnLayers) != 0)
                {
                    addRayHit(hit);
                }
                if ((1 << hit.collider.gameObject.layer & ObstructedByLayers) != 0)
                {
                    // Potentially blocks the ray, just make sure it isn't in the ignore list
                    if (shouldIgnore(hit.collider.gameObject)
                        || hit.rigidbody != null
                        && shouldIgnore(hit.rigidbody.gameObject))
                    {
                        // Obstructing collider or its rigid body is in the ignore list
                        continue;
                    }
                    else
                    {
                        obstructionRayHit = hit;
                        break;
                    }
                }
            }
        }

        void logInsufficientBufferSize()
        {
            Debug.LogWarning("A ray sensor on " + name + " has an insufficient buffer size. Some objects may not be detected");
        }

        void prepareHitsBuffer()
        {
            if (CurrentBufferSize == 0)
            {
                InitialBufferSize = Math.Max(1, InitialBufferSize);
                CurrentBufferSize = InitialBufferSize;
            }
            if (hitsBuffer == null || hitsBuffer.Length != CurrentBufferSize)
            {
                hitsBuffer = new RaycastHit[CurrentBufferSize];
            }
        }

		void addRayHit(RaycastHit hit)
		{
            GameObject go;
            if (DetectionMode == SensorMode.RigidBodies)
            {
                if (hit.rigidbody == null) return;
                go = hit.rigidbody.gameObject;
            }
            else
            {
                go = hit.collider.gameObject;
            }
			if (!detectedObjectHits.ContainsKey(go) && !shouldIgnore(go))
			{
				detectedObjectHits.Add(go, hit);
                detectedObjectsInternal.Add(go);
                if (!previousDetectedObjects.Contains(go))
                {
                    OnDetected.Invoke(go, this);
                }
                else
                {
                    previousDetectedObjects.Remove(go);
                }
			}
		}

        void clearDetectedObjects()
		{
			obstructionRayHit = new RaycastHit();
			detectedObjectHits.Clear();
            detectedObjectsInternal.Clear();
            detectedObjects.Clear();
        }

        // Called by the RaySensorEditor in a SendMessage
        void reset() 
        {
            clearDetectedObjects();
            isObstructed = false;
            CurrentBufferSize = 0;
        }

        class RayDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                if (x.distance < y.distance) { return -1; }
                else if (x.distance > y.distance) { return 1; }
                else { return 0; }
            }
        }

        protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
		protected static readonly Color GizmoBlockedColor = Color.red;
        private static Mesh primitiveCylinderCache;
        private static Mesh primitiveCylinder 
        {
            get 
            {
                if (primitiveCylinderCache == null) 
                {
                    var primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    primitiveCylinderCache = primitive.GetComponent<MeshFilter>().sharedMesh;
                    DestroyImmediate(primitive);
                } 
                
                return primitiveCylinderCache;
            }
        }
        public void OnDrawGizmosSelected()
        {
            if (!isActiveAndEnabled) return;

            Vector3 endPosition;
            if (IsObstructed)
            {
                Gizmos.color = GizmoBlockedColor;
                endPosition = transform.position + direction * obstructionRayHit.distance;
            }
            else
            {
                Gizmos.color = GizmoColor;
                endPosition = transform.position + direction * Length;
            }

            if (Radius > 0f) 
            {
                Gizmos.DrawWireSphere(transform.position, Radius);
                Gizmos.DrawWireSphere(endPosition, Radius);

                var line = endPosition - transform.position;
                if (line == Vector3.zero) 
                {
                    line = Vector3.forward * Length;
                }
                var center = transform.position + line / 2f;
                var length = line.magnitude;
                var rotation = Quaternion.LookRotation(line.normalized) * Quaternion.Euler(90f, 0f, 0f);
                Gizmos.DrawWireMesh(primitiveCylinder, center, rotation, new Vector3(Radius * 2f, length / 2f, Radius * 2f));
            } 
            else 
            {
                Gizmos.DrawLine(transform.position, endPosition);
            }

            Gizmos.color = GizmoColor;
            foreach(RaycastHit hit in DetectedObjectRayHits)
            {
                Gizmos.DrawIcon(hit.point, "SensorToolkit/eye.png", true);
            }
        }
    }
}