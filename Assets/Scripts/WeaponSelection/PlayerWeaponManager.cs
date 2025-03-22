using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public WeaponStateMachine[] weapons;
    private int currentSecondaryWeaponIndex = 1; // 1'den başlıyoruz çünkü 0 kılıç
    
    [SerializeField] private KeyCode weaponSwitchKey = KeyCode.Q; // Silah değiştirme tuşu

    void Start()
    {
        if (weapons.Length < 2)
        {
            Debug.LogError("En az 2 silah (kılıç ve ikincil silah) atanmalı!");
            return;
        }

        // Kılıcı aktif et (her zaman aktif kalacak)
        weapons[0].gameObject.SetActive(true);
        
        // İkincil silahı aktif et
        EquipSecondaryWeapon(currentSecondaryWeaponIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(weaponSwitchKey) && weapons.Length > 2)
        {
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
        
        Debug.Log($"Equipped secondary weapon: {weapons[index].name}");
    }
}