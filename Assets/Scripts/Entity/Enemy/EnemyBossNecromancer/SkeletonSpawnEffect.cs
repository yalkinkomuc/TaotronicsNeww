using UnityEngine;

public class SkeletonSpawnEffect : MonoBehaviour
{
    private EnemyBossNecromancerBoss necromancer;
    private Vector2 spawnPosition;

    public void Initialize(EnemyBossNecromancerBoss _necromancer, Vector2 _spawnPosition)
    {
        necromancer = _necromancer;
        spawnPosition = _spawnPosition;
    }

    // Animation Event'ten çağrılacak
    private void OnSpawnAnimationComplete()
    {
        if (necromancer != null && necromancer.CanSummon())
        {
            necromancer.SummonSkeletonAtPosition(spawnPosition);
        }
        Destroy(gameObject);
    }
} 