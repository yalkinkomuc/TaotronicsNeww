using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlacksmithUI : BaseUIPanel
{
    // Singleton instance
    public static BlacksmithUI Instance;
    
    [Header("UI References")]
    [SerializeField] private GameObject blacksmithPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI goldText;
    
    [Header("Weapon Selection")]
    [SerializeField] private Transform weaponButtonContainer;
    [SerializeField] private GameObject weaponButtonPrefab;
    
    [Header("Weapon Info")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI currentDamageText;
    [SerializeField] private TextMeshProUGUI nextLevelDamageText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private Button closeButton;
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
        // Singleton pattern uygula
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSource kontrolü
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Kapatma butonu için olay ekle
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseBlacksmith);
        }
        

        
        // Initially hide the panel
        if (blacksmithPanel != null)
        {
            blacksmithPanel.SetActive(false);
        }
    }
    
    public void OpenBlacksmith(PlayerStats stats)
    {
        try
        {
            playerStats = stats;
            
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats is null!");
                return;
            }
            
            // Show the panel
            if (blacksmithPanel != null)
            {
                blacksmithPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("blacksmithPanel null!");
                return;
            }
            
            // Update UI
            UpdateGoldText();
            
            // Silinecek weaponInfoUI referansını kaldır
            ClearWeaponInfo();
            
            // Display default message
            if (descriptionText != null)
            {
                descriptionText.text = "Silahını geliştirmek için seç.";
            }
            
            try
            {
                // Create weapon buttons
                CreateWeaponButtons();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CreateWeaponButtons hata verdi: {e.Message}\n{e.StackTrace}");
            }
            
            // Time scale'i sıfıra ayarla (oyunu duraklat)
            Time.timeScale = 0f;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OpenBlacksmith hata verdi: {e.Message}\n{e.StackTrace}");
            // Hata olsa da oyunu devam ettir
            Time.timeScale = 1f;
        }
    }
    
    public void CloseBlacksmith()
    {
        // Hide the panel
        blacksmithPanel.SetActive(false);
        
        // Time scale'i normal hale getir (oyunu devam ettir)
        Time.timeScale = 1f;
    }
    
    // Silah bilgilerini temizle
    private void ClearWeaponInfo()
    {
        if (weaponNameText != null) weaponNameText.text = "";
        if (currentLevelText != null) currentLevelText.text = "";
        if (currentDamageText != null) currentDamageText.text = "";
        if (nextLevelDamageText != null) nextLevelDamageText.text = "";
        if (upgradeCostText != null) upgradeCostText.text = "";
        if (upgradeButton != null) upgradeButton.interactable = false;
        
        // Seviye göstergelerini sıfırla
        if (levelIndicators != null)
        {
            foreach (var indicator in levelIndicators)
            {
                if (indicator != null)
                {
                    indicator.color = Color.gray;
                }
            }
        }
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
            
            // Button için null kontrolü
            if (button == null)
            {
                Debug.LogError("Buton bileşenini bulunamadı!");
                continue;
            }
            
            // Image bileşenini alma
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage == null)
            {
                // Eğer butonun kendi image'i yoksa, child bir image bileşeni aramayı dene
                buttonImage = buttonObj.GetComponentInChildren<Image>();
                
                if (buttonImage == null)
                {
                    Debug.LogWarning($"{weapon.weaponName} için image bileşeni bulunamadı!");
                }
            }
            
            // Set button icon
            if (weapon.weaponIcon != null && buttonImage != null)
            {
                buttonImage.sprite = weapon.weaponIcon;
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
        try
        {
            // BlacksmithManager kontrolü
            if (BlacksmithManager.Instance == null)
            {
                Debug.LogError("BlacksmithManager.Instance silah seçiminde null!");
                return;
            }
            
            // Get weapon data
            selectedWeapon = BlacksmithManager.Instance.GetWeapon(weaponId);
            
            if (selectedWeapon == null)
            {
                Debug.LogError($"Weapon with ID {weaponId} not found!");
                return;
            }
            
            // Update UI - tüm UI elemanları için null kontrolü
            if (weaponNameText != null)
            {
                weaponNameText.text = selectedWeapon.weaponName;
            }
            
            if (weaponIcon != null && selectedWeapon.weaponIcon != null)
            {
                weaponIcon.sprite = selectedWeapon.weaponIcon;
            }
            
            if (currentLevelText != null)
            {
                currentLevelText.text = $" {selectedWeapon.level}/{selectedWeapon.maxLevel}";
            }
            
            // Get the appropriate base damage based on weapon type
            float baseDamage;
            switch (selectedWeapon.weaponType)
            {
                case WeaponType.Sword:
                    baseDamage = playerStats.baseDamage.GetBaseValue();
                    break;
                case WeaponType.Boomerang:
                    baseDamage = playerStats.boomerangDamage.GetBaseValue();
                    break;
                case WeaponType.Spellbook:
                    baseDamage = playerStats.spellbookDamage.GetBaseValue();
                    break;
                default:
                    baseDamage = playerStats.baseDamage.GetBaseValue();
                    break;
            }
            
            // Calculate current damage (base damage + might bonus + current weapon bonus)
            float mightBonus = playerStats.CalculateDamageBonusForMight(playerStats.Might);
            float currentWeaponBonus = selectedWeapon.GetCurrentDamageBonus();
            float totalCurrentDamage = baseDamage + mightBonus + currentWeaponBonus;
            
            if (currentDamageText != null)
            {
                currentDamageText.text = $" {totalCurrentDamage}";
            }
            
            // Calculate next level damage if not max level
            if (selectedWeapon.level < selectedWeapon.maxLevel)
            {
                float nextLevelWeaponBonus = selectedWeapon.baseDamageBonus + 
                                           (selectedWeapon.upgradeDamageIncrement * selectedWeapon.level);
                float totalNextLevelDamage = baseDamage + mightBonus + nextLevelWeaponBonus;
                
                if (nextLevelDamageText != null)
                {
                    nextLevelDamageText.text = $" {totalNextLevelDamage}";
                }
                
                // Set upgrade cost and materials
                int upgradeCost = selectedWeapon.GetNextUpgradeCost();
                
                // Get required materials
                var requiredMaterials = BlacksmithManager.Instance.GetRequiredMaterialsForDisplay(selectedWeapon.weaponId);
                
                if (upgradeCostText != null)
                {
                    string costText = $" {upgradeCost} Altın";
                    
                    // Add required materials to the text
                    if (requiredMaterials.Count > 0)
                    {
                        costText += "\nMateryaller:";
                        foreach (var material in requiredMaterials)
                        {
                            int available = GetMaterialCountInInventory(material.Key);
                            bool hasEnough = available >= material.Value;
                            string color = hasEnough ? "white" : "red";
                            
                            costText += $"\n<color={color}>{GetMaterialDisplayName(material.Key)}: {available}/{material.Value}</color>";
                        }
                    }
                    
                    upgradeCostText.text = costText;
                }
                
                // Enable or disable upgrade button based on player's gold and materials
                if (upgradeButton != null && playerStats != null)
                {
                    bool hasEnoughGold = playerStats.gold >= upgradeCost;
                    bool hasEnoughMaterials = CheckHasRequiredMaterials(requiredMaterials);
                    
                    upgradeButton.interactable = hasEnoughGold && hasEnoughMaterials;
                }
            }
            else
            {
                // Max level reached
                if (nextLevelDamageText != null)
                {
                    nextLevelDamageText.text = "Maksimum seviyeye ulaşıldı!";
                }
                
                if (upgradeCostText != null)
                {
                    upgradeCostText.text = "";
                }
                
                if (upgradeButton != null)
                {
                    upgradeButton.interactable = false;
                }
            }
            
            // Update level indicators if they exist
            if (levelIndicators != null && levelIndicators.Length > 0)
            {
                for (int i = 0; i < levelIndicators.Length; i++)
                {
                    if (levelIndicators[i] != null)
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
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SelectWeapon hata verdi: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public void UpgradeSelectedWeapon()
    {
        try
        {
            if (selectedWeapon == null)
            {
                Debug.LogError("selectedWeapon null!");
                return;
            }
            
            if (playerStats == null)
            {
                Debug.LogError("playerStats null!");
                return;
            }
                
            if (BlacksmithManager.Instance == null)
            {
                Debug.LogError("BlacksmithManager.Instance null!");
                return;
            }
            
            // Try to upgrade
            bool success = BlacksmithManager.Instance.UpgradeWeapon(selectedWeapon.weaponId, playerStats);
            
            if (success)
            {
                // Update UI
                UpdateGoldText();
                
                // Refresh weapon selection
                SelectWeapon(selectedWeapon.weaponId);
                
                // Show success message
                if (descriptionText != null)
                {
                    descriptionText.text = $"{selectedWeapon.weaponName} başarıyla geliştirildi!";
                }
            }
            else
            {
                // Show detailed error message
                if (descriptionText != null)
                {
                    string errorMessage = "Geliştirme başarısız. ";
                    
                    // Check specific reasons for failure
                    if (selectedWeapon.level >= selectedWeapon.maxLevel)
                    {
                        errorMessage += "Maksimum seviyeye ulaşıldı.";
                    }
                    else
                    {
                        int upgradeCost = selectedWeapon.GetNextUpgradeCost();
                        bool hasEnoughGold = playerStats.gold >= upgradeCost;
                        var requiredMaterials = BlacksmithManager.Instance.GetRequiredMaterialsForDisplay(selectedWeapon.weaponId);
                        bool hasEnoughMaterials = CheckHasRequiredMaterials(requiredMaterials);
                        
                        if (!hasEnoughGold && !hasEnoughMaterials)
                        {
                            errorMessage += "Yeterli altın ve materyal yok.";
                        }
                        else if (!hasEnoughGold)
                        {
                            errorMessage += "Yeterli altın yok.";
                        }
                        else if (!hasEnoughMaterials)
                        {
                            errorMessage += "Yeterli materyal yok.";
                        }
                    }
                    
                    descriptionText.text = errorMessage;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UpgradeSelectedWeapon hata verdi: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void UpdateGoldText()
    {
        if (goldText != null && playerStats != null)
        {
            goldText.text = $"{playerStats.gold} Altın";
        }
    }
    
    // Helper method to get material count in inventory
    private int GetMaterialCountInInventory(MaterialType materialType)
    {
        if (Inventory.instance == null)
            return 0;
            
        int totalCount = 0;
        
        foreach (var inventoryItem in Inventory.instance.inventoryItems)
        {
            if (inventoryItem.data is UpgradeMaterialData material && 
                material.materialType == materialType)
            {
                totalCount += inventoryItem.stackSize;
            }
        }
        
        return totalCount;
    }
    
    // Helper method to get display name for materials
    private string GetMaterialDisplayName(MaterialType materialType)
    {
        switch (materialType)
        {
            case MaterialType.Leather: return "Deri";
            case MaterialType.Iron: return "Demir";
            case MaterialType.Rock: return "Taş";
            case MaterialType.Diamond: return "Elmas";
            case MaterialType.Crystal: return "Kristal";
            case MaterialType.Gem: return "Mücevher";
            default: return materialType.ToString();
        }
    }
    
    // Helper method to check if player has required materials
    private bool CheckHasRequiredMaterials(Dictionary<MaterialType, int> requiredMaterials)
    {
        foreach (var requirement in requiredMaterials)
        {
            int availableCount = GetMaterialCountInInventory(requirement.Key);
            if (availableCount < requirement.Value)
            {
                return false;
            }
        }
        
        return true;
    }
} 