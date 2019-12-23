using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField][Range(1,10)] private int inventorySize = 2;
    [SerializeField] private GameObject[] inventory;
    [SerializeField] private GameObject defaultItem;

    private int inventoryIndex = 0;

    private readonly KeyCode[] numKeyCodes =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0
    };

    private void Start()
    {
        inventory = new GameObject[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            inventory[i] = defaultItem;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;
        
        int previousIndex = inventoryIndex;
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (inventoryIndex >= inventorySize - 1)
                inventoryIndex = 0;
            else
            {
                inventoryIndex++;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (inventoryIndex <= 0)
                inventoryIndex = inventorySize - 1;
            else
            {
                inventoryIndex--;
            }
        }

        for (int i = 0; i < numKeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(numKeyCodes[i]))
            {
                inventoryIndex = i;
                break;
            }
        }
        
        if (previousIndex != inventoryIndex && !(inventory[previousIndex] == defaultItem && inventory[inventoryIndex] == defaultItem))
            UpdateItem();
    }

    public bool TryAddItem(GameObject newItem, bool replaceHeldItem)
    {
        foreach (GameObject g in inventory) // disallowing duplicate items in inventory
        {
            if (g == newItem)
                return false;
        }

        if (replaceHeldItem)
        {
            inventory[inventoryIndex].SetActive(false);
            inventory[inventoryIndex] = Instantiate(newItem, gameObject.transform.GetChild(0));
            UpdateItem();
            // code will need to be added if the item being replaced needs to be dropped
        }
        else
        {
            bool found = false;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == defaultItem)
                {
                    inventory[i].SetActive(false);
                    inventory[i] = Instantiate(newItem, gameObject.transform.GetChild(0));
                    UpdateItem();
                    found = true;
                    break;
                }
            }

            return found;
        }

        return true;
    }

    public void UpdateItem()
    {
        int i = 0;
        foreach (GameObject g in inventory)
        {
            if (i == inventoryIndex)
            {
                g.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }

            i++;
        }
    }
}
