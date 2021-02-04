using System;
using UnityEngine;
using System.Collections;

public class Billboarding : MonoBehaviour
{
    public Camera m_Camera;
    public bool unlockHorizontalAxes = false;

    private void Start()
    {
        if (m_Camera == null)
        {
            m_Camera = Camera.main;
        }
    }

    //Orient the object after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);

        if (!unlockHorizontalAxes)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }
}