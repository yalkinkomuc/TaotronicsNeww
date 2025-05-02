using System;
using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    [SerializeField] private List<ItemData> chestItems = new List<ItemData>();
    [SerializeField] private Animator animator;
    [SerializeField] private InteractionPrompt prompt;
    
    private bool isOpen = false;
    private bool isInteracting = false;

    private void Awake()
    {
        // Animator'ı al
        if (animator == null)
            animator = GetComponent<Animator>();
            
        // Interaction prompt'u al
        if (prompt == null)
            prompt = GetComponentInChildren<InteractionPrompt>();
    }

    public void Interact()
    {
        if (!isOpen)
        {
            OpenChest();
        }
        else if (!isInteracting)
        {
            ToggleChestInventory();
        }
    }

    private void OpenChest()
    {
        isOpen = true;
        animator?.SetTrigger("Open");
        
        // Sandık açıldığında inventory UI'ı göster
        if (UI_ChestInventory.Instance != null)
        {
            UI_ChestInventory.Instance.OpenChestInventory(this, chestItems);
            isInteracting = true;
        }
    }

    private void ToggleChestInventory()
    {
        if (UI_ChestInventory.Instance != null)
        {
            isInteracting = !isInteracting;
            if (isInteracting)
            {
                UI_ChestInventory.Instance.OpenChestInventory(this, chestItems);
            }
            else
            {
                UI_ChestInventory.Instance.CloseInventory();
            }
        }
    }

    public void ShowInteractionPrompt()
    {
        // Sadece sandık kapalıysa prompt göster
        if (!isOpen)
        {
            prompt?.ShowPrompt();
        }
    }

    public void HideInteractionPrompt()
    {
        prompt?.HidePrompt();
    }

    // Tüm itemleri envantere ekle
    public void TakeAllItems()
    {
        if (chestItems.Count == 0) return;

        foreach (var item in chestItems.ToArray())
        {
            Inventory.instance.AddItem(item);
            chestItems.Remove(item);
        }

        // Tüm itemler alındıysa UI'ı kapat
        if (chestItems.Count == 0)
        {
            CloseInventory();
        }
        else
        {
            UI_ChestInventory.Instance?.UpdateUI();
        }
    }

    // Tek bir itemi envantere ekle
    public void TakeItem(ItemData item)
    {
        if (chestItems.Contains(item))
        {
            Inventory.instance.AddItem(item);
            chestItems.Remove(item);

            // Tüm itemler alındıysa UI'ı kapat
            if (chestItems.Count == 0)
            {
                CloseInventory();
            }
            else
            {
                UI_ChestInventory.Instance?.UpdateUI();
            }
        }
    }

    private void CloseInventory()
    {
        isInteracting = false;
        if (UI_ChestInventory.Instance != null)
        {
            UI_ChestInventory.Instance.CloseInventory();
        }
    }
}
