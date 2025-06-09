using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TalkToNPCObjective", menuName = "Quest/Objective/TalkToNPC")]
public class TalkToNPCObjective : QuestObjective
{
    [Header("NPC Settings")]
    public string npcID;
    
    [Space(10)]
    [Header("Item Rewards")]
    [Tooltip("Objective tamamlandığında verilecek eşyalar")]
    public List<ItemReward> itemRewards = new List<ItemReward>();
    
    [System.Serializable]
    public class ItemReward
    {
        [Tooltip("Verilecek eşya")]
        public ItemData item;
        [Tooltip("Kaç adet verilecek")]
        public int quantity = 1;
    }

    public override void Initialize()
    {
        isCompleted = false;
        isInitialized = true;
    }

    public override void HandleEvent(string eventName, object data)
    {
        if (eventName == "TalkedToNPC" && data is string talkedID && talkedID == npcID && !isCompleted)
        {
            isCompleted = true;
            
            // Objective tamamlandığında eşyaları ver
            GiveItemRewards();
            
            Debug.Log($"✅ Talked to NPC: {npcID} - Objective completed!");
        }
    }
    
    private void GiveItemRewards()
    {
        if (itemRewards == null || itemRewards.Count == 0) return;
        
        foreach (var reward in itemRewards)
        {
            if (reward.item == null) continue;
            
            // Her quantity için ayrı ayrı ekle
            for (int i = 0; i < reward.quantity; i++)
            {
                Inventory.instance?.AddItem(reward.item);
            }
            
            Debug.Log($"🎁 Rewarded: {reward.quantity}x {reward.item.itemName}");
        }
        
        Debug.Log($"🎉 Total {itemRewards.Count} item types given as rewards!");
    }
}