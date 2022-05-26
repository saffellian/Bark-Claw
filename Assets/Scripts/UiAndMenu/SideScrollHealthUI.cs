using UnityEngine;

public class SideScrollHealthUI : MonoBehaviour
{
    private PlayerHealth playerHealth;

    [Tooltip("Place life sprites in order from lowest to highest.")]
    [SerializeField] private SpriteRenderer[] lifeSprites;
    [Range(0, 1)]
    [Tooltip("Alpha value to use for disabled health icons.")]
    [SerializeField] private float missingHealthAlpha = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerHealth.onDamaged.AddListener(PlayerDamaged);
        playerHealth.onHealed.AddListener(PlayerHealed);
    }

    void PlayerDamaged(int remainingHealth)
    {
        Color currColor;
        for (int i = 0; i < lifeSprites.Length; i++)
        {
            currColor = lifeSprites[i].color;
            currColor.a = (i + 1 <= remainingHealth) ? 1 : missingHealthAlpha;
            lifeSprites[i].color = currColor;
        }
    }

    void PlayerHealed(int remainingHealth)
    {
        Color currColor;
        for (int i = 0; i < lifeSprites.Length; i++)
        {
            currColor = lifeSprites[i].color;
            currColor.a = (i + 1 <= remainingHealth) ? 1 : missingHealthAlpha;
            lifeSprites[i].color = currColor;
        }
    }
}
