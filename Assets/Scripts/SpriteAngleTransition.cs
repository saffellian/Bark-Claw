using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class SpriteAngleTransition : MonoBehaviour
{
    private enum Orientation
    {
        Front,
        Sides,
        Back
    }

    [SerializeField] private float sideAngle = 90;
    [SerializeField] private Transform referenceTransform;
    [SerializeField] private List<GameObject> sideSprites;
    [SerializeField] private List<GameObject> frontSprites;
    [SerializeField] private List<GameObject> backSprites;

    private Transform player;
    private Orientation currOrientation = Orientation.Sides;
    private Orientation prevOrientation = Orientation.Front;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<FirstPersonController>().transform;
        sideSprites.ForEach(x => x.SetActive(false));
        backSprites.ForEach(x => x.SetActive(false));
        frontSprites.ForEach(x => x.SetActive(false));
    }

    // Update is called once per frame
    void Update()
    {
        currOrientation = OrientationToPlayer();
        if (prevOrientation != currOrientation)
            UpdateDisplayedSprites();
        prevOrientation = currOrientation;
    }

    private void UpdateDisplayedSprites()
    {
        switch (prevOrientation)
        {
            case Orientation.Sides:
                sideSprites.ForEach(x => x.SetActive(false));
                break;
            case Orientation.Back:
                backSprites.ForEach(x => x.SetActive(false));
                break;
            case Orientation.Front: 
                frontSprites.ForEach(x => x.SetActive(false));
                break;
        }

        switch (currOrientation)
        {
            case Orientation.Sides:
                sideSprites.ForEach(x => x.SetActive(true));
                break;
            case Orientation.Back:
                backSprites.ForEach(x => x.SetActive(true));
                break;
            case Orientation.Front:
                frontSprites.ForEach(x => x.SetActive(true));
                break;
        }
    }

    private Orientation OrientationToPlayer()
    {
        Vector3 target = (referenceTransform.position - player.position).normalized;
        float angle = Vector3.Angle(referenceTransform.forward, target);
        float refAngle = sideAngle - (angle / 2);

        if (angle > refAngle && angle < refAngle + sideAngle)
        {
            return Orientation.Sides;
        }
        
        if (angle < refAngle)
        {
            return Orientation.Front;
        }

        return Orientation.Back;

    }
}
