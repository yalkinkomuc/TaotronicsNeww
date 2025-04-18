using UnityEngine;

public class EnemyStats : CharacterStats
{
    private Enemy enemy;
    private ItemDrop myDropSystem;
    
    [Header("Experience Reward")]
    [SerializeField] private int baseExperienceReward = 10;
    [SerializeField] private float experienceLevelMultiplier = 0.5f; // 50% more XP per level
    
    [Header("Level Scaling")]
    [SerializeField] private bool scaleWithPlayerLevel = false;
    [SerializeField] private int levelOffset = 0; // Can be negative or positive

    protected override void Start()
    {
        base.Start();

        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrop>();
        
        // Apply level scaling based on player level if enabled
        if (scaleWithPlayerLevel)
        {
            ScaleWithPlayerLevel();
        }
    }
    
    private void ScaleWithPlayerLevel()
    {
        // Find player and get their level
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                int playerLevel = playerStats.GetLevel();
                int newLevel = Mathf.Max(1, playerLevel + levelOffset);
                
                // Set the enemy level
                SetLevel(newLevel);
            }
        }
    }

    /// <summary>
    /// Calculate experience reward based on enemy level
    /// </summary>
    public int GetExperienceReward()
    {
        // Base XP + additional XP per level
        int experienceReward = baseExperienceReward;
        if (level > 1)
        {
            experienceReward += Mathf.RoundToInt(baseExperienceReward * (level - 1) * experienceLevelMultiplier);
        }
        
        return experienceReward;
    }

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
    }

    public override void Die()
    {
        base.Die();
        
        // Reward player with experience
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddExperience(GetExperienceReward());
            }
        }
        
        enemy.Die();

        if (myDropSystem != null)
        {
            myDropSystem.GenerateDrop();
        }
    }
}

