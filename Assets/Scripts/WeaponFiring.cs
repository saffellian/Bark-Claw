using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFiring : MonoBehaviour
{
    [Tooltip("Screen position that the bullet originates from")]
    [SerializeField] private Vector2 firePoint = new Vector2(0.5f, 0.5f);

    private Camera cam;
    private Ray ray;
    private RaycastHit hit;
    
    // Start is called before the first frame update
    void Start()
    {
        firePoint = new Vector2(Mathf.Clamp01(firePoint.x) * Screen.width, Mathf.Clamp01(firePoint.y) * Screen.height);
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = cam.ScreenPointToRay(firePoint);
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.transform.name);
            }
        }
    }
}
