using UnityEngine;


[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
   public string itemName;
   public Sprite icon;
}
