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
    
    [Header("References")]
    [SerializeField] private AttributesUpgradePanel attributesPanel;
    [SerializeField] private CheckpointSelectionScreen selectionScreen;
    
    // Statik referanslar
    public static AttributesUpgradePanel persistentUpgradePanel;
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
        
        FindOrCreateUIElements();
    }
    
    private void FindOrCreateUIElements()
    {
        // Önce statik referanslara bak
        if (persistentUpgradePanel != null)
            attributesPanel = persistentUpgradePanel;
        
        if (persistentSelectionScreen != null)
            selectionScreen = persistentSelectionScreen;
        
        // AttributesUpgradePanel bulma
        if (attributesPanel == null)
        {
            attributesPanel = FindFirstObjectByType<AttributesUpgradePanel>();
            if (attributesPanel == null)
                Debug.LogWarning("AttributesUpgradePanel bulunamadı!");
        }
        
        // Selection Screen bulma
        if (selectionScreen == null)
        {
            selectionScreen = FindFirstObjectByType<CheckpointSelectionScreen>();
            if (selectionScreen == null)
                Debug.LogWarning("CheckpointSelectionScreen bulunamadı!");
        }
        
        // AttributesUpgradePanel'i statik referansa ata
        if (attributesPanel != null && persistentUpgradePanel == null)
            persistentUpgradePanel = attributesPanel;
            
        // Selection Screen'i statik referansa ata
        if (selectionScreen != null && persistentSelectionScreen == null)
            persistentSelectionScreen = selectionScreen;
    }

    public GameObject Player { get; set; }
    public bool CanInteract { get; set; }

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
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
        else
        {
            // Eğer selection screen yoksa, doğrudan iyileştir ve attribute panelini göster
            HealPlayer();
            ShowAttributesPanel();
        }
    }
    
    private void ShowAttributesPanel()
    {
        if (attributesPanel == null)
        {
            Debug.LogWarning("AttributesUpgradePanel bulunamadı!");
            return;
        }
        
        Player player = PlayerManager.instance?.player;
        if (player == null)
            return;
            
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
            return;
            
        // AttributesUpgradePanel'ın aktifliğini kontrol et ve etkinleştir
        if (!attributesPanel.gameObject.activeSelf)
        {
            attributesPanel.gameObject.SetActive(true);
        }
        
        // Etkileşim panelini gizle
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void UpdateLightEffect()
    {
        if (checkpointLight != null)
            checkpointLight.color = isActivated ? activeColor : inactiveColor;
    }

    public void ShowInteractionPrompt()
    {
        // Eğer attribute paneli veya selection screen açıksa, etkileşim isteğini gösterme
        if ((attributesPanel != null && attributesPanel.gameObject.activeSelf) ||
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
        Player player = PlayerManager.instance?.player;
        if (player == null) return;
        
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            // Tam sayı değerlere yuvarla
            float roundedMaxHealth = Mathf.Round(playerStats.maxHealth.GetValue());
            float roundedMaxMana = Mathf.Round(playerStats.maxMana.GetValue());
            
            // Can ve manayı doldur (Current health MAX health'e eşitle)
            playerStats.SetHealth(roundedMaxHealth);
            playerStats.currentMana = roundedMaxMana;
            
            // Health bar'ı güncelle
            if (player.healthBar != null)
                player.healthBar.UpdateHealthBar(playerStats.currentHealth, playerStats.maxHealth.GetValue());
            else
            {
                // Force reinitialize health bar if reference is lost
                HealthBar healthBar = player.GetComponent<HealthBar>();
                if (healthBar != null)
                {
                    healthBar.ForceReinitialize();
                }
            }
            
            // Force mana bar update as well
            ManaBar manaBar = player.GetComponent<ManaBar>();
            if (manaBar != null)
            {
                manaBar.UpdateManaBar(playerStats.currentMana, playerStats.maxMana.GetValue());
            }
            
          
                
           
        }
        else if (player.stats != null)
        {
            // Basit iyileştirme
            float roundedMaxHealth = Mathf.Round(player.stats.maxHealth.GetValue());
            float roundedMaxMana = Mathf.Round(player.stats.maxMana.GetValue());
            
            // Can ve manayı doldur (Current health MAX health'e eşitle)
            player.stats.SetHealth(roundedMaxHealth);
            player.stats.currentMana = roundedMaxMana;
            
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
        
        // Şu anki sahne indeksini kaydet
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("CheckpointSceneIndex", currentSceneIndex);
        
        // Oyuncu stat değerlerini kaydet
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Temel değerleri kaydet
                PlayerPrefs.SetInt("PlayerLevel", playerStats.GetLevel());
                PlayerPrefs.SetFloat("PlayerMaxHealth", playerStats.maxHealth.GetValue());
                PlayerPrefs.SetFloat("PlayerMaxMana", playerStats.maxMana.GetValue());
                PlayerPrefs.SetFloat("PlayerBaseDamage", playerStats.baseDamage.GetValue());
                PlayerPrefs.SetInt("PlayerSkillPoints", playerStats.AvailableSkillPoints);
                
                // Attribute değerlerini kaydet
                PlayerPrefs.SetInt("PlayerVitality", playerStats.Vitality);
                PlayerPrefs.SetInt("PlayerMight", playerStats.Might);
                PlayerPrefs.SetInt("PlayerMind", playerStats.Mind);
                PlayerPrefs.SetInt("PlayerDefense", playerStats.Defense);
                PlayerPrefs.SetInt("PlayerLuck", playerStats.Luck);
                
                // Experience değerlerini kaydet
                System.Type type = playerStats.GetType();
                int experience = (int)type.GetField("experience", System.Reflection.BindingFlags.Instance | 
                                                 System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
                int experienceToNextLevel = (int)type.GetField("experienceToNextLevel", System.Reflection.BindingFlags.Instance | 
                                                            System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
                
                PlayerPrefs.SetInt("PlayerExperience", experience);
                PlayerPrefs.SetInt("PlayerExperienceToNextLevel", experienceToNextLevel);
            }
        }

        // Item durumlarını kaydet
        SaveItemStates();
        
        PlayerPrefs.Save();
    }

    private void SaveItemStates()
    {
        Player player = PlayerManager.instance?.player;
        if (player == null) return;
        
        // Persist equipped weapons using EquipmentManager if available
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.SaveEquipment();
        }
        
        // PlayerWeaponManager unlock durumları zaten PlayerWeaponManager.SaveUnlockStates() ile kaydediliyor
        // Eski silah durumu kaydetme sistemi kaldırıldı - yeni unlock sistemi kullanılıyor
        PlayerWeaponManager weaponManager = player.GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.SaveWeaponState();
        }
    }

    public static void LoadItemStates(Player player)
    {
        if (player == null) return;

        // Silah durumları artık PlayerWeaponManager unlock sistemi ile yönetiliyor
        // LoadWeaponState() metodu zaten çağrıldığı için burada ek bir işlem yapmaya gerek yok
        
        // PlayerWeaponManager unlock durumlarını yükle
        PlayerWeaponManager weaponManager = player.GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.LoadWeaponState();
        }
        else
        {
            Debug.LogWarning("[Checkpoint] PlayerWeaponManager not found during LoadItemStates");
        }
    }

    public bool IsActivated()
    {
        return isActivated;
    }
    
    // Kaydedilmiş checkpoint sahne indeksini döndürür
    public static int GetSavedSceneIndex()
    {
        return PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
    }
} 