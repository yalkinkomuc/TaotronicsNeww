using UnityEngine;

[CreateAssetMenu(fileName = "New SkillShard", menuName = "Data/SkillShard")]
public class SkillShard : ItemData
{
    [Header("Skill Shard Properties")]
    [SerializeField] private int shardValue = 4; // Bir shard kaç puan değerinde

    public int GetShardValue()
    {
        if (itemName == "Small Shard")
        {
            shardValue = Random.Range(4, 12);
        }
        
        if (itemName == "Medium Shard")
        {
            shardValue = Random.Range(12, 25);
        }
        
        if (itemName == "Large Shard")
        {
            shardValue = Random.Range(25, 40);
        }
        
        if (itemName == "XLarge Shard")
        {
            shardValue = Random.Range(40, 75);
        }
        
        return shardValue;
    }
} 