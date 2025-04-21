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
    
    [Header("Upgrade Panel")]
    [SerializeField] private UpgradePanel upgradePanel;
    
    [Header("Selection Screen")]
    [SerializeField] private CheckpointSelectionScreen selectionScreen;
    
    // Statik referanslar ekleyelim ki sahneler arasında kaybolmasın
    public static UpgradePanel persistentUpgradePanel;
    public static CheckpointSelectionScreen persistentSelectionScreen;
    
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
        
        // Kalıcı panelleri bul veya oluştur
        FindOrCreateUIElements();
    }
    
    private void FindOrCreateUIElements()
    {
        // Önce statik referanslara bak
        if (persistentUpgradePanel != null)
        {
            upgradePanel = persistentUpgradePanel;
        }
        
        if (persistentSelectionScreen != null)
        {
            selectionScreen = persistentSelectionScreen;
        }
        
        // Upgrade Panel bulma veya yaratma
        if (upgradePanel == null)
        {
            upgradePanel = FindFirstObjectByType<UpgradePanel>();
            
            if (upgradePanel == null)
            {
                Debug.LogWarning("UpgradePanel bulunamadı! Lütfen sahneye UpgradePanel ekleyin.");
            }
        }
        
        // Selection Screen bulma veya yaratma
        if (selectionScreen == null)
        {
            selectionScreen = FindFirstObjectByType<CheckpointSelectionScreen>();
            
            if (selectionScreen == null)
            {
                Debug.LogWarning("CheckpointSelectionScreen bulunamadı! Lütfen sahneye CheckpointSelectionScreen ekleyin.");
            }
        }
        
        // Eğer selection screen ve upgrade panel varsa, aralarındaki bağlantıyı kur
        if (selectionScreen != null && upgradePanel != null)
        {
            selectionScreen.upgradePanel = upgradePanel;
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
        
        // Selection Screen'i göster
        if (selectionScreen != null)
        {
            selectionScreen.ShowPanel();
            
            // Etkileşim promptunu gizle
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
        else
        {
            // Eğer selection screen yoksa, doğrudan iyileştir ve upgrade panelini göster (eski davranış)
            HealPlayer();
            ShowUpgradePanel();
        }
    }
    
    private void ShowUpgradePanel()
    {
        // Eğer panel veya oyuncu yoksa işlem yapma
        if (upgradePanel == null)
            return;
        
        // Oyuncuyu bul
        Player player = PlayerManager.instance.player;
        if (player == null)
            return;
            
        // PlayerStats'i al 
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
            return;
            
        // Paneli göster
        upgradePanel.Show(playerStats);
        
        // Etkileşim promtunu gizle
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
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
        // Eğer upgrade paneli veya selection screen açıksa, etkileşim isteğini gösterme
        if ((upgradePanel != null && upgradePanel.gameObject.activeSelf) ||
            (selectionScreen != null && selectionScreen.gameObject.activeSelf))
            return;
            
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
        Player player = PlayerManager.instance.player;
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
        // Checkpoint'i aktifleştir ve konumunu kaydet
        PlayerPrefs.SetInt("CheckpointActivated", 1);
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);

        // Item durumlarını kaydet
        SaveItemStates();
        
        PlayerPrefs.Save();
    }

    private void SaveItemStates()
    {
        // Bumerang durumunu kaydet
        Player player = PlayerManager.instance.player;
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