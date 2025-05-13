using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlacksmithUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject blacksmithPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI goldText;
    
    [Header("Weapon Selection")]
    [SerializeField] private Transform weaponButtonContainer;
    [SerializeField] private GameObject weaponButtonPrefab;
    
    [Header("Upgrade UI")]
    [SerializeField] private GameObject upgradeSection;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI currentDamageText;
    [SerializeField] private TextMeshProUGUI nextLevelDamageText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image[] levelIndicators;
    
    [Header("Audio")]
    //[SerializeField] private AudioClip forgeSound;
    //[SerializeField] private AudioClip selectSound;
    //[SerializeField] private AudioClip upgradeSound;
    
    private PlayerStats playerStats;
    private WeaponData selectedWeapon;
    private Dictionary<string, Button> weaponButtons = new Dictionary<string, Button>();
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initially hide the panel
        if (blacksmithPanel != null)
        {
            blacksmithPanel.SetActive(false);
        }
    }
    
    public void OpenBlacksmith(PlayerStats stats)
    {
        playerStats = stats;
        
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats is null!");
            return;
        }
        
        // Show the panel
        blacksmithPanel.SetActive(true);
        
        
        
        // Update UI
        UpdateGoldText();
        
        // Create weapon buttons
        CreateWeaponButtons();
        
        // Hide upgrade section until a weapon is selected
        upgradeSection.SetActive(false);
        
        // Display default message
        descriptionText.text = "Silahını geliştirmek için seç.";
        
        // Time scale'i sıfıra ayarla (oyunu duraklat)
        Time.timeScale = 0f;
    }
    
    public void CloseBlacksmith()
    {
        // Hide the panel
        blacksmithPanel.SetActive(false);
        
        // Time scale'i normal hale getir (oyunu devam ettir)
        Time.timeScale = 1f;
    }
    
    private void CreateWeaponButtons()
    {
        
        // Null kontrolleri ekleyin
        if (BlacksmithManager.Instance == null)
        {
            Debug.LogError("BlacksmithManager.Instance null");
            return;
        }
    
        if (weaponButtonContainer == null)
        {
            Debug.LogError("weaponButtonContainer null");
            return;
        }
    
        if (weaponButtonPrefab == null)
        {
            Debug.LogError("weaponButtonPrefab null");
            return;
        }
        
        
        
        // Clear existing buttons
        foreach (Transform child in weaponButtonContainer)
        {
            Destroy(child.gameObject);
        }
        weaponButtons.Clear();
        
        // Get weapons from blacksmith manager
        List<WeaponData> weapons = BlacksmithManager.Instance.GetAllWeapons();
        
        // Silah listesi null kontrolü
        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogError("Silah listesi boş veya null!");
            return;
        }
        
        // Create buttons for each weapon
        foreach (WeaponData weapon in weapons)
        {
            // Her silah için null kontrolü yap
            if (weapon == null)
            {
                Debug.LogWarning("Null silah verisi atlandı!");
                continue;
            }
            
            GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // Set button icon
            if (weapon.weaponIcon != null)
            {
                button.image.sprite = weapon.weaponIcon;
            }
            
            // Add click event
            string weaponId = weapon.weaponId; // Create a local copy for the lambda
            button.onClick.AddListener(() => SelectWeapon(weaponId));
            
            // Store button reference
            weaponButtons.Add(weaponId, button);
            
            // Set tooltip
            // TooltipTrigger tooltip = buttonObj.GetComponent<TooltipTrigger>();
            // if (tooltip != null)
            // {
            //     tooltip.headerText = weapon.weaponName;
            //     tooltip.contentText = $"Seviye {weapon.level}/{weapon.maxLevel}\nHasar: +{weapon.GetCurrentDamageBonus()}";
            // }
        }
    }
    
    private void SelectWeapon(string weaponId)
    {
       
        
        // Get weapon data
        selectedWeapon = BlacksmithManager.Instance.GetWeapon(weaponId);
        
        if (selectedWeapon == null)
        {
            Debug.LogError($"Weapon with ID {weaponId} not found!");
            return;
        }
        
        // Update UI
        weaponNameText.text = selectedWeapon.weaponName;
        
        if (selectedWeapon.weaponIcon != null)
        {
            weaponIcon.sprite = selectedWeapon.weaponIcon;
        }
        
        currentLevelText.text = $"Seviye {selectedWeapon.level}/{selectedWeapon.maxLevel}";
        
        // Update damage texts
        float currentDamage = selectedWeapon.GetCurrentDamageBonus();
        currentDamageText.text = $"Mevcut Hasar Bonusu: +{currentDamage}";
        
        // Calculate next level damage if not max level
        if (selectedWeapon.level < selectedWeapon.maxLevel)
        {
            float nextLevelDamage = selectedWeapon.baseDamageBonus + 
                                   (selectedWeapon.upgradeDamageIncrement * selectedWeapon.level);
            nextLevelDamageText.text = $"Sonraki Seviye: +{nextLevelDamage}";
            
            // Set upgrade cost
            int upgradeCost = selectedWeapon.GetNextUpgradeCost();
            upgradeCostText.text = $"Geliştirme Bedeli: {upgradeCost} Altın";
            
            // Enable or disable upgrade button based on player's gold
            upgradeButton.interactable = playerStats.gold >= upgradeCost;
        }
        else
        {
            // Max level reached
            nextLevelDamageText.text = "Maksimum seviyeye ulaşıldı!";
            upgradeCostText.text = "";
            upgradeButton.interactable = false;
        }
        
        // Update level indicators
        for (int i = 0; i < levelIndicators.Length; i++)
        {
            if (i < selectedWeapon.level)
            {
                levelIndicators[i].color = Color.yellow;
            }
            else
            {
                levelIndicators[i].color = Color.gray;
            }
        }
        
        // Show upgrade section
        upgradeSection.SetActive(true);
    }
    
    public void UpgradeSelectedWeapon()
    {
        if (selectedWeapon == null || playerStats == null)
            return;
            
        // Try to upgrade
        bool success = BlacksmithManager.Instance.UpgradeWeapon(selectedWeapon.weaponId, playerStats);
        
        if (success)
        {
            
            
            // Update UI
            UpdateGoldText();
            
            // Refresh weapon selection
            SelectWeapon(selectedWeapon.weaponId);
            
            // Show success message
            descriptionText.text = $"{selectedWeapon.weaponName} başarıyla geliştirildi!";
        }
        else
        {
            // Show error message
            descriptionText.text = "Geliştirme başarısız. Yeterli altın yok veya maksimum seviyeye ulaşıldı.";
        }
    }
    
    private void UpdateGoldText()
    {
        if (goldText != null && playerStats != null)
        {
            goldText.text = $"{playerStats.gold} Altın";
        }
    }
} 