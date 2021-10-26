using UnityEngine;

public class GrassBeautifier : MonoBehaviour
{
    
    private const float moveAmount = 0.1f;

    void Start()
    {
        GameObject[] grass = GameObject.FindGameObjectsWithTag("GrassDecor");

        Vector3 currRotation;
        GameObject child;
        Vector3 currLocation;

        foreach(var parent in grass)
        {
            currRotation = parent.transform.rotation.eulerAngles;
            currRotation.y = Random.Range(0f, 359f);
            parent.transform.rotation = Quaternion.Euler(currRotation);

            child = parent.transform.GetChild(0).gameObject;
            currLocation = child.transform.localPosition;
            currLocation.x = Random.Range(0f, moveAmount);
            currLocation.z = Random.Range(0f, moveAmount);
            child.transform.localPosition = currLocation;
        }

        Destroy(gameObject);
    }
}
