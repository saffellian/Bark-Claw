using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField][Range(1,10)] private int inventorySize = 2;
    [SerializeField] private List<GameObject> inventory;
    [SerializeField] private GameObject defaultItem;
    [SerializeField] private float weaponSwitchDelay = 0.2f;

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
                inventory[i] = null;
            }
        }

        inventoryIndex = 0;
        UpdateItem();
    }

    public bool TryAddItem(WeaponPickup pickup, int invIndex)
    {
        if (inventory[invIndex] != null)
        {
            Weapon weapon = pickup.itemPrefab.GetComponent<Weapon>();
            Debug.Log(weapon.GetAmmoAmount());
            if (pickup.overridePickupAmmo)
                inventory[invIndex].GetComponent<Weapon>().AddAmmo(pickup.overrideAmmoAmount, inventoryIndex == invIndex);
            else
                inventory[invIndex].GetComponent<Weapon>().AddAmmo(weapon.GetAmmoAmount(), inventoryIndex == invIndex);
            return true;
        }

        if (inventoryIndex == invIndex)
            return false; // don't replace current item

        inventory[invIndex] = Instantiate(pickup.itemPrefab, gameObject.transform.GetChild(0));
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
