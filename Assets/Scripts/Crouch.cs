using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : MonoBehaviour
{
    public CharacterController CharacterController;

   void Start ()
   {
        CharacterController = gameObject.GetComponent<CharacterController>();
   }
    void Update()
    {
        if (Input.GetButton("Crouch"))
        {
            CharacterController.height = .1f;
        }
        else
        {
            CharacterController.height = 1f;
        }
        
    }    
}
