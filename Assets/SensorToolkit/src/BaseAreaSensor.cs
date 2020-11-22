using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SensorToolkit
{
    /*
     * Common functionality for 2D sensors that detect colliders within an area. Sensors that implement this class should
     * detect colliders and pass those colliders to this base implementation through the addCollider protected method. When
     * colliders are no longer detected then a corresponding call to removeCollider should be made. This class manages
     * the list of detected objects by processing the list of detected colliders.
     *
     * Includes optional support for line of sight testing, when enabled a GameObject will only appear as detected if
     * it passes a line of sight test. To calculate line of sight the sensor casts a number of rays towards the target
     * object. The ratio of these test rays that reach the object unobstructed is it's computed visibility. If the visibility
     * ratio is greater then a user specified amount then the object appears in the detected list. These target points can
     * be specified by adding a LOSTargets component to the object, or else they are randomly generated. See the documentation 
     * at www.micosmo.com/sensortoolkit/#rangesensor for more info.
     * 
     * If line of sight testing is enabled then it will need to be continually refreshed by calls to the refreshLineOfSight()
     * protected method. Ideally this should happen during a pulse.
     */
    [ExecuteInEditMode]
    public abstract class BaseAreaSensor : Sensor
    {
        [Tooltip("In Collider mode the sensor detects GameObjects attached to colliders. In RigidBody mode it detects the RigidBody GameObject attached to colliders.")]
        public SensorMode DetectionMode;

        [Tooltip("GameObjects are only detected if they pass a line of sight test.")]
        public bool RequiresLineOfSight;

        [Tooltip("Line of Sight rays will be tested against this layer mask.")]
        public LayerMask BlocksLineOfSight;

        [Tooltip("If true will only perform line of sight tests on objects with a LOSTargets component, if false then the sensor will auto-generate test points for object which don't have this component.")]
        public bool TestLOSTargetsOnly;

        [Range(1, 20), Tooltip("Number of test points the Sensor will generate on objects that don't have a LOSTargets component.")]
        public int NumberOfRays = 1;

        [Range(0f, 1f), Tooltip("Minimum visibility an object must be for it to be detected.")]
        public float MinimumVisibility = 0.5f;

        // Displays the results of line of sight tests during OnDrawGizmosSelected for objects in this set.
        public HashSet<GameObject> ShowRayCastDebug;

        // Returns a list of all GameObjects detected by this sensor.
        public override List<GameObject> DetectedObjects
        {
            get
            {
                detectedObjects.Clear();

                if (RequiresLineOfSight)
                {
                    var objectVisibilityEnumerator = objectVisibility.GetEnumerator();
                    while (objectVisibilityEnumerator.MoveNext())
                    {
                        var current = objectVisibilityEnumerator.Current;
                        var go = current.Key;
                        if (go != null && go.activeInHierarchy && !shouldIgnore(go) && current.Value >= MinimumVisibility)
                        {
                            detectedObjects.Add(go);
                        }
                    }
                }
                else if (DetectionMode == SensorMode.RigidBodies)
                {
                    var rigidBodyCollidersEnumerator = rigidBodyColliders.GetEnumerator();
                    while (rigidBodyCollidersEnumerator.MoveNext())
                    {
                        var go = rigidBodyCollidersEnumerator.Current.Key;
                        if (go != null && go.activeInHierarchy && !shouldIgnore(go))
                        {
                            detectedObjects.Add(go);
                        }
                    }
                }
                else
                {
                    var gameObjectCollidersEnumerator = gameObjectColliders.GetEnumerator();
                    while (gameObjectCollidersEnumerator.MoveNext())
                    {
                        var go = gameObjectCollidersEnumerator.Current.Key;
                        if (go != null && go.activeInHierarchy && !shouldIgnore(go))
                        {
                            detectedObjects.Add(go);
                        }
                    }
                }

                return detectedObjects;
            }
        }

        // Returns a list of all GameObjects detected by sensor and ordered by their distance from the sensor.
        // Bit of a hack here, don't get nested enumerators from this property.
        public override List<GameObject> DetectedObjectsOrderedByDistance
        {
            get
            {
                detectedObjectsOrderedByDistance.Clear();
                detectedObjectsOrderedByDistance.AddRange(DetectedObjects);
                distanceComparer.Point = transform.position;
                detectedObjectsOrderedByDistance.Sort(distanceComparer);
                return detectedObjectsOrderedByDistance;
            }
        }

        // Returns a map of every sensed object and their computed visibility. This includes objects whose
        // visibility is too low to be detected.
        public Dictionary<GameObject, float> ObjectVisibilities
        {
            get
            {
                return objectVisibility;
            }
        }

        // Returns the visibility between 0-1 of the specified object. A 0 means its not visible at all while
        // a 1 means it is entirely visible. Visibility only makes sense in the context of line of sight tests,
        // it is the ratio of rays towards the target object that are clear of obstructions.
        public override float GetVisibility(GameObject go)
        {
            if (!RequiresLineOfSight) return base.GetVisibility(go);
            else if (objectVisibility.ContainsKey(go)) return objectVisibility[go];
            else return 0f;
        }

        // Returns a list of transforms on the given object that passed line of sight tests. Will only return
        // results for objects that have a LOSTargets component.
        public List<Transform> GetVisibleTransforms(GameObject go)
        {
            RayCastTargets targets;
            if (go != null && rayCastTargets.TryGetValue(go, out targets))
            {
                return targets.GetVisibleTransforms();
            }
            else
            {
                return new List<Transform>();
            }
        }

        // Returns a list of positions on a given object that passed line of sight tests.
        public List<Vector2> GetVisiblePositions(GameObject go)
        {
            RayCastTargets targets;
            if (go != null && rayCastTargets.TryGetValue(go, out targets))
            {
                return targets.GetVisibleTargetPositions();
            }
            else
            {
                return new List<Vector2>();
            }
        }
        
        // Maps a GameObject to a list of it's colliders that have been detected.
        Dictionary<GameObject, List<Collider2D>> gameObjectColliders = new Dictionary<GameObject, List<Collider2D>>();

        // Maps a RigidBody GameObject to a list of it's colliders that have been detected. These colliders
        // may be attached to children GameObjects.
        Dictionary<GameObject, List<Collider2D>> rigidBodyColliders = new Dictionary<GameObject, List<Collider2D>>();

        // Collider.attachedRigidBody is null if the gameobject is deactivated. So store it when its first detected
        Dictionary<Collider2D, GameObject> attachedRigidBody = new Dictionary<Collider2D, GameObject>();

        // Maps a GameObject to a list of raycast target positions for calculating line of sight
        Dictionary<GameObject, RayCastTargets> rayCastTargets = new Dictionary<GameObject, RayCastTargets>();

        // Maps a detected object to its computed visibility from line of sight tests
        Dictionary<GameObject, float> objectVisibility = new Dictionary<GameObject, float>();

        // A list of results from all the raycast tests
        List<RayCastResult> raycastResults = new List<RayCastResult>();

        // List of temporary values for modifying collections
        List<GameObject> gameObjectList = new List<GameObject>();

        List<GameObject> detectedObjects = new List<GameObject>();

        List<GameObject> detectedObjectsOrderedByDistance = new List<GameObject>();

        DistanceFromPointComparer distanceComparer = new DistanceFromPointComparer();

        int prevNumberOfRays = 0;

        protected static ListCache<Collider2D> colliderListCache = new ListCache<Collider2D>();
        protected static ListCache<Vector2> vector2ListCache = new ListCache<Vector2>();
        class RayCastTargetsCache : ObjectCache<RayCastTargets>
        {
            public override void Dispose(RayCastTargets obj)
            {
                obj.dispose();
                base.Dispose(obj);
            }
        }
        static RayCastTargetsCache rayCastTargetsCache = new RayCastTargetsCache();

        struct RayCastResult
        {
            public GameObject go;
            public Vector2 testPoint;
            public Vector2 obstructionPoint;
            public bool isObstructed;
        }

        class RayCastTargets
        {
            GameObject go;
            Transform[] targetTransforms;
            List<Vector2> targetPoints;
            List<Vector2> returnPoints;
            List<bool> isTargetVisible;
            public RayCastTargets()
            {
                returnPoints = new List<Vector2>();
                isTargetVisible = new List<bool>();
            }

            public bool IsTransforms()
            {
                return targetTransforms != null;
            }

            public void Set(GameObject go, Transform[] targets)
            {
                this.go = go;
                targetTransforms = targets;
                targetPoints = null;
                isTargetVisible.Clear(); for (int i = 0; i < targets.Length; i++) isTargetVisible.Add(false);
            }

            public void Set(GameObject go, List<Vector2> targets)
            {
                this.go = go;
                targetTransforms = null;
                targetPoints = targets;
                isTargetVisible.Clear(); for (int i = 0; i < targets.Count; i++) isTargetVisible.Add(false);
            }

            public List<Transform> GetVisibleTransforms()
            {
                var visibleList = new List<Transform>();
                for (int i = 0; i < isTargetVisible.Count; i++)
                {
                    if (isTargetVisible[i]) visibleList.Add(targetTransforms[i]);
                }
                return visibleList;
            }

            public List<Vector2> GetVisibleTargetPositions()
            {
                var visibleList = new List<Vector2>();
                if (targetTransforms != null)
                {
                    for (int i = 0; i < isTargetVisible.Count; i++)
                    {
                        if (isTargetVisible[i]) visibleList.Add(targetTransforms[i].position);
                    }
                }
                else
                {
                    for (int i = 0; i < isTargetVisible.Count; i++)
                    {
                        if (isTargetVisible[i]) visibleList.Add(go.transform.TransformPoint(targetPoints[i]));
                    }
                }
                return visibleList;
            }

            public void SetIsTargetVisible(int i, bool isVisible)
            {
                isTargetVisible[i] = isVisible;
            }

            public List<Vector2> getTargetPoints()
            {
                returnPoints.Clear();
                if (targetTransforms != null)
                {
                    for (int i = 0; i < targetTransforms.Length; i++)
                    {
                        returnPoints.Add(targetTransforms[i].position);
                    }
                }
                else
                {
                    var go = this.go;
                    for (int i = 0; i < targetPoints.Count; i++)
                    {
                        returnPoints.Add(go.transform.TransformPoint(targetPoints[i]));
                    }
                }
                return returnPoints;
            }

            public void dispose()
            {
                if (targetPoints != null) { vector2ListCache.Dispose(targetPoints); }
            }
        }

        protected virtual void OnEnable()
        {
            clearColliders();
            clearRayCastTargets();
            objectVisibility.Clear();
            raycastResults.Clear();
            gameObjectList.Clear();
        }

        protected GameObject addCollider(Collider2D c)
        {
            GameObject newColliderDetection = null;
            GameObject newRigidBodyDetection = null;

            if (addColliderToMap(c, c.gameObject, gameObjectColliders))
            {
                disposeRayCastTarget(c.gameObject);
                newColliderDetection = c.gameObject;
            }
            if (c.attachedRigidbody != null) 
            {
                attachedRigidBody[c] = c.attachedRigidbody.gameObject;

                if (addColliderToMap(c, c.attachedRigidbody.gameObject, rigidBodyColliders)) 
                {
                    disposeRayCastTarget(c.attachedRigidbody.gameObject);
                    newRigidBodyDetection = c.attachedRigidbody.gameObject;
                }
            }

            var newDetection = DetectionMode == SensorMode.Colliders ? newColliderDetection : newRigidBodyDetection;
            if (shouldIgnore(newDetection))
            {
                return null;
            }
            else if (RequiresLineOfSight && newDetection != null)
            {
                bool prevDetected = objectVisibility.ContainsKey(newDetection) && objectVisibility[newDetection] >= MinimumVisibility;
                var targets = getRayCastTargets(newDetection);
                if (TestLOSTargetsOnly && !targets.IsTransforms()) return null;
                objectVisibility[newDetection] = testObjectVisibility(newDetection, targets);

                if (!prevDetected && objectVisibility[newDetection] >= MinimumVisibility) return newDetection;
                else return null;
            }
            else
            {
                return newDetection;
            }
        }

        protected GameObject removeCollider(Collider2D c)
        {
            if (c == null)
            {
                clearDestroyedGameObjects();
                return null;
            }

            GameObject colliderDetectionLost = null;
            GameObject rigidBodyDetectionLost = null;

            if (removeColliderFromMap(c, c.gameObject, gameObjectColliders))
            {
                disposeRayCastTarget(c.gameObject);
                colliderDetectionLost = c.gameObject;
            }

            GameObject rigidBody;
            if (attachedRigidBody.TryGetValue(c, out rigidBody)) 
            {
                if (removeColliderFromMap(c, rigidBody, rigidBodyColliders)) 
                {
                    disposeRayCastTarget(rigidBody);
                    rigidBodyDetectionLost = rigidBody;
                }

                attachedRigidBody.Remove(c);
            }

            var detectionLost = DetectionMode == SensorMode.Colliders ? colliderDetectionLost : rigidBodyDetectionLost;
            if (shouldIgnore(detectionLost))
            {
                return null;
            }
            else if (RequiresLineOfSight && detectionLost != null)
            {
                float visibility;
                if (objectVisibility.TryGetValue(detectionLost, out visibility)) {
                    objectVisibility.Remove(detectionLost);
                    if (visibility >= MinimumVisibility) {
                        return detectionLost;
                    }
                }
                return null;
            }
            else
            {
                return detectionLost;
            }
        }

        protected void clearDestroyedGameObjects()
        {
            gameObjectList.Clear();
            var collidersGameObjectsEnumerator = gameObjectColliders.GetEnumerator();
            while (collidersGameObjectsEnumerator.MoveNext())
            {
                var go = collidersGameObjectsEnumerator.Current.Key;
                if (go == null) { gameObjectList.Add(go); }
            }
            for (int i = 0; i < gameObjectList.Count; i++)
            {
                gameObjectColliders.Remove(gameObjectList[i]);
            }

            gameObjectList.Clear();
            var rigidBodyGameObjectsEnumerator = rigidBodyColliders.GetEnumerator();
            while (rigidBodyGameObjectsEnumerator.MoveNext())
            {
                var go = rigidBodyGameObjectsEnumerator.Current.Key;
                if (go == null) { gameObjectList.Add(go); }
            }
            for (int i = 0; i < gameObjectList.Count; i++)
            {
                var colliderList = rigidBodyColliders[gameObjectList[i]];
                for (int j = 0; j < colliderList.Count; j++) 
                {
                    attachedRigidBody.Remove(colliderList[j]);
                }
                rigidBodyColliders.Remove(gameObjectList[i]);
            }
        }

        protected void clearColliders()
        {
            var collidersEnumerator = gameObjectColliders.GetEnumerator();
            while (collidersEnumerator.MoveNext())
            {
                var colliderList = collidersEnumerator.Current.Value;
                colliderListCache.Dispose(colliderList);
            }
            gameObjectColliders.Clear();

            collidersEnumerator = rigidBodyColliders.GetEnumerator();
            while (collidersEnumerator.MoveNext())
            {
                var colliderList = collidersEnumerator.Current.Value;
                colliderListCache.Dispose(colliderList);
            }
            rigidBodyColliders.Clear();

            attachedRigidBody.Clear();

            clearLineOfSight();
        }

        protected void clearLineOfSight()
        {
            objectVisibility.Clear();
            raycastResults.Clear();
        }

        protected void refreshLineOfSight()
        {
            if (prevNumberOfRays != NumberOfRays)
            {
                prevNumberOfRays = NumberOfRays;
                clearRayCastTargets();
            }

            objectVisibility.Clear();
            raycastResults.Clear();
            if (DetectionMode == SensorMode.RigidBodies)
            {
                var gosEnumerator = rigidBodyColliders.GetEnumerator();
                while (gosEnumerator.MoveNext())
                {
                    var go = gosEnumerator.Current.Key;
                    if (go == null || shouldIgnore(go)) continue;
                    var targets = getRayCastTargets(go);
                    if (TestLOSTargetsOnly && !targets.IsTransforms()) continue;
                    objectVisibility[go] = testObjectVisibility(go, targets);
                }
            }
            else
            {
                var gosEnumerator = gameObjectColliders.GetEnumerator();
                while (gosEnumerator.MoveNext())
                {
                    var go = gosEnumerator.Current.Key;
                    if (go == null || shouldIgnore(go)) continue;
                    var targets = getRayCastTargets(go);
                    if (TestLOSTargetsOnly && !targets.IsTransforms()) continue;
                    objectVisibility[go] = testObjectVisibility(go, targets);
                }
            }
        }

        float testObjectVisibility(GameObject go, RayCastTargets targets)
        {
            int nSuccess = 0;
            var rayCastTargets = getRayCastTargets(go);
            List<Vector2> testPoints = rayCastTargets.getTargetPoints();
            for (int i = 0; i < testPoints.Count; i++)
            {
                var testPoint = testPoints[i];
                Vector2 obstructionPoint;
                var result = new RayCastResult();
                result.go = go;
                result.testPoint = testPoint;
                result.isObstructed = false;
                if (isInLineOfSight(go, testPoint, out obstructionPoint))
                {
                    nSuccess++;
                    rayCastTargets.SetIsTargetVisible(i, true);
                }
                else
                {
                    result.isObstructed = true;
                    result.obstructionPoint = obstructionPoint;
                    rayCastTargets.SetIsTargetVisible(i, false);
                }
                raycastResults.Add(result);
            }

            return nSuccess / (float)testPoints.Count;
        }

        RayCastTargets getRayCastTargets(GameObject go)
        {
            RayCastTargets rts;
            if (rayCastTargets.TryGetValue(go, out rts))
            {
                return rts;
            }
            else
            {
                var losTargets = go.GetComponent<LOSTargets>();
                rts = rayCastTargetsCache.Get();
                if (losTargets != null && losTargets.Targets != null)
                {
                    rts.Set(go, losTargets.Targets);
                    rayCastTargets.Add(go, rts);
                    return rts;
                }
                else
                {
                    rts.Set(go, generateRayCastTargets(go));
                    rayCastTargets.Add(go, rts);
                    return rts;
                }
            }
        }

        List<Vector2> generateRayCastTargets(GameObject go)
        {
            List<Collider2D> cs;
            if (DetectionMode == SensorMode.Colliders) cs = gameObjectColliders[go];
            else cs = rigidBodyColliders[go];

            List<Vector2> rts = vector2ListCache.Get();
            if (NumberOfRays == 1)
            {
                rts.Add(getCentreOfColliders(go, cs));
            }
            else
            {
                for (int i = 0; i < NumberOfRays; i++)
                {
                    rts.Add(getRandomPointInColliders(go, cs));
                }
            }
            return rts;
        }

        bool isInLineOfSight(GameObject go, Vector2 testPoint, out Vector2 obstructionPoint)
        {
            obstructionPoint = Vector3.zero;
            var toGoCentre = testPoint - (Vector2)transform.position;

            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, toGoCentre.normalized, toGoCentre.magnitude, BlocksLineOfSight);
            if (hitInfo.collider != null)
            {
                // Ray hit something, check that it was the target.
                if (DetectionMode == SensorMode.RigidBodies && hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == go)
                {
                    return true;
                }
                else if (DetectionMode == SensorMode.Colliders && hitInfo.collider.gameObject == go)
                {
                    return true;
                }
                else
                {
                    obstructionPoint = hitInfo.point;
                    return false;
                }
            }
            else
            {
                // Ray didn't hit anything so assume target is in line of sight
                return true;
            }
        }

        Vector2 getCentreOfColliders(GameObject goRoot, List<Collider2D> goColliders)
        {
            Vector2 aggregate = Vector2.zero;
            for (int i = 0; i < goColliders.Count; i++)
            {
                var c = goColliders[i];
                aggregate += (Vector2)c.bounds.center - (Vector2)goRoot.transform.position;
            }
            return aggregate / goColliders.Count;
        }

        Vector2 getRandomPointInColliders(GameObject goRoot, List<Collider2D> colliders)
        {
            // Choose a random collider weighted by its volume
            Collider2D rc = colliders[0];
            var totalArea = 0f;
            for (int i = 0; i < colliders.Count; i++)
            {
                var c = colliders[i];
                totalArea += c.bounds.size.x * c.bounds.size.y;
            }

            var r = Random.Range(0f, 1f);
            for (int i = 0; i < colliders.Count; i++)
            {
                var c = colliders[i];
                rc = c;
                var v = c.bounds.size.x * c.bounds.size.y;
                r -= v / totalArea;
                if (r <= 0f) break;
            }

            // Now choose a random point within that random collider and return it
            var rp = new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
            rp.Scale(rc.bounds.size);
            rp += (Vector2)(rc.bounds.center - goRoot.transform.position);
            return rp;
        }

        bool addColliderToMap(Collider2D c, GameObject go, Dictionary<GameObject, List<Collider2D>> dict)
        {
            var newDetection = false;
            List<Collider2D> colliderList;
            if (!dict.TryGetValue(go, out colliderList))
            {
                newDetection = true;
                colliderList = colliderListCache.Get();
                dict[go] = colliderList;
            }
            if (!colliderList.Contains(c)) {
                colliderList.Add(c);
            }
            return newDetection;
        }

        bool removeColliderFromMap(Collider2D c, GameObject go, Dictionary<GameObject, List<Collider2D>> dict)
        {
            var detectionLost = false;
            List<Collider2D> colliderList;
            if (dict.TryGetValue(go, out colliderList))
            {
                colliderList.Remove(c);
                if (colliderList.Count == 0)
                {
                    detectionLost = true;
                    dict.Remove(go);
                    colliderListCache.Dispose(colliderList);
                }
            }
            return detectionLost;
        }

        void clearRayCastTargets() 
        {
            var rayCastTargetsEnumerator = rayCastTargets.GetEnumerator();
            while (rayCastTargetsEnumerator.MoveNext()) 
            {
                var rayCastTarget = rayCastTargetsEnumerator.Current.Value;
                rayCastTargetsCache.Dispose(rayCastTarget);
            }
            rayCastTargets.Clear();
        }

        void disposeRayCastTarget(GameObject forGameObject)
        {
            if (rayCastTargets.ContainsKey(forGameObject))
            {
                rayCastTargetsCache.Dispose(rayCastTargets[forGameObject]);
                rayCastTargets.Remove(forGameObject);
            }
        }

        protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
        protected static readonly Color GizmoBlockedColor = Color.red;
        public virtual void OnDrawGizmosSelected()
        {
            if (!isActiveAndEnabled) return;

            Gizmos.color = GizmoColor;
            foreach (GameObject go in DetectedObjects)
            {
                Vector3 goCentre = getCentreOfColliders(go, DetectionMode == SensorMode.RigidBodies && rigidBodyColliders.ContainsKey(go)
                    ? rigidBodyColliders[go] : gameObjectColliders[go]) + (Vector2)go.transform.position;
                Gizmos.DrawIcon(goCentre, "SensorToolkit/eye.png", true);
            }

            if (RequiresLineOfSight && ShowRayCastDebug != null)
            {
                foreach (RayCastResult result in raycastResults)
                {
                    if (!ShowRayCastDebug.Contains(result.go)) continue;

                    Gizmos.color = GizmoColor;
                    if (result.isObstructed)
                    {
                        Gizmos.DrawLine(transform.position, result.obstructionPoint);
                        Gizmos.color = GizmoBlockedColor;
                        Gizmos.DrawLine(result.obstructionPoint, result.testPoint);
                        Gizmos.DrawCube(result.testPoint, Vector3.one * 0.1f);
                    }
                    else
                    {
                        Gizmos.DrawLine(transform.position, result.testPoint);
                        Gizmos.DrawCube(result.testPoint, Vector3.one * 0.1f);
                    }
                }
            }
        }
    }
}
