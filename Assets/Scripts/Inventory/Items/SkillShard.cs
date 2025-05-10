using UnityEngine;

[CreateAssetMenu(fileName = "New SkillShard", menuName = "Data/SkillShard")]
public class SkillShard : ItemData
{
    [Header("Skill Shard Properties")]
    [SerializeField] private int shardValue = 1; // Bir shard kaç puan değerinde

    public int GetShardValue()
    {
        return shardValue;
    }
} 