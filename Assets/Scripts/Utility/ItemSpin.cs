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
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private Axis rotateAxis = Axis.Z;

    private float startY;
    private Vector3 axis;

    private void Start() {
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

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, startY + BobCurve.Evaluate(Time.fixedTime * bobSpeed), transform.position.z);

        transform.Rotate(axis, Time.deltaTime * rotateSpeed);
    }
}
