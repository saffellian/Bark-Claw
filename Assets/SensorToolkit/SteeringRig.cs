using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SensorToolkit
{
    /*
     * A steering rig is used to compute steering vectors, so that gameobjects are able to navigate
     * around obstacles. Assume you have a character that wants to move in some direction, the steering
     * rig will push that direction depending on the obstacles around it, resulting in a vector that should
     * be in a similar general direction, but clear of nearby obstacles.
     * 
     * The rig computes its steering vectors using a set of RaySensor components on child transforms. You are 
     * free to create as many RaySensors as you'd like, and you may orient them how you like. There are some 
     * premade steering rig prefabs under the SensorToolkit/SteeringRigPrefabs/ folder, I recommend using 
     * one of these as a template.
     * 
     * The steering rig has two modes of operation, you can use it as a simple movement system by assigning
     * a rigid body to control, or you can use it for calculating steering vectors only by leaving the rigid
     * body field blank. If you leave it blank then you are using only the 'sensing' capability of the steering
     * rig whilst retaining full control over your characters movement. Have your character tell the sensor
     * what direction it wants to move, and the sensor will calculate the direction it should move instead so that it
     * avoids nearby obstacles.
     * 
     * If you use the rig as a movement system then you can give it a destination and the rig will try to
     * move there while avoiding obstacles.
     */
    public class SteeringRig : MonoBehaviour
    {
        [Tooltip("The rig won't try to steer around objects in this list.")]
        public List<GameObject> IgnoreList;

        [Range(0.1f, 4f), Tooltip("Lower numbers mean the rig will move closer to obstacles.")]
        public float AvoidanceSensitivity = 1f;
        [Range(1f, 2f), Tooltip("The max distance that can be steered from the target direction.")]
        public float MaxAvoidanceLength = 1f;

        [Tooltip("Should the rig steer along the Y-axis, up and down. Should be false for characters that move along the ground, but true for spaceships or airoplanes.")]
        public bool YAxis = true;
        [Tooltip("Rotate the rig towards the target direction before calculating steer vectors. Useful for creating asymetric ray sensor setups. See the example prefabs, they are all RotateTowardsTarget = true.")]
        public bool RotateTowardsTarget = false;

        [Tooltip("If assigned the steering rig will control the movement of this rigid body.")]
        public Rigidbody RB;
        [Tooltip("The maximum torque that will be applied to the rigid body.")]
        public float TurnForce;
        [Tooltip("The maximum force that will be applied to the rigid body in a forwards direction.")]
        public float MoveForce;
        [Tooltip("The maximum force that will be applied to the rigid body in a sideways or backwards direction.")]
        public float StrafeForce;
        [Tooltip("The maximum turning speed that will be applied to kinematic rigid bodies.")]
        public float TurnSpeed;
        [Tooltip("The maximum movement speed that will be applied to kinematic rigid bodies in a forwards direction.")]
        public float MoveSpeed;
        [Tooltip("The maximum movement speed that will be applied to kinematic rigid bodies in a sideways or backwards direction.")]
        public float StrafeSpeed;
        [Tooltip("The distance threshold for the rig to arrive at a destination position.")]
        public float StoppingDistance = 0.5f;
        [Tooltip("The rig will attempt to move towards this transform.")]
        public Transform DestinationTransform;
        [Tooltip("The rig will face towards this transform, even strafing while moving towards destination.")]
        public Transform FaceTowardsTransform;

        RaySensor[] sensors;
        Vector3 destination;
        bool trackingToDestinationPosition;
        Vector3 faceDirection;
        bool directionToFaceAssigned;
        Vector3 previousAttractionVector;
        Vector3 previousRepulsionVector;
        Vector3 previousAvoidanceVector;

        // Set a destination vector that the rig should seek towards, can only be set while DestinationTrasnform
        // is null. Only works if a rigid body is assigned to the rig.
        public Vector3 Destination
        {
            get
            {
                if (DestinationTransform != null)
                {
                    trackingToDestinationPosition = false;
                    return Vector3.Scale(DestinationTransform.position, axisConstraint);
                }
                else return Vector3.Scale(destination, axisConstraint);
            }
            set
            {
                if (DestinationTransform != null)
                {
                    Debug.LogWarning("Cannot set Destination while DestinationTransform is not Null.");
                }
                else
                {
                    destination = value;
                    trackingToDestinationPosition = true;
                }
            }
        }

        // Is the rig currently seeking towards a destination.
        public bool IsSeeking
        {
            get { return RB != null && (DestinationTransform != null || trackingToDestinationPosition); }
        }

        // Set a direction vector that the rig should face towards. Normally the rig will face towards the
        // direction its travelling, but this parameter overrides that allowing the rig to strafe. Only works
        // if a rigid body is assigned. Clear the DirectionToFace by calling ClearDirectionToFace() or assigning
        // the zero vector.
        public Vector3 DirectionToFace
        {
            get
            {
                if (FaceTowardsTransform != null)
                {
                    return (FaceTowardsTransform.position - RB.transform.position).normalized;
                }
                else if (directionToFaceAssigned)
                {
                    return faceDirection;
                }
                else
                {
                    return Vector3.zero;
                }
            }
            set
            {
                if (FaceTowardsTransform != null)
                {
                    Debug.LogWarning("Cannot set DirectionToFace while FaceTowardsTransform is not Null.");
                }
                else if (value == Vector3.zero)
                {
                    ClearDirectionToFace();
                }
                else
                {
                    directionToFaceAssigned = true;
                    faceDirection = value.normalized;
                }
            }
        }

        // Is the rig currently instructed to face a specific direction.
        public bool IsDirectionToFaceAssigned
        {
            get { return FaceTowardsTransform != null || directionToFaceAssigned; }
        }

        // Clears the assigned direction to face, stopping the rig from strafing.
        public void ClearDirectionToFace()
        {
            FaceTowardsTransform = null;
            directionToFaceAssigned = false;
        }

        // Takes a direction as parameter and pushes it depending on the obstacles nearby the rig.
        // Returns a vector that is in the general direction of 'targetDirection' but moved so that
        // nearby obstacles are avoided.
        public Vector3 GetSteeredDirection(Vector3 targetDirection)
        {
            targetDirection = Vector3.Scale(targetDirection, axisConstraint).normalized;
            if (RotateTowardsTarget)
            {
                transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            }

            return accumForces(targetDirection);
        }

        Vector3 axisConstraint
        {
            get
            {
                return new Vector3(1f, YAxis ? 1f : 0f, 1f);
            }
        }

        void Awake()
        {
            if (IgnoreList == null) 
            {
                IgnoreList = new List<GameObject>();
            }

            sensors = GetComponentsInChildren<RaySensor>();
            trackingToDestinationPosition = false;
        }

        void Update()
        {
            // Seek routine for kinematic rigid bodies
            if (RB == null || !RB.isKinematic) return;

            // If we have been assigned a direction to face, then turn to face it even if we aren't currently seeking
            if (IsDirectionToFaceAssigned) FaceDirectionKinematic(DirectionToFace);

            if (!IsSeeking) return;

            if (hasReachedDestination())
            {
                trackingToDestinationPosition = false;
                return;
            }

            var targetMoveDirection = (Destination - RB.transform.position).normalized;
            var avoidanceMoveDirection = GetSteeredDirection(targetMoveDirection);

            // If we haven't been assigned a direction to face then turn to face the direction we will move in.
            if (!IsDirectionToFaceAssigned) FaceDirectionKinematic(avoidanceMoveDirection);

            // Lerp the dot product of the direction I'm facing to the direction I'm moving,
            // this will interpolate between the strafing speed and the moving speed.
            float forwardDotVel = Vector3.Dot(RB.transform.forward, avoidanceMoveDirection.normalized);
            avoidanceMoveDirection = Mathf.Lerp(StrafeSpeed, MoveSpeed, Mathf.Clamp(forwardDotVel, 0f, 1f)) * avoidanceMoveDirection;
            RB.transform.position = RB.transform.position + avoidanceMoveDirection * Time.deltaTime;
        }

        void FaceDirectionKinematic(Vector3 direction)
        {
            var targetRot = Quaternion.LookRotation(direction, Vector3.up);
            RB.transform.rotation = Quaternion.RotateTowards(RB.transform.rotation, targetRot, TurnSpeed * Time.deltaTime);
        }

        void FixedUpdate()
        {
            // Only seek with forces if we are seeking and we have a non-kinematic rigid body
            if (RB == null || RB.isKinematic) return;

            // If we have been assigned a direction to face, then turn to face it even if we aren't currently seeking
            if (IsDirectionToFaceAssigned) FaceDirectionForces(DirectionToFace);

            // Only seek with forces if we are seeking and we have a non-kinematic rigid body
            if (!IsSeeking) return;

            if (hasReachedDestination())
            {
                trackingToDestinationPosition = false;
                return;
            }

            var targetMoveDirection = Destination - RB.transform.position;
            var avoidanceMoveDirection = GetSteeredDirection(targetMoveDirection);

            // If we haven't been assigned a direction to face then turn to face the direction we will move in.
            if (!IsDirectionToFaceAssigned) FaceDirectionForces(avoidanceMoveDirection);

            // Lerp the dot product of the direction I'm facing to the direction I'm moving,
            // this will interpolate between the strafing force and the moving force.
            float forwardDotMove = Vector3.Dot(RB.transform.forward, avoidanceMoveDirection);
            Vector3 moveForce = Mathf.Lerp(StrafeForce, MoveForce, Mathf.Clamp(forwardDotMove, 0f, 1f)) * avoidanceMoveDirection;
            RB.AddForce(moveForce);

            // Try to keep up vector facing sky
            if (YAxis)
            {
                var zRot = RB.rotation.eulerAngles.z;
                if (zRot > 180f) zRot -= 360f;
                var zForce = Mathf.Clamp(zRot / 10f, -1f, 1f) * TurnForce;
                RB.AddTorque(RB.transform.forward * -zForce);
            }
        }

        void FaceDirectionForces(Vector3 direction)
        {
            Quaternion dRot;
            if (YAxis)
            {
                dRot = Quaternion.FromToRotation(RB.transform.forward, direction);
            }
            else
            {
                var a = signedAngleXZ(RB.transform.forward, direction);
                dRot = Quaternion.AngleAxis(a, Vector3.up);
            }

            float angle; Vector3 axis;
            dRot.ToAngleAxis(out angle, out axis);
            angle = angle > 180f ? angle - 360f : angle;
            var torque = Mathf.Clamp(angle / 20f, -1f, 1f) * TurnForce;
            RB.AddTorque(axis * torque);
        }

        Vector3 accumForces(Vector3 targetDirection)
        {
            previousAttractionVector = attractionForce(targetDirection);
            previousRepulsionVector = repulsionForce();
            var f = previousAttractionVector + previousRepulsionVector;
            if (f.sqrMagnitude > 0.01f)
            {
                previousAvoidanceVector = f.normalized;
                return previousAvoidanceVector;
            }
            else
            {
                previousAvoidanceVector = f * 100f;
                return previousAvoidanceVector;
            }
        }

        Vector3 attractionForce(Vector3 targetDirection)
        {
            var dest = Vector3.Scale(targetDirection, axisConstraint);
            if (dest.sqrMagnitude > 1f)
            {
                return dest.normalized;
            }
            else
            {
                return dest;
            }
        }

        Vector3 repulsionForce()
        {
            var rf = Vector3.zero;
            for (int i = 0; i < sensors.Length; i++)
            {
                var s = sensors[i];
                s.IgnoreList = IgnoreList;
                s.Pulse();
                if (!s.IsObstructed) continue;
                var obsRatio = Mathf.Pow(1f - (s.ObstructionRayHit.distance / s.Length), 1f/AvoidanceSensitivity); // 0 when unobstructed, 1 when touching
                rf += obsRatio * s.ObstructionRayHit.normal;
            }
            var rfMag = rf.magnitude;
            rf = Vector3.Scale(rf, axisConstraint).normalized;
            if (rfMag > MaxAvoidanceLength)
            {
                return rf * MaxAvoidanceLength;
            }
            return rf * rfMag;
        }

        float signedAngleXZ(Vector3 a, Vector3 b)
        {
            var aa = Mathf.Atan2(a.x, a.z) * Mathf.Rad2Deg;
            var ba = Mathf.Atan2(b.x, b.z) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(aa, ba);
        }

        bool hasReachedDestination()
        {
            return (RB.transform.position - Destination).magnitude <= StoppingDistance;
        }

        protected static readonly Color AttractionVectorColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
        protected static readonly Color RepulsionVectorColor = Color.yellow;
        protected static readonly Color AvoidanceVectorColor = Color.green;
        public void OnDrawGizmosSelected()
        {
            if (!isActiveAndEnabled) return;

            var attractionPoint = transform.position + previousAttractionVector * 2f;
            var repulsionPoint = attractionPoint + previousRepulsionVector * 2f;
            var avoidancePoint = transform.position + previousAvoidanceVector * 2f;

            Gizmos.color = AttractionVectorColor;
            Gizmos.DrawLine(transform.position, attractionPoint);
            Gizmos.color = RepulsionVectorColor;
            Gizmos.DrawLine(attractionPoint, repulsionPoint);
            Gizmos.color = AvoidanceVectorColor;
            Gizmos.DrawLine(transform.position, avoidancePoint);
        }
    }
}