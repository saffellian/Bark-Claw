using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private float weaponSwitchDelay = 0.2f;
    [SerializeField] private List<GameObject> availableItems = new List<GameObject>();

    private List<GameObject> instantiatedItems;
    private List<GameObject> inventory;
    private int inventorySize = 0;
    private int inventoryIndex = 0;
    private bool canSwitch = true;

    private StatusBar statusBar;

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
        statusBar = FindObjectOfType<StatusBar>();
        inventorySize = availableItems.Count;

        instantiatedItems = new List<GameObject>(inventorySize);
        var weaponParent = transform.Find("FirstPersonCharacter").Find("WeaponCanvas");
        foreach (var item in availableItems)
        {
            instantiatedItems.Add(Instantiate(item, weaponParent));
        }

        foreach (var item in instantiatedItems)
        {
            item.SetActive(false);
        }

        inventory = new List<GameObject>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(null);
        }
        inventory[0] = instantiatedItems[0];
        inventory[0].SetActive(true);
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;
        
        int previousIndex = inventoryIndex;
        
        if ((Input.mouseScrollDelta.y > 0 && canSwitch)|| Input.GetButtonDown("RB")) // scroll wheel up
        {
            if (!Input.GetButtonDown("RB"))
                StartCoroutine("WeaponSwitchTimer");

            inventoryIndex++;

            if (inventoryIndex > inventorySize - 1)
                inventoryIndex = 0;
            else
            {
                for (int i = inventoryIndex; i <= inventorySize; i++)
                {
                    if (i == inventorySize)
                    {
                        inventoryIndex = 0;
                        break;
                    }
                    else if (inventory[i] != null)
                    {
                        inventoryIndex = i;
                        break;
                    }
                }
            }
        }
        else if ((Input.mouseScrollDelta.y < 0 && canSwitch)|| Input.GetButtonDown("LB")) // scroll wheel down
        {
            if (!Input.GetButtonDown("LB"))
                StartCoroutine("WeaponSwitchTimer");

            inventoryIndex--;

            if (inventoryIndex < 0)
                inventoryIndex = inventorySize - 1;

            for (int i = inventoryIndex; i >= 0; i--)
            {
                if (i == 0 || inventory[i] != null)
                {
                    inventoryIndex = i;
                    break;
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

    private IEnumerator WeaponSwitchTimer()
    {
        canSwitch = false;
        yield return new WaitForSeconds(weaponSwitchDelay);
        canSwitch = true;
    }

    public void RemoveItem(GameObject item)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == item)
            {
                statusBar.DisableWeapon(i);
                instantiatedItems[i].SetActive(false);
                inventory[i] = null;
            }
        }

        inventoryIndex = 0;
        UpdateItem();
    }

    public bool TryAddItem(WeaponPickup pickup)
    {
        int invIndex = availableItems.FindIndex(0, availableItems.Count, x => x.gameObject == pickup.itemPrefab);

        if (inventory[invIndex] != null)
        {
            Weapon weapon = pickup.itemPrefab.GetComponent<Weapon>();
            
            if (pickup.overridePickupAmmo)
                inventory[invIndex].GetComponent<Weapon>().AddAmmo(pickup.overrideAmmoAmount, inventoryIndex == invIndex);
            else
                inventory[invIndex].GetComponent<Weapon>().AddAmmo(weapon.GetAmmoAmount(), inventoryIndex == invIndex);
            return true;
        }

        if (inventoryIndex == invIndex)
            return false; // don't replace current item

        inventory[invIndex] = instantiatedItems[invIndex];
        statusBar.EnableWeapon(invIndex);
        inventoryIndex = invIndex;
        UpdateItem();

        return true;
    }

    public void UpdateItem()
    {
        GameObject obj = inventory[inventoryIndex];
        obj.GetComponent<Weapon>().WeaponSwapped();
        statusBar.SetCurrentWeapon(inventoryIndex);
        foreach (GameObject g in inventory)
        {
            if (g == null)
                continue;

            if (g == obj)
                g.SetActive(true);
            else
                g.SetActive(false);
        }
    }
}
