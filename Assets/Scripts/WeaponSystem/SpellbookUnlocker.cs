using System.Linq;
using UnityEngine;

/// <summary>
/// Quest tamamlandığında Spellbook silahını otomatik olarak oyuncuya ekleyip isteğe bağlı olarak kuşanır.
/// "RunCustomAction" completion davranışı ile tetiklenmesi amaçlanmıştır.
/// </summary>
public class SpellbookUnlocker : MonoBehaviour
{
    [Tooltip("Resources klasöründeki Spellbook prefab yolunu belirtin (Resources.Load kullanır).")]
    [SerializeField] private string spellbookPrefabPath = "Weapons/Spellbook";

    /// <summary>
    /// QuestGiver → CompletionBehaviour → RunCustomAction yoluyla çağrılır.
    /// </summary>
    public void UnlockSpellbook()
    {
        // 1) Player ve temel referansları bul
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("[SpellbookUnlocker] Player bulunamadı!");
            return;
        }

        // PlayerWeaponManager aynı GameObject'te olmayabilir; sahnede arayalım
        PlayerWeaponManager weaponManager = player.GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError("[SpellbookUnlocker] PlayerWeaponManager bulunamadı!");
            return;
        }

        // Spellbook zaten eklenmiş mi kontrol et
        int existingIdx = System.Array.FindIndex(weaponManager.weapons, w => w is SpellbookWeaponStateMachine);
        if (existingIdx >= 0)
        {
            // Zaten var; sadece kuşan
            weaponManager.EquipSecondaryWeapon(existingIdx);
            Debug.Log("[SpellbookUnlocker] Spellbook zaten mevcut, kuşanıldı.");
            return;
        }

        // 2) Prefab'ı Resources'tan yükle
        GameObject prefab = Resources.Load<GameObject>(spellbookPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[SpellbookUnlocker] Resources.Load başarısız! Yol: {spellbookPrefabPath}");
            return;
        }

        // 3) Instantiate ve Player'ın child'ı yap
        GameObject spellbookGO = Instantiate(prefab, player.transform);
        spellbookGO.name = "Spellbook"; // sahnede temiz isim

        SpellbookWeaponStateMachine spellbookSM = spellbookGO.GetComponent<SpellbookWeaponStateMachine>();
        if (spellbookSM == null)
        {
            Debug.LogError("[SpellbookUnlocker] Prefab SpellbookWeaponStateMachine içermiyor!");
            Destroy(spellbookGO);
            return;
        }

        // 4) weapons dizisini genişlet
        var weaponList = weaponManager.weapons.ToList();
        weaponList.Add(spellbookSM);
        weaponManager.weapons = weaponList.ToArray();

        // Yeni indeks
        int newIndex = weaponManager.weapons.Length - 1;

        // 5) Kuşan (Secondary Weapon) ve UI event'lerini tetikle
        weaponManager.EquipSecondaryWeapon(newIndex);

        // 6) EquipmentManager'a bildir (stats güncellensin)
        if (EquipmentManager.Instance != null && BlacksmithManager.Instance != null)
        {
            var spellbookData = BlacksmithManager.Instance.GetAllWeapons()
                                .Find(w => w.weaponType == WeaponType.Spellbook);
            if (spellbookData != null)
            {
                // Spellbook asset'inde slot yanlış tanımlı ise düzelt
                spellbookData.equipmentSlot = EquipmentSlot.SecondaryWeapon;
                EquipmentManager.Instance.EquipItem(spellbookData);
            }
        }

        Debug.Log("[SpellbookUnlocker] Spellbook başarıyla eklendi ve kuşanıldı.");
    }
} 