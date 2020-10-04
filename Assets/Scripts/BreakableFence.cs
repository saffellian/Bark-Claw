using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableFence : MonoBehaviour
{
    [SerializeField] private int fenceHealth = 40;
    
    public void DamageFence(int amount)
    {
        fenceHealth -= amount;
        if (fenceHealth <= 0)
        {
            // animations and sounds go here
            gameObject.SetActive(false); // save some resources by not destroying. probably unnecessary
        }
    }

}
