using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusBar : MonoBehaviour
{
    [SerializeField] private Animator dogHeadAnimator;
    [SerializeField] private Color active, inactive, disabled;
    [SerializeField] private Transform numbersParent;
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private TextMeshProUGUI ammoDisplay;

    private TextMeshProUGUI[] numbers;

    void Start()
    {
        PlayerHealth ph = PlayerHealth.Instance;
        ph.onDamaged.AddListener(PlayerDamaged);
        ph.onDeath.AddListener(PlayerDied);

        Weapon.onAmmoUpdate.AddListener(AmmoUpdated);

        numbers = numbersParent.GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var num in numbers)
        {
            num.color = disabled;
        }

        numbers[0].color = active;
    }

    public void SetCurrentWeapon(int index)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            if (i == index && numbers[i].color == inactive)
                numbers[i].color = active;
            else if (numbers[i].color == active)
                numbers[i].color = inactive;
        }
    }

    private void PlayerDamaged(int currHealth)
    {
        healthDisplay.text = currHealth.ToString();
        dogHeadAnimator.SetTrigger("Hurt");
    }

    private void PlayerDied()
    {
        dogHeadAnimator.SetBool("Dead", true);
    }

    private void AmmoUpdated(int amount)
    {
        if (amount == -1) // melee weapon type
            ammoDisplay.text = string.Empty;
        else
            ammoDisplay.text = amount.ToString();
    }

    public void EnableWeapon(int index)
    {
        numbers[index].color = inactive;
    }

    public void DisableWeapon(int index)
    {
        numbers[index].color = disabled;
    }
}
