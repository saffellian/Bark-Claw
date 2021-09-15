using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ParallaxObject : MonoBehaviour
{
    public float parallaxFactor = 1;

    private Transform cameraTransform;
    private float prevPosition;
    private float delta;
    private RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
        prevPosition = cameraTransform.position.x;
        image = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!prevPosition.Equals(cameraTransform.position.x))
        {
            delta = cameraTransform.position.x - prevPosition;
            delta *= parallaxFactor / 100f;
            image.uvRect = new Rect((image.uvRect.x + delta) % 2, image.uvRect.y, image.uvRect.width, image.uvRect.height);
        }
        prevPosition = cameraTransform.position.x;
    }
}
