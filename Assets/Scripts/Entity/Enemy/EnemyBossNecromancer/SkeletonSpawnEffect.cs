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
        if (necromancer != null)
        {
            // İskeleti tam olarak efektin olduğu konumda spawn et
            necromancer.SummonSkeletonAtPosition(spawnPosition);
        }
        Destroy(gameObject); // Efekt bitince yok ol
    }
} 