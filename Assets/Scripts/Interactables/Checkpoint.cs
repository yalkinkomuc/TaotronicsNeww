using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [Header("UI Elements")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private SpriteRenderer checkpointLight;
    [SerializeField] private Color activeColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
    
    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        // Checkpoint daha önce aktifleştirilmişse ışığı ayarla
        if (PlayerPrefs.HasKey("CheckpointActivated") && PlayerPrefs.GetInt("CheckpointActivated") == 1)
        {
            isActivated = true;
            UpdateLightEffect();
        }
    }

    public void Interact()
    {
        if (!isActivated)
        {
            isActivated = true;
            UpdateLightEffect();
            SaveCheckpoint();
        }
        
        HealPlayer();
    }

    private void UpdateLightEffect()
    {
        if (checkpointLight != null)
        {
            checkpointLight.color = isActivated ? activeColor : inactiveColor;
        }
    }

    public void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            if (promptText != null)
                promptText.text = isActivated ? "Press E to Rest" : "Press E to Activate Checkpoint";
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void HealPlayer()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.stats.currentHealth = player.stats.maxHealth.GetValue();
            player.stats.currentMana = player.stats.maxMana.GetValue();
            
            if (player.healthBar != null)
                player.healthBar.UpdateHealthBar(player.stats.currentHealth, player.stats.maxHealth.GetValue());
        }
    }

    private void SaveCheckpoint()
    {
        // Checkpoint pozisyonunu kaydet
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.SetInt("CheckpointActivated", 1);

        // Item durumlarını kaydet
        SaveItemStates();
        
        PlayerPrefs.Save();
    }

    private void SaveItemStates()
    {
        // Bumerang durumunu kaydet
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            // Bumerang durumu
            if (player.boomerangWeapon != null)
            {
                PlayerPrefs.SetInt("HasBoomerang", player.boomerangWeapon.gameObject.activeInHierarchy ? 1 : 0);
            }

            // Spellbook durumu
            if (player.spellbookWeapon != null)
            {
                PlayerPrefs.SetInt("HasSpellbook", player.spellbookWeapon.gameObject.activeInHierarchy ? 1 : 0);
            }

            // Kılıç durumu
            if (player.swordWeapon != null)
            {
                PlayerPrefs.SetInt("HasSword", player.swordWeapon.gameObject.activeInHierarchy ? 1 : 0);
            }
        }
    }

    public static void LoadItemStates(Player player)
    {
        if (player == null) return;

        // Bumerang durumunu yükle
        if (player.boomerangWeapon != null)
        {
            bool hasBoomerang = PlayerPrefs.GetInt("HasBoomerang", 0) == 1;
            player.boomerangWeapon.gameObject.SetActive(hasBoomerang);
        }

        // Spellbook durumunu yükle
        if (player.spellbookWeapon != null)
        {
            bool hasSpellbook = PlayerPrefs.GetInt("HasSpellbook", 0) == 1;
            player.spellbookWeapon.gameObject.SetActive(hasSpellbook);
        }

        // Kılıç durumunu yükle
        if (player.swordWeapon != null)
        {
            bool hasSword = PlayerPrefs.GetInt("HasSword", 0) == 1;
            player.swordWeapon.gameObject.SetActive(hasSword);
        }
    }

    public bool IsActivated()
    {
        return isActivated;
    }
} 