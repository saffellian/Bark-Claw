using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SensorToolkit
{
    /*
     * A sensor that detects colliders within a specified range using Physics2D.OverlapCircleAll. Detects colliders or rigid
     * bodies on the chosen physics layers. Can be configured to pulse automatically at fixed intervals or manually. Has
     * optional support for line of sight testing.
     */
    [ExecuteInEditMode]
    public class RangeSensor2D : BaseAreaSensor
    {
        // Should the sensor be pulsed automatically at fixed intervals or should it be pulsed manually.
        public enum UpdateMode { FixedInterval, Manual }

        [Tooltip("The radius in world units that the sensor detects colliders in.")]
        public float SensorRange = 10f;

        [Tooltip("The physics layer mask that the sensor detects colliders on.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("Automatic or manually pulsing mode.")]
        public UpdateMode SensorUpdateMode;

        [Tooltip("If the chosen update mode is automatic then this is the interval in seconds between each automatic pulse.")]
        public float CheckInterval = 1f;

        [Tooltip("The initial size of the buffer used when calling Physics.OverlapSphereNoAlloc.")]
        public int InitialBufferSize = 20;

        [Tooltip("When set true the buffer used with Physics.OverlapSphereNoAlloc is expanded if its not sufficiently large.")]
        public bool DynamicallyIncreaseBufferSize = true;

        public int CurrentBufferSize { get; private set; }

        // Event that is called each time the sensor is pulsed. Used by the editor extensions, you shouldn't need to listen to it.
        public delegate void SensorUpdateHandler();
        public event SensorUpdateHandler OnSensorUpdate;

        // Pulses the sensor to update its list of detected objects
        public override void Pulse()
        {
            if (isActiveAndEnabled) testSensor();
        }

        HashSet<GameObject> previousDetectedObjects = new HashSet<GameObject>();
        Collider2D[] collidersBuffer;
        float timer = 0f;

        protected override void Awake() 
        {
            base.Awake();

            CurrentBufferSize = 0;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            previousDetectedObjects.Clear();
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (SensorUpdateMode == UpdateMode.FixedInterval) 
            {
                timer += Time.deltaTime;
                if (timer >= CheckInterval)
                {
                    testSensor();
                    timer = 0f;
                }
            }
            else
            {
                timer = 0f;
            }
        }

        void testSensor()
        {
            prepareCollidersBuffer();

            var numberDetected = Physics2D.OverlapCircleNonAlloc(transform.position, SensorRange, collidersBuffer, DetectsOnLayers);
            if (numberDetected == CurrentBufferSize)
            {
                if (DynamicallyIncreaseBufferSize)
                {
                    CurrentBufferSize *= 2;
                    testSensor();
                    return;
                }
                else
                {
                    logInsufficientBufferSize();
                }
            }

            clearColliders();
            for (int i = 0; i < numberDetected; i++)
            {
                var newDetection = addCollider(collidersBuffer[i]);
                if (newDetection != null)
                {
                    if (previousDetectedObjects.Contains(newDetection))
                    {
                        previousDetectedObjects.Remove(newDetection);
                    }
                    else
                    {
                        OnDetected.Invoke(newDetection, this);
                    }
                }
            }

            // Any entries still in previousDetectedObjects are no longer detected
            var previousDetectedEnumerator = previousDetectedObjects.GetEnumerator();
            while (previousDetectedEnumerator.MoveNext())
            {
                var lostDetection = previousDetectedEnumerator.Current;
                OnLostDetection.Invoke(lostDetection, this);
            }

            previousDetectedObjects.Clear();
            var detectedEnumerator = DetectedObjects.GetEnumerator();
            while (detectedEnumerator.MoveNext())
            {
                previousDetectedObjects.Add(detectedEnumerator.Current);
            }

            if (OnSensorUpdate != null) OnSensorUpdate();
        }

        void logInsufficientBufferSize()
        {
            Debug.LogWarning("A range sensor on " + name + " has an insufficient buffer size. Some objects may not be detected");
        }

        void prepareCollidersBuffer()
        {
            if (CurrentBufferSize == 0) 
            {
                InitialBufferSize = Math.Max(1, InitialBufferSize);
                CurrentBufferSize = InitialBufferSize;
            }
            if (collidersBuffer == null || collidersBuffer.Length != CurrentBufferSize) 
            {
                collidersBuffer = new Collider2D[CurrentBufferSize];
            }
        }

        void reset()
        {
            clearColliders();
            CurrentBufferSize = 0;
        }

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (!isActiveAndEnabled) return;
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireSphere(transform.position, SensorRange);
        }
    }
}
