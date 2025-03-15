using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public WeaponStateMachine[] weapons; 
    //private int currentWeaponIndex = 0;

    void Start()
    {
        EquipWeapon(0); 
        EquipWeapon(1); // ikisini birden takmak istediğimizde bunu çağırcaz.
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Q)) 
        // {
        //     currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        //     EquipWeapon(currentWeaponIndex);
        // }
    }

    void EquipWeapon(int index)
    {
        if (weapons.Length == 0 || index >= weapons.Length)
        {
            Debug.LogError("Geçersiz silah indeksi");
            return;
        }
        
        // Sadece seçilen silahı aktif et, diğerlerine dokunma
        weapons[index].gameObject.SetActive(true);
    }
}