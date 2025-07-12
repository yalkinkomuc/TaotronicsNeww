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
    
    private new void Awake()
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
                    Debug.LogWarning($"{weapon.itemName} için image bileşeni bulunamadı!");
                }
            }
            
            // Set button icon
            if (weapon.icon != null && buttonImage != null)
            {
                buttonImage.sprite = weapon.icon;
            }
            
            // Add click event
            string weaponName = weapon.itemName; // Create a local copy for the lambda
            button.onClick.AddListener(() => SelectWeapon(weaponName));
            
            // Store button reference
            weaponButtons.Add(weaponName, button);
            
            // Set tooltip
            // TooltipTrigger tooltip = buttonObj.GetComponent<TooltipTrigger>();
            // if (tooltip != null)
            // {
            //     tooltip.headerText = weapon.weaponName;
            //     tooltip.contentText = $"Seviye {weapon.level}/{weapon.maxLevel}\nHasar: +{weapon.GetCurrentDamageBonus()}";
            // }
        }
    }
    
    private void SelectWeapon(string weaponName)
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
            selectedWeapon = BlacksmithManager.Instance.GetWeapon(weaponName);
            
            if (selectedWeapon == null)
            {
                Debug.LogError($"Weapon with name {weaponName} not found!");
                return;
            }
            
            // Update UI - tüm UI elemanları için null kontrolü
            if (weaponNameText != null)
            {
                weaponNameText.text = selectedWeapon.itemName;
            }
            
            if (weaponIcon != null && selectedWeapon.icon != null)
            {
                weaponIcon.sprite = selectedWeapon.icon;
            }
            
            if (currentLevelText != null)
            {
                currentLevelText.text = $" {selectedWeapon.level}/{selectedWeapon.maxLevel}";
            }
            
            // Calculate current damage range (min normal - max critical)
            string currentDamageRange = CalculateWeaponDamageRange(selectedWeapon, false);
            
            if (currentDamageText != null)
            {
                currentDamageText.text = $" {currentDamageRange}";
            }
            
            // Calculate next level damage if not max level
            if (selectedWeapon.level < selectedWeapon.maxLevel)
            {
                string nextLevelDamageRange = CalculateWeaponDamageRange(selectedWeapon, true);
                
                if (nextLevelDamageText != null)
                {
                    nextLevelDamageText.text = $" {nextLevelDamageRange}";
                }
                
                // Set upgrade cost and materials
                int upgradeCost = selectedWeapon.GetNextUpgradeCost();
                
                // Get required materials
                var requiredMaterials = BlacksmithManager.Instance.GetRequiredMaterialsForDisplay(selectedWeapon.itemName);
                
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
            bool success = BlacksmithManager.Instance.UpgradeWeapon(selectedWeapon.itemName, playerStats);
            
            if (success)
            {
                // Update UI
                UpdateGoldText();
                
                // Refresh weapon selection
                SelectWeapon(selectedWeapon.itemName);
                
                // Show success message
                if (descriptionText != null)
                {
                    descriptionText.text = $"{selectedWeapon.itemName} başarıyla geliştirildi!";
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
                        var requiredMaterials = BlacksmithManager.Instance.GetRequiredMaterialsForDisplay(selectedWeapon.itemName);
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
    
    /// <summary>
    /// Calculate weapon damage range using PlayerStats method (same as AdvancedInventoryUI)
    /// Shows the actual total damage range including attributes and upgrades
    /// </summary>
    /// <param name="weapon">Weapon to calculate for</param>
    /// <param name="isNextLevel">If true, calculate for next level upgrade</param>
    /// <returns>Formatted damage range string</returns>
    private string CalculateWeaponDamageRange(WeaponData weapon, bool isNextLevel)
    {
        if (weapon == null || playerStats == null) return "0";
        
        if (isNextLevel)
        {
            // For next level preview, temporarily increment weapon level
            int originalLevel = weapon.level;
            if (originalLevel >= weapon.maxLevel) 
            {
                // Already at max level, return current damage
                return playerStats.GetDamageRangeWithCriticalString();
            }
            
            // Temporarily increase weapon level for preview
            weapon.level++;
            
            // Get damage range with upgraded weapon
            string nextLevelRange = playerStats.GetDamageRangeWithCriticalString();
            
            // Restore original level
            weapon.level = originalLevel;
            
            return nextLevelRange;
        }
        else
        {
            // For current level, use the same method as AdvancedInventoryUI
            return playerStats.GetDamageRangeWithCriticalString();
        }
    }
} 