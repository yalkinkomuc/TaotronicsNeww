using UnityEngine;


[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
   public string itemName;
   public Sprite icon;

   [Range(0,100)]
   public float dropChance;

   // Item'ın Resources klasöründeki yolu - Bu genellikle "Items/itemName" şeklinde olmalı
   public string resourcePath;

   private void OnValidate()
   {
       // Editor'da resource path'i otomatik ayarla
       if (string.IsNullOrEmpty(resourcePath) && !string.IsNullOrEmpty(itemName))
       {
           resourcePath = $"Items/{itemName}";
           Debug.Log($"{name} için resourcePath ayarlandı: {resourcePath}");
       }
   }
}
