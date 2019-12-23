using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class AutomatedVehicle : MonoBehaviour
{
    [SerializeField] private Transform axleCenter;
    [SerializeField] private Transform rearAxle;
    [Tooltip("Place checkpoints in the order of traversal")]
    [SerializeField] private List<Transform> checkpoints;
    [SerializeField] private float speed = 12;
    [SerializeField] private int collisionDamage = 50;
    [SerializeField] private float knockbackForce = 10;
    [Tooltip("The magnitude at which the vehicle will cause damage to the player")]
    [SerializeField] private float damageMagnitude = 5;

    private Rigidbody rigidbody;
    private Queue<Transform> checkQueue = new Queue<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        checkpoints.ForEach(cp =>
        {
            cp.position = new Vector3(cp.position.x, rigidbody.position.y, cp.position.z);
            checkQueue.Enqueue(cp);
        });

        StartCoroutine(Traversal());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player") && other.relativeVelocity.magnitude > damageMagnitude)
        {
            Rigidbody rb = other.rigidbody;
            Vector3 dir = (other.contacts[0].point - transform.position);
            other.transform.GetComponent<FirstPersonController>().AddImpact(dir*knockbackForce);
            PlayerHealth.Instance.ApplyDamage(collisionDamage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Enemy>() && Mathf.Abs(Vector3.Angle(transform.forward, (other.transform.position - transform.position))) < 30)
        {
            other.GetComponent<Enemy>().InstantDeath(true);
        }
    }

    private IEnumerator Traversal()
    {
        Transform target;
        Vector3 rotPoint = Vector3.zero;
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForFixedUpdate();
            target = checkQueue.Dequeue();
            while (Vector3.Distance(rigidbody.position, target.position) > 1f)
            {
                axleCenter.LookAt(target);
                rigidbody.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
                if (axleCenter.transform.localRotation.eulerAngles.y < 0)
                {
                    LineLineIntersection(out rotPoint, axleCenter.position, -axleCenter.right, rearAxle.position,
                        -rearAxle.right);
                    rigidbody.MoveRotation(UnityEngine.Quaternion.Lerp(transform.rotation, axleCenter.rotation, Time.fixedDeltaTime));
                }
                else if (axleCenter.transform.localRotation.eulerAngles.y > 0)
                {
                    LineLineIntersection(out rotPoint, axleCenter.position, axleCenter.right, rearAxle.position,
                        rearAxle.right);
                    rigidbody.MoveRotation(UnityEngine.Quaternion.Lerp(transform.rotation, axleCenter.rotation, Time.fixedDeltaTime));

                }
                yield return new WaitForFixedUpdate();
            }
            checkQueue.Enqueue(target);
        }
    }

    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
}
