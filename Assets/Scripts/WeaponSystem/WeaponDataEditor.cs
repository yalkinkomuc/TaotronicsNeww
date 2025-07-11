#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class WeaponDataEditor
{
    [MenuItem("Tools/Weapons/Reset All Weapon Levels")]
    public static void ResetAllWeaponLevels()
    {
        if (EditorUtility.DisplayDialog("Reset Weapon Levels", 
            "Bu işlem tüm silah seviyelerini sıfırlayacak. Emin misiniz?", 
            "Evet, Sıfırla", "İptal"))
        {
            int resetCount = 0;
            
            // BlacksmithManager'ı bul
            BlacksmithManager blacksmith = Object.FindFirstObjectByType<BlacksmithManager>();
            if (blacksmith != null)
            {
                var weapons = blacksmith.GetAllWeapons();
                foreach (var weapon in weapons)
                {
                    if (weapon != null)
                    {
                        weapon.level = 1;
                        EditorUtility.SetDirty(weapon); // ScriptableObject'i dirty olarak işaretle
                        resetCount++;
                        
                        // PlayerPrefs'i de temizle
                        PlayerPrefs.DeleteKey($"Weapon_{weapon.itemName}_Level");
                    }
                }
                
                PlayerPrefs.Save();
                AssetDatabase.SaveAssets(); // Değişiklikleri kaydet
                
                Debug.Log($"✅ {resetCount} silahın seviyesi sıfırlandı!");
                EditorUtility.DisplayDialog("Başarılı", 
                    $"{resetCount} silahın seviyesi başarıyla sıfırlandı!", "Tamam");
            }
            else
            {
                Debug.LogWarning("❌ BlacksmithManager bulunamadı!");
                EditorUtility.DisplayDialog("Hata", 
                    "BlacksmithManager bulunamadı! Sahnede BlacksmithManager olduğundan emin olun.", "Tamam");
            }
        }
    }
    
    [MenuItem("Tools/Weapons/Find All WeaponData Assets")]
    public static void FindAllWeaponDataAssets()
    {
        // Proje içindeki tüm WeaponData asset'lerini bul ve resetle
        string[] guids = AssetDatabase.FindAssets("t:WeaponData");
        int resetCount = 0;
        
        if (guids.Length == 0)
        {
            Debug.LogWarning("❌ Hiç WeaponData asset'i bulunamadı!");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Reset All WeaponData Assets", 
            $"{guids.Length} WeaponData asset'i bulundu. Hepsinin seviyesini sıfırlamak istiyor musunuz?", 
            "Evet, Sıfırla", "İptal"))
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                
                if (weapon != null)
                {
                    weapon.level = 1;
                    EditorUtility.SetDirty(weapon);
                    resetCount++;
                    
                    // PlayerPrefs'i de temizle
                    PlayerPrefs.DeleteKey($"Weapon_{weapon.itemName}_Level");
                    
                    Debug.Log($"Reset: {weapon.itemName} (Level: {weapon.level})");
                }
            }
            
            PlayerPrefs.Save();
            AssetDatabase.SaveAssets();
            
            Debug.Log($"✅ {resetCount} WeaponData asset'inin seviyesi sıfırlandı!");
            EditorUtility.DisplayDialog("Başarılı", 
                $"{resetCount} WeaponData asset'inin seviyesi başarıyla sıfırlandı!", "Tamam");
        }
    }
}
#endif 