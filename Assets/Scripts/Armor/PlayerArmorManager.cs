using UnityEngine;

public class PlayerArmorManager : MonoBehaviour
{
    public ArmorStateMachine[] armors;
    public ArmorData[] armorDatas; // Inspector'dan sırayla atayacaksın
    [SerializeField] public int startingArmorIndex = 0;
    private int currentArmorIndex = -1;

    void Start()
    {
        if (armors == null || armors.Length == 0)
            return;

        // Tüm armorları devre dışı bırak
        for (int i = 0; i < armors.Length; i++)
        {
            if (armors[i] != null)
                armors[i].gameObject.SetActive(false);
        }

        // Başlangıç armorunu aktif et
        ActivateArmor(startingArmorIndex);
    }

    public void ActivateArmor(int index)
    {
        if (index < 0 || index >= armors.Length || armors[index] == null)
        {
            Debug.LogError($"Invalid armor index: {index}");
            return;
        }

        // Tüm armorları devre dışı bırak
        for (int i = 0; i < armors.Length; i++)
        {
            if (armors[i] != null)
                armors[i].gameObject.SetActive(false);
        }

        // Seçilen armor'u aktif et
        armors[index].gameObject.SetActive(true);
        currentArmorIndex = index;
        Debug.Log($"Activated armor: {armors[index].name}");

        // ArmorData'yı EquipmentManager'a giydir
        if (armorDatas != null && index < armorDatas.Length && armorDatas[index] != null)
        {
            EquipmentManager.Instance.EquipItem(armorDatas[index]);
        }
        else
        {
            Debug.LogWarning($"ArmorData eşleşmedi! Index: {index}");
        }
    }

    public int GetCurrentArmorIndex() => currentArmorIndex;
} 