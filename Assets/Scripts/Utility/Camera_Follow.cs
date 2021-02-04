using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    
        [Header("Camera Follow")]
        public float _dampTime = 0.15f;
        public Transform _target;

        // These public booleans allow you to move in any specific direction
        public bool _followX = true;
        public bool _followY = true;

        private Vector3 _velocity = Vector3.zero;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (_target)
            {
                Vector3 point = Camera.main.WorldToViewportPoint(_target.position);
                Vector3 delta = _target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
                Vector3 destination = transform.position + delta;
                transform.position = Vector3.SmoothDamp(transform.position, new Vector3(_followX ? destination.x : transform.position.x,
                                                                                         _followY ? destination.y : transform.position.y,
                                                                                         transform.position.z), ref _velocity, _dampTime);
            }

        }
     
}
