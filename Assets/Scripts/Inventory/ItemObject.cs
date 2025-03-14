using System;
using UnityEngine;

public class ItemObject : MonoBehaviour,IInteractable
{
   
   
   [SerializeField] private ItemData itemData;
   [SerializeField] private InteractionPrompt prompt;

  


   private void OnValidate()
   {
      GetComponent<SpriteRenderer>().sprite = itemData.icon;
      gameObject.name = "Item Object -  " + itemData.itemName;
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
