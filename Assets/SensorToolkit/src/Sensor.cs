using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SensorToolkit
{
    /*
     *  Sensors can run in two detection modes
     *  - Colliders: The sensor detects the GameObject attached to any collider it intersects.
     *  - RigidBodies: The sensor detects the GameObject owning the attached RigidBody of any collider it intersects.
     */
    public enum SensorMode { Colliders, RigidBodies }

    public class TagSelectorAttribute : PropertyAttribute { }

    /*
     *  Base class implemented by all sensor types with common functions for querying and filtering
     *  the sensors list of detected objects.
     */
    public abstract class Sensor : MonoBehaviour
    {
        [Tooltip("Any GameObject in this list will not be detected by this sensor, however it may still block line of sight.")]
        public List<GameObject> IgnoreList;

        [Tooltip("When set to true the sensor will only detect objects whose tags are in the 'TagFilter' array.")]
        public bool EnableTagFilter;

        [Tooltip("Array of tags that will be detected by the sensor.")]
        [TagSelector]
        public string[] AllowedTags;

        // Should return a list of all detected GameObjects, not necessarily in any order.
        public abstract List<GameObject> DetectedObjects { get; }

        // Should return a list of all detected GameObjects in order of distance from the sensor.
        public abstract List<GameObject> DetectedObjectsOrderedByDistance { get; }

        [System.Serializable]
        public class SensorEventHandler : UnityEvent<Sensor> { }

        [System.Serializable]
        public class SensorDetectionEventHandler : UnityEvent<GameObject, Sensor> { }

        // Event is called for each GameObject at the time it is added to the sensors DetectedObjects list
        [SerializeField]
        public SensorDetectionEventHandler OnDetected;

        // Event is called for each GameObject at the time it is removed to the sensors DetectedObjects list
        [SerializeField]
        public SensorDetectionEventHandler OnLostDetection;

        protected virtual void Awake()
        {
            if (IgnoreList == null)
            {
                IgnoreList = new List<GameObject>();
            }

            if (OnDetected == null) 
            {
                OnDetected = new SensorDetectionEventHandler();
            }

            if (OnLostDetection == null)
            {
                OnLostDetection = new SensorDetectionEventHandler();
            }
        }

        // Returns true when the passed GameObject is currently detected by the sensor, false otherwise.
        public virtual bool IsDetected(GameObject go)
        {
            var detectedEnumerator = DetectedObjects.GetEnumerator();
            while (detectedEnumerator.MoveNext())
            {
                if (detectedEnumerator.Current == go) { return true; }
            }
            return false;
        }

        // Returns the visibility between 0-1 of the specified object. A 0 means its not visible at all while
        // a 1 means it is entirely visible. Generally only used in the context of line of sight testing.
        public virtual float GetVisibility(GameObject go)
        {
            return IsDetected(go) ? 1f : 0f;
        }

        // Should cause the sensor to perform it's 'sensing' routine, so that its list of detected objects
        // is up to date at the time of calling. Each sensor can be configured to pulse automatically at
        // fixed intervals or each timestep, however, if you need more control over when this occurs then
        // you can call this method manually.
        public abstract void Pulse();

        /*
         * These methods return a list of GameObjects ordered by distance from the sensor. You may also
         * filter detected GameObjects by name, tag, Component or a combination thereof.
         */
        public List<GameObject> GetDetected()
        {
            return new List<GameObject>(DetectedObjectsOrderedByDistance);
        }

        public List<T> GetDetectedByComponent<T>() where T : Component
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<T>();
            while(detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                var c = go.GetComponent<T>();
                if (c != null) { filtered.Add(c); }
            }
            return filtered;
        }

        public List<Component> GetDetectedByComponent(Type t)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<Component>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                var c = go.GetComponent(t);
                if (c != null) { filtered.Add(c); }
            }
            return filtered;
        }

        public List<GameObject> GetDetectedByName(string name)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<GameObject>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.name == name) { filtered.Add(go); }
            }
            return filtered;
        }

        public List<T> GetDetectedByNameAndComponent<T>(string name) where T : Component
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<T>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.name == name)
                {
                    var c = go.GetComponent<T>();
                    if (c != null) { filtered.Add(c); }
                }
            }
            return filtered;
        }

        public List<Component> GetDetectedByNameAndComponent(string name, Type t)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<Component>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.name == name)
                {
                    var c = go.GetComponent<Component>();
                    if (c != null) { filtered.Add(c); }
                }
            }
            return filtered;
        }

        public List<GameObject> GetDetectedByTag(string tag)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<GameObject>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.CompareTag(tag)) { filtered.Add(go); }
            }
            return filtered;
        }

        public List<T> GetDetectedByTagAndComponent<T>(string tag) where T : Component
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<T>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.CompareTag(tag))
                {
                    var c = go.GetComponent<T>();
                    if (c != null) { filtered.Add(c); }
                }
            }
            return filtered;
        }

        public List<Component> GetDetectedByTagAndComponent(string tag, Type t)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<Component>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.CompareTag(tag))
                {
                    var c = go.GetComponent(t);
                    if (c != null) { filtered.Add(c); }
                }
            }
            return filtered;
        }

        public List<GameObject> GetDetectedByNameAndTag(string name, string tag)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<GameObject>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.CompareTag(tag) && go.name == name) { filtered.Add(go); }
            }
            return filtered;
        }

        public List<T> GetDetectedByNameAndTagAndComponent<T>(string name, string tag) where T : Component
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<T>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.CompareTag(tag) && go.name == name)
                {
                    var c = GetComponent<T>();
                    if (c != null) { filtered.Add(c); }
                }
            }
            return filtered;
        }

        public List<Component> GetDetectedByNameAndTagAndComponent(string name, string tag, Type t)
        {
            var detectedEnumerator = DetectedObjectsOrderedByDistance.GetEnumerator();
            var filtered = new List<Component>();
            while (detectedEnumerator.MoveNext())
            {
                var go = detectedEnumerator.Current;
                if (go.CompareTag(tag) && go.name == name)
                {
                    var c = GetComponent(t);
                    if (c != null) { filtered.Add(c); }
                }
            }
            return filtered;
        }

        /*
         * These methods return the detected GameObject that's nearest to the specified world position. 
         * If no GameObjects are detected then it returns null. You may also filter detected GameObjects 
         * by name, tag, Component or a combination thereof.
         */
        public GameObject GetNearestToPoint(Vector3 p)
        {
            return nearestToPoint(DetectedObjects, p);
        }

        public T GetNearestToPointByComponent<T>(Vector3 p) where T : Component
        {
            return nearestToPointWithComponent<T>(DetectedObjects, p);
        }

        public Component GetNearestToPointByComponent(Vector3 p, Type t)
        {
            return nearestToPointWithComponent(DetectedObjects, p, t);
        }

        public GameObject GetNearestToPointByName(Vector3 p, string name)
        {
            return nearestToPointWithName(DetectedObjects, p, name);
        }

        public T GetNearestToPointByNameAndComponent<T>(Vector3 p, string name) where T : Component
        {
            return nearestToPointWithNameAndComponent<T>(DetectedObjects, p, name);
        }

        public Component GetNearestToPointByNameAndComponent(Vector3 p, string name, Type t)
        {
            return nearestToPointWithNameAndComponent(DetectedObjects, p, name, t);
        }

        public GameObject GetNearestToPointByTag(Vector3 p, string tag)
        {
            return nearestToPointWithTag(DetectedObjects, p, tag);
        }

        public T GetNearestToPointByTagAndComponent<T>(Vector3 p, string tag) where T : Component
        {
            return nearestToPointWithTagAndComponent<T>(DetectedObjects, p, tag);
        }

        public Component GetNearestToPointByTagAndComponent(Vector3 p, string tag, Type t)
        {
            return nearestToPointWithTagAndComponent(DetectedObjects, p, tag, t);
        }

        public GameObject GetNearestToPointByNameAndTag(Vector3 p, string name, string tag)
        {
            return nearestToPointWithNameAndTag(DetectedObjects, p, name, tag);
        }

        public T GetNearestToPointByNameAndTagAndComponent<T>(Vector3 p, string name, string tag) where T : Component
        {
            return nearestToPointWithNameAndTagAndComponent<T>(DetectedObjects, p, name, tag);
        }

        public Component GetNearestToPointByNameAndTagAndComponent(Vector3 p, string name, string tag, Type t)
        {
            return nearestToPointWithNameAndTagAndComponent(DetectedObjects, p, name, tag, t);
        }

        /*
         * These methods return the detected GameObject that's nearest to the sensor. If no GameObjects are 
         * detected then it returns null. You may also filter detected GameObjects by name, tag, Component 
         * or a combination thereof.
         */
        public GameObject GetNearest()
        {
            return nearestToPoint(DetectedObjects, transform.position);
        }

        public T GetNearestByComponent<T>() where T : Component
        {
            return nearestToPointWithComponent<T>(DetectedObjects, transform.position);
        }

        public Component GetNearestByComponent(Type t)
        {
            return nearestToPointWithComponent(DetectedObjects, transform.position, t);
        }

        public GameObject GetNearestByName(string name)
        {
            return nearestToPointWithName(DetectedObjects, transform.position, name);
        }

        public T GetNearestByNameAndComponent<T>(string name) where T : Component
        {
            return nearestToPointWithNameAndComponent<T>(DetectedObjects, transform.position, name);
        }

        public Component GetNearestByNameAndComponent(string name, Type t)
        {
            return nearestToPointWithNameAndComponent(DetectedObjects, transform.position, name, t);
        }

        public GameObject GetNearestByTag(string tag)
        {
            return nearestToPointWithTag(DetectedObjects, transform.position, tag);
        }

        public T GetNearestByTagAndComponent<T>(string tag) where T : Component
        {
            return nearestToPointWithTagAndComponent<T>(DetectedObjects, transform.position, tag);
        }

        public Component GetNearestByTagAndComponent(string tag, Type t)
        {
            return nearestToPointWithTagAndComponent(DetectedObjects, transform.position, tag, t);
        }

        public GameObject GetNearestByNameAndTag(string name, string tag)
        {
            return nearestToPointWithNameAndTag(DetectedObjects, transform.position, name, tag);
        }

        public T GetNearestByNameAndTagAndComponent<T>(string name, string tag) where T : Component
        {
            return nearestToPointWithNameAndTagAndComponent<T>(DetectedObjects, transform.position, name, tag);
        }

        public Component GetNearestByNameAndTagAndComponent(string name, string tag, Type t)
        {
            return nearestToPointWithNameAndTagAndComponent(DetectedObjects, transform.position, name, tag, t);
        }

        protected bool shouldIgnore(GameObject go)
        {
            if (EnableTagFilter)
            {
                var tagFound = false;
                for (int i = 0; i < AllowedTags.Length; i++)
                {
                    if (AllowedTags[i] != "" && go != null && go.CompareTag(AllowedTags[i]))
                    {
                        tagFound = true;
                        break;
                    }
                }
                if (!tagFound) return true;
            }
            for (int i = 0; i < IgnoreList.Count; i++)
            {
                if (IgnoreList[i] == go) return true;
            }
            return false;
        }

        private GameObject nearestToPoint(List<GameObject> gos, Vector3 point)
        {
            GameObject nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = go;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private T nearestToPointWithComponent<T>(List<GameObject> gos, Vector3 point) where T : Component
        {
            T nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                var c = go.GetComponent<T>();
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private Component nearestToPointWithComponent(List<GameObject> gos, Vector3 point, Type t)
        {
            Component nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                var c = go.GetComponent(t);
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private GameObject nearestToPointWithName(List<GameObject> gos, Vector3 point, string name)
        {
            GameObject nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (go.name != name) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = go;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private T nearestToPointWithNameAndComponent<T>(List<GameObject> gos, Vector3 point, string name) where T : Component
        {
            T nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (go.name != name) { continue; }
                var c = go.GetComponent<T>();
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private Component nearestToPointWithNameAndComponent(List<GameObject> gos, Vector3 point, string name, Type t)
        {
            Component nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (go.name != name) { continue; }
                var c = go.GetComponent(t);
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private GameObject nearestToPointWithTag(List<GameObject> gos, Vector3 point, string tag)
        {
            GameObject nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (!go.CompareTag(tag)) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = go;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private T nearestToPointWithTagAndComponent<T>(List<GameObject> gos, Vector3 point, string tag) where T : Component
        {
            T nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (!go.CompareTag(tag)) { continue; }
                var c = go.GetComponent<T>();
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private Component nearestToPointWithTagAndComponent(List<GameObject> gos, Vector3 point, string tag, Type t)
        {
            Component nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (!go.CompareTag(tag)) { continue; }
                var c = go.GetComponent(t);
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private GameObject nearestToPointWithNameAndTag(List<GameObject> gos, Vector3 point, string name, string tag)
        {
            GameObject nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (go.name != name || !go.CompareTag(tag)) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = go;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private T nearestToPointWithNameAndTagAndComponent<T>(List<GameObject> gos, Vector3 point, string name, string tag) where T : Component
        {
            T nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (go.name != name || !go.CompareTag(tag)) { continue; }
                var c = go.GetComponent<T>();
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }

        private Component nearestToPointWithNameAndTagAndComponent(List<GameObject> gos, Vector3 point, string name, string tag, Type t)
        {
            Component nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            while (gosEnumerator.MoveNext())
            {
                var go = gosEnumerator.Current;
                if (go.name != name || !go.CompareTag(tag)) { continue; }
                var c = go.GetComponent(t);
                if (c == null) { continue; }
                var d = Vector3.SqrMagnitude(go.transform.position - point);
                if (nearest == null || d < nearestDistance)
                {
                    nearest = c;
                    nearestDistance = d;
                }
            }
            return nearest;
        }
    }

    public class DistanceFromPointComparer : IComparer<GameObject>
    {
        public Vector3 Point;

        public int Compare(GameObject x, GameObject y)
        {
            var d1 = Vector3.SqrMagnitude(x.transform.position - Point);
            var d2 = Vector3.SqrMagnitude(y.transform.position - Point);
            if (d1 < d2) { return -1; }
            else if (d1 > d2) { return 1; }
            else { return 0; }
        }
    }
}