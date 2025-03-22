using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UI_ItemSlot : MonoBehaviour
{

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;

    public InventoryItem item;
    

    private void Awake()
    {
        // Başlangıçta image'i temizle
        ClearSlot();
    }

    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;

        if (item != null)
        {
            itemImage.sprite = item.data.icon;
            itemImage.color = Color.white;

            if (item.stackSize > 1)
            {
                itemText.text = item.stackSize.ToString();
            }
            else
            {
                itemText.text = "";
            }
        }
        else
        {
            ClearSlot();
        }
    }

    private void ClearSlot()
    {
        item = null;
        itemImage.sprite = null;
        itemImage.color = new Color(0, 0, 0, 0); // Tamamen şeffaf
        itemText.text = "";
    }
}
