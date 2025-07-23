using System.Linq;
using UnityEngine;


public class WeaponUnlocker : MonoBehaviour
{
    [ContextMenu("UnlockSpellbook")]
    public void UnlockSpellbook()
    {
        UnlockWeapon<SpellbookWeaponStateMachine>("Spellbook", SecondaryWeaponType.Spellbook, true);
    }

    [ContextMenu("UnlockBoomerang")]
    public void UnlockBoomerang()
    {
        UnlockWeapon<BoomerangWeaponStateMachine>("Boomerang", SecondaryWeaponType.Boomerang, true);
    }

    [ContextMenu("UnlockShield")]
    public void UnlockShield()
    {
        UnlockWeapon<ShieldStateMachine>("Shield", SecondaryWeaponType.Shield, true);
    }

    [ContextMenu("UnlockBurningSword")]
    public void UnlockBurningSword()
    {
        UnlockWeapon<BurningSwordStateMachine>("Burning Sword", null, false);
    }

    [ContextMenu("UnlockHammer")]
    public void UnlockHammer()
    {
        UnlockWeapon<HammerSwordStateMachine>("Hammer", null, false);
    }

    [ContextMenu("UnlockSword")]
    public void UnlockSword()
    {
        UnlockWeapon<SwordWeaponStateMachine>("Sword", null, false);
    }

   
    private void UnlockWeapon<T>(string weaponName, SecondaryWeaponType? secondaryType, bool isSecondary) where T : WeaponStateMachine
    {
        // 1) Player ve temel referansları bul
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError($"[WeaponUnlocker] Player bulunamadı! {weaponName} unlock edilemedi.");
            return;
        }

        // PlayerWeaponManager aynı GameObject'te olmayabilir; sahnede arayalım
        PlayerWeaponManager weaponManager = player.GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError($"[WeaponUnlocker] PlayerWeaponManager bulunamadı! {weaponName} unlock edilemedi.");
            return;
        }

        // 2) Sahneye manuel eklenen silahı bul
        T weaponStateMachine = null;
        int weaponIndex = -1;
        
        // Weapons dizisinde ilgili silahı ara
        for (int i = 0; i < weaponManager.weapons.Length; i++)
        {
            if (weaponManager.weapons[i] is T)
            {
                weaponStateMachine = weaponManager.weapons[i] as T;
                weaponIndex = i;
                break;
            }
        }

        if (weaponStateMachine == null)
        {
            Debug.LogError($"[WeaponUnlocker] Sahneye manuel eklenen {weaponName} silahı bulunamadı! Lütfen Player hierarchy'sinde {weaponName} GameObject'ini manuel olarak ekleyin ve PlayerWeaponManager'ın weapons dizisine atayın.");
            return;
        }

        // 3) Silahı unlock et
        weaponManager.UnlockWeapon(weaponIndex);
        
        // 4) Secondary weapon ise kuşan, primary weapon ise sadece unlock et
        if (isSecondary)
        {
            weaponManager.EquipSecondaryWeapon(weaponIndex);
        }

        // 5) EquipmentManager'a bildir (stats güncellensin)
        if (EquipmentManager.Instance != null && BlacksmithManager.Instance != null)
        {
            if (isSecondary && secondaryType.HasValue)
            {
                // Secondary weapon için SecondaryWeaponData'dan ara
                var secondaryWeaponData = BlacksmithManager.Instance.secondaryWeaponDataBase?.Find(s => s.secondaryWeaponType == secondaryType.Value);
                
                if (secondaryWeaponData != null)
                {
                    Debug.Log($"[WeaponUnlocker] {weaponName} SecondaryWeaponData bulundu: {secondaryWeaponData.itemName}");
                    bool equipResult = EquipmentManager.Instance.EquipItem(secondaryWeaponData);
                    Debug.Log($"[WeaponUnlocker] EquipmentManager.EquipItem sonucu: {equipResult}");
                    
                    if (!equipResult)
                    {
                        Debug.LogError($"[WeaponUnlocker] {weaponName} EquipItem başarısız oldu!");
                    }
                }
                else
                {
                    Debug.LogError($"[WeaponUnlocker] BlacksmithManager.secondaryWeaponDataBase'de {weaponName} bulunamadı!");
                    LogSecondaryWeaponDatabase();
                }
            }
            else
            {
                // Primary weapon için WeaponData'dan ara
                var allWeapons = BlacksmithManager.Instance.GetAllWeapons();
                if (allWeapons != null)
                {
                    WeaponType targetWeaponType = GetWeaponTypeFromStateMachine(weaponStateMachine);
                    var weaponData = allWeapons.Find(w => w.weaponType == targetWeaponType);
                    
                    if (weaponData != null)
                    {
                        Debug.Log($"[WeaponUnlocker] {weaponName} WeaponData bulundu: {weaponData.itemName}");
                        bool equipResult = EquipmentManager.Instance.EquipItem(weaponData);
                        Debug.Log($"[WeaponUnlocker] EquipmentManager.EquipItem sonucu: {equipResult}");
                        
                        if (!equipResult)
                        {
                            Debug.LogError($"[WeaponUnlocker] {weaponName} EquipItem başarısız oldu!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[WeaponUnlocker] BlacksmithManager.weaponDatabase'de {weaponName} bulunamadı!");
                    }
                }
            }
        }

        // 6) UI Equipment Selection Panel'i güncelle (eğer açıksa)
        RefreshEquipmentSelectionPanel(isSecondary);

        Debug.Log($"[WeaponUnlocker] {weaponName} başarıyla unlock edildi ve aktif hale getirildi.");
    }

    /// <summary>
    /// Quest tamamlandıktan sonra Equipment Selection Panel'inin güncellenmesini sağlar
    /// </summary>
    private void RefreshEquipmentSelectionPanel(bool isSecondaryWeapon)
    {
        // UI_EquipmentSelectionPanel instance'ını kontrol et ve refresh et
        if (UI_EquipmentSelectionPanel.Instance != null)
        {
            Debug.Log("[WeaponUnlocker] Equipment Selection Panel güncelleniyor...");
            UI_EquipmentSelectionPanel.Instance.RefreshCurrentPanel();
        }
    }

    /// <summary>
    /// WeaponStateMachine'den WeaponType'ı belirle
    /// </summary>
    private WeaponType GetWeaponTypeFromStateMachine(WeaponStateMachine stateMachine)
    {
        if (stateMachine == null) return WeaponType.Sword; // Default fallback
        
        // Determine weapon type based on state machine type
        if (stateMachine is SwordWeaponStateMachine)
        {
            return WeaponType.Sword;
        }
        else if (stateMachine is BurningSwordStateMachine)
        {
            return WeaponType.BurningSword;
        }
        else if (stateMachine is HammerSwordStateMachine)
        {
            return WeaponType.Hammer;
        }
        else if (stateMachine is BoomerangWeaponStateMachine)
        {
            return WeaponType.Boomerang;
        }
        else if (stateMachine is SpellbookWeaponStateMachine)
        {
            return WeaponType.Spellbook;
        }
        else if (stateMachine is ShieldStateMachine)
        {
            return WeaponType.Shield;
        }
        
        return WeaponType.Sword; // Default fallback
    }

    /// <summary>
    /// Debug: SecondaryWeaponDataBase'deki tüm itemları listele
    /// </summary>
    private void LogSecondaryWeaponDatabase()
    {
        if (BlacksmithManager.Instance.secondaryWeaponDataBase != null)
        {
            Debug.Log($"[WeaponUnlocker] SecondaryWeaponDataBase'de toplam {BlacksmithManager.Instance.secondaryWeaponDataBase.Count} item var:");
            foreach (var item in BlacksmithManager.Instance.secondaryWeaponDataBase)
            {
                Debug.Log($"  - {item.itemName} (Tip: {item.secondaryWeaponType})");
            }
        }
        else
        {
            Debug.LogError("[WeaponUnlocker] BlacksmithManager.secondaryWeaponDataBase null!");
        }
    }
} 