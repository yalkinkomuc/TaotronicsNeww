using UnityEngine;
using System.Collections.Generic;

public class TestInventorySystem : MonoBehaviour
{
    [Header("Test Items")]
    [SerializeField] private List<ItemData> testItems = new List<ItemData>();
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode openInventoryKey = KeyCode.Tab;
    [SerializeField] private KeyCode addRandomItemKey = KeyCode.F1;
    [SerializeField] private KeyCode addTestEquipmentKey = KeyCode.F2;
    [SerializeField] private KeyCode addTestRunesKey = KeyCode.F3;
    [SerializeField] private KeyCode addTestMaterialsKey = KeyCode.F4;
    [SerializeField] private KeyCode addTestCollectiblesKey = KeyCode.F5;
    
    private void Update()
    {
        HandleTestInputs();
    }
    
    private void HandleTestInputs()
    {
        // Open/Close inventory
        if (Input.GetKeyDown(openInventoryKey))
        {
            ToggleInventory();
        }
        
        // Add random item
        if (Input.GetKeyDown(addRandomItemKey))
        {
            AddRandomItem();
        }
        
        // Add test equipment
        if (Input.GetKeyDown(addTestEquipmentKey))
        {
            AddTestEquipment();
        }
        
        // Add test runes
        if (Input.GetKeyDown(addTestRunesKey))
        {
            AddTestRunes();
        }
        
        // Add test materials
        if (Input.GetKeyDown(addTestMaterialsKey))
        {
            AddTestMaterials();
        }
        
        // Add test collectibles
        if (Input.GetKeyDown(addTestCollectiblesKey))
        {
            AddTestCollectibles();
        }
    }
    
    private void ToggleInventory()
    {
        if (AdvancedInventoryUI.Instance != null)
        {
            if (AdvancedInventoryUI.Instance.gameObject.activeInHierarchy)
            {
                AdvancedInventoryUI.Instance.CloseInventory();
            }
            else
            {
                AdvancedInventoryUI.Instance.OpenInventory();
            }
        }
        else
        {
            Debug.LogWarning("AdvancedInventoryUI.Instance is null!");
        }
    }
    
    private void AddRandomItem()
    {
        if (testItems.Count == 0)
        {
            Debug.LogWarning("No test items assigned!");
            return;
        }
        
        ItemData randomItem = testItems[Random.Range(0, testItems.Count)];
        
        if (Inventory.instance != null && randomItem != null)
        {
            Inventory.instance.AddItem(randomItem);
            Debug.Log($"Added random item: {randomItem.itemName}");
        }
    }
    
    private void AddTestEquipment()
    {
        // Create test weapon
        var testWeapon = CreateTestWeapon();
        if (testWeapon != null && Inventory.instance != null)
        {
            Inventory.instance.AddItem(testWeapon);
            Debug.Log("Added test weapon");
        }
        
        // Create test armor
        var testArmor = CreateTestArmor();
        if (testArmor != null && Inventory.instance != null)
        {
            Inventory.instance.AddItem(testArmor);
            Debug.Log("Added test armor");
        }
    }
    
    private void AddTestRunes()
    {
        var testRune = CreateTestRune();
        if (testRune != null && Inventory.instance != null)
        {
            for (int i = 0; i < 3; i++) // Add 3 runes
            {
                Inventory.instance.AddItem(testRune);
            }
            Debug.Log("Added test runes");
        }
    }
    
    private void AddTestMaterials()
    {
        var testMaterial = CreateTestMaterial();
        if (testMaterial != null && Inventory.instance != null)
        {
            for (int i = 0; i < 10; i++) // Add 10 materials
            {
                Inventory.instance.AddItem(testMaterial);
            }
            Debug.Log("Added test materials");
        }
    }
    
    private void AddTestCollectibles()
    {
        var testCollectible = CreateTestCollectible();
        if (testCollectible != null && Inventory.instance != null)
        {
            Inventory.instance.AddItem(testCollectible);
            Debug.Log("Added test collectible");
        }
    }
    
    // Create test items programmatically
    private EquipmentWeaponData CreateTestWeapon()
    {
        var weapon = ScriptableObject.CreateInstance<EquipmentWeaponData>();
        weapon.itemName = "Test Sword";
        weapon.description = "A basic test sword";
        weapon.itemType = ItemType.Weapon;
        weapon.rarity = ItemRarity.Common;
        weapon.weaponType = EquipmentWeaponType.Sword;
        weapon.minDamage = 10;
        weapon.maxDamage = 15;
        weapon.requiredLevel = 1;
        weapon.statModifiers.Add(new EquipmentStatModifier(StatType.WarriorDamage, 5));
        return weapon;
    }
    
    private ArmorData CreateTestArmor()
    {
        var armor = ScriptableObject.CreateInstance<ArmorData>();
        armor.itemName = "Test Armor";
        armor.description = "Basic test armor";
        armor.itemType = ItemType.Armor;
        armor.rarity = ItemRarity.Common;
        armor.armorType = ArmorType.Leather;
        armor.armorValue = 8;
        armor.requiredLevel = 1;
        armor.statModifiers.Add(new EquipmentStatModifier(StatType.Armor, 8));
        return armor;
    }
    
    private RuneData CreateTestRune()
    {
        var rune = ScriptableObject.CreateInstance<RuneData>();
        rune.itemName = "Test Vitality Rune";
        rune.description = "Increases health";
        rune.itemType = ItemType.Rune;
        rune.rarity = ItemRarity.Common;
        rune.runeType = RuneType.Vitality;
        rune.runeLevel = 1;
        rune.statModifier = new EquipmentStatModifier(StatType.Health, 25);
        return rune;
    }
    
    private UpgradeMaterialData CreateTestMaterial()
    {
        var material = ScriptableObject.CreateInstance<UpgradeMaterialData>();
        material.itemName = "Iron Ore";
        material.description = "Basic crafting material";
        material.itemType = ItemType.UpgradeMaterial;
        material.rarity = ItemRarity.Common;
        material.materialType = MaterialType.Iron;
        material.materialRarity = MaterialRarity.Common;
        material.enhancementPower = 1;
        return material;
    }
    
    private CollectibleData CreateTestCollectible()
    {
        var collectible = ScriptableObject.CreateInstance<CollectibleData>();
        collectible.itemName = "Ancient Coin";
        collectible.description = "An old coin from a forgotten empire";
        collectible.itemType = ItemType.Collectible;
        collectible.rarity = ItemRarity.Rare;
        collectible.collectibleType = CollectibleType.Coin;
        collectible.category = CollectibleCategory.Ancient_Artifacts;
        collectible.setName = "Lost Empire Collection";
        collectible.discoveryLocation = "Ruins of Valeria";
        collectible.loreText = "This coin bears the mark of the lost empire of Valeria, said to have fallen over a thousand years ago.";
        return collectible;
    }
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Advanced Inventory System Test");
        GUILayout.Label($"Press {openInventoryKey} to open/close inventory");
        GUILayout.Label($"Press {addRandomItemKey} to add random item");
        GUILayout.Label($"Press {addTestEquipmentKey} to add test equipment");
        GUILayout.Label($"Press {addTestRunesKey} to add test runes");
        GUILayout.Label($"Press {addTestMaterialsKey} to add test materials");
        GUILayout.Label($"Press {addTestCollectiblesKey} to add test collectibles");
        GUILayout.EndArea();
    }
} 