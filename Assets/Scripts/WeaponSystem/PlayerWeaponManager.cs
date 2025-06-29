using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public WeaponStateMachine[] weapons;
    private int currentSecondaryWeaponIndex = 1; // 1'den başlıyoruz çünkü 0 kılıç
    
    [SerializeField] private KeyCode weaponSwitchKey = KeyCode.Tab; // Silah değiştirme tuşu
    
    private Player player;
    private PlayerStats playerStats;

    void Start()
    {
        if (weapons.Length < 2)
        {

            return;
        }
        
        // Get player references
        player = GetComponent<Player>();
        playerStats = GetComponent<PlayerStats>();

        // Kılıcı aktif et (her zaman aktif kalacak)
        weapons[0].gameObject.SetActive(true);
        
        // İkincil silahı aktif et
        EquipSecondaryWeapon(currentSecondaryWeaponIndex);
        
        // Initialize weapon upgrades
        if (BlacksmithManager.Instance != null && playerStats != null)
        {
            BlacksmithManager.Instance.ApplyWeaponUpgrades(playerStats);
        }
    }

    void Update()
    {
                if (Input.GetKeyDown(weaponSwitchKey) && weapons.Length > 2)
        {
            // Boomerang havadayken silah değişimine izin verme
            if (player != null && player.isBoomerangInAir)
            {
                return;
            }
            
            // Sadece ikincil silahlar arasında geçiş yap (1. indeksten başlayarak)
            currentSecondaryWeaponIndex++;
            if (currentSecondaryWeaponIndex >= weapons.Length)
            {
                currentSecondaryWeaponIndex = 1; // 1'e dön (0 kılıç olduğu için)
            }
            EquipSecondaryWeapon(currentSecondaryWeaponIndex);
        }
    }

    void EquipSecondaryWeapon(int index)
    {
        // Kılıcı etkileme (0. index)
        for (int i = 1; i < weapons.Length; i++)
        {
            // Seçilen silahı aktif et, diğerlerini deaktif et
            weapons[i].gameObject.SetActive(i == index);
        }
        
        // Player üzerindeki lastActiveWeaponState'i güncelle
        if (player != null)
        {
            // Eğer aktif edilen silah boomerang ise
            if (weapons[index] is BoomerangWeaponStateMachine)
            {
                player.UpdateLastActiveWeapon(WeaponState.ThrowBoomerang);
            }
            // Eğer aktif edilen silah spellbook ise
            else if (weapons[index] is SpellbookWeaponStateMachine)
            {
                player.UpdateLastActiveWeapon(WeaponState.Spell1);
            }
        }
        
        //Debug.Log($"Equipped secondary weapon: {weapons[index].name}");
    }
    
    // Method to restore weapon visibility - called after HideWeapons/ShowWeapons
    public void RefreshWeaponVisibility()
    {
        
        
        // Refresh the secondary weapons visibility
        EquipSecondaryWeapon(currentSecondaryWeaponIndex);
    }
    
    // Get the current active weapons for blacksmith system
    public WeaponType[] GetEquippedWeaponTypes()
    {
        WeaponType[] types = new WeaponType[weapons.Length];
        
        // Determine weapon types
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] is SwordWeaponStateMachine)
            {
                types[i] = WeaponType.Sword;
            }
            else if (weapons[i] is BoomerangWeaponStateMachine)
            {
                types[i] = WeaponType.Boomerang;
            }
            else if (weapons[i] is SpellbookWeaponStateMachine)
            {
                types[i] = WeaponType.Spellbook;
            }
        }
        
        return types;
    }
}