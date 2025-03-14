using System;
using UnityEngine;

public class ItemObject : MonoBehaviour,IInteractable
{
   
   private SpriteRenderer spriteRenderer;
   [SerializeField] private ItemData itemData;
   [SerializeField] private InteractionPrompt prompt;

   private void Start()
   {
      spriteRenderer = GetComponent<SpriteRenderer>();
      spriteRenderer.sprite = itemData.icon;
   }


   public void Interact()
   {
      Inventory.instance.AddItem(itemData);
      Destroy(gameObject);
   }

   public void ShowInteractionPrompt()
   {
      if (prompt != null)
         prompt.ShowPrompt();
   }

   public void HideInteractionPrompt()
   {
      if (prompt != null)
         prompt.HidePrompt();
   }
}
