using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField][Range(1,10)] private int inventorySize = 2;
    [SerializeField] private List<GameObject> inventory;
    [SerializeField] private GameObject defaultItem;

   [SerializeField] private int inventoryIndex = 0;

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
        inventory = new List<GameObject>(inventorySize);
        inventory.Add(defaultItem);
        for (int i = 1; i < inventorySize; ++i)
        {
            inventory.Add(null);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;
        
        int previousIndex = inventoryIndex;
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // scroll wheel up
        {
            if (inventoryIndex >= inventorySize - 1)
                inventoryIndex = 0;
            else
            {
                inventoryIndex++;
                for (int i = inventoryIndex; i < inventorySize; ++i)
                {
                    if (i == inventorySize)
                    {
                        inventoryIndex = 0;
                        break;
                    }
                    else if (inventory[i] != null)
                    {
                        break;
                    }
                    inventoryIndex++;
                }
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) // scroll wheel down
        {
            if (inventoryIndex <= 0)
                inventoryIndex = inventorySize - 1;
            else
            {
                inventoryIndex--;
            }

            if (inventory[inventoryIndex] == null)
            {
                for (int i = inventoryIndex - 1; i >= 0; i--)
                {
                    inventoryIndex--;
                    if (inventoryIndex == 0 || inventory[i] != null)
                    {
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < numKeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(numKeyCodes[i]) && inventory[i] != null)
            {
                inventoryIndex = i;
                break;
            }
        }
        
        if (previousIndex != inventoryIndex)// && !(inventory[previousIndex] == defaultItem && inventory[inventoryIndex] == defaultItem))
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
            if (inventoryIndex == 0)
                return false; // don't replace claws

            inventory[inventoryIndex].SetActive(false);
            inventory[inventoryIndex] = Instantiate(newItem, gameObject.transform.GetChild(0));
            UpdateItem();
            // code will need to be added if the item being replaced needs to be dropped
        }
        else
        {
            bool found = false;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == null)
                {
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
            if (g == null)
                continue;

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
