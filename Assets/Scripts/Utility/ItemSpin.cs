using UnityEngine;

public class ItemSpin : MonoBehaviour
{
    private enum Axis
    {
        X,
        Y,
        Z
    }

    [SerializeField] private AnimationCurve BobCurve;
    [SerializeField] private bool randomizeBobOffset = false;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private bool randomizeRotateSpeed = true;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private Axis rotateAxis = Axis.Z;

    private float startY;
    private Vector3 axis;
    private float bobOffset = 0f;
    private float rotateOffset = 0f;

    private void Start() {
        if (randomizeBobOffset)
        {
            bobOffset = Random.Range(0f, 0.99f);
        }

        if (randomizeRotateSpeed)
        {
            float qSpeed = (rotateSpeed / 4f);
            rotateOffset = Random.Range(rotateSpeed - qSpeed, rotateSpeed + qSpeed);
        }

        startY = transform.position.y;
        switch(rotateAxis)
        {
            case Axis.Z:
                axis = Vector3.forward;
                break;
            case Axis.Y:
                axis = Vector3.up;
                break;
            case Axis.X:
                axis = Vector3.right;
                break;
        }  
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, startY + BobCurve.Evaluate((Time.fixedTime + bobOffset) * bobSpeed), transform.position.z);

        transform.Rotate(axis, Time.deltaTime * rotateSpeed);
    }
}
