using UnityEngine;

public class EnemyStats : CharacterStats
{
    private Enemy enemy;
    private ItemDrop myDropSystem;
    
    [Header("Experience Reward")]
    [SerializeField] private int baseExperienceReward = 10;
    [SerializeField] private float experienceLevelMultiplier = 0.5f; // 50% more XP per level
    
    [Header("Gold Reward")]
    [SerializeField] private int baseGoldReward = 5;
    [SerializeField] private float goldLevelMultiplier = 0.3f; // 30% more gold per level
    [SerializeField] private float goldRandomVariance = 0.2f; // +/- 20% random variance
    
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
    
    /// <summary>
    /// Calculate gold reward based on enemy level with some randomness
    /// </summary>
    public int GetGoldReward()
    {
        // Base gold + additional gold per level
        int baseGold = baseGoldReward;
        if (level > 1)
        {
            baseGold += Mathf.RoundToInt(baseGoldReward * (level - 1) * goldLevelMultiplier);
        }
        
        // Add random variance
        float variance = Random.Range(-goldRandomVariance, goldRandomVariance);
        int finalGold = Mathf.RoundToInt(baseGold * (1f + variance));
        
        // Ensure at least 1 gold
        return Mathf.Max(1, finalGold);
    }

    public override void Die()
    {
        base.Die();
        
        // Find player
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Reward player with experience
                int xpReward = GetExperienceReward();
                playerStats.AddExperience(xpReward);
                
                // Reward player with gold
                int goldReward = GetGoldReward();
                playerStats.AddGold(goldReward);
                
                // Log rewards
                Debug.Log($"Enemy defeated: +{xpReward} XP, +{goldReward} Gold");
            }
        }
        
        enemy.Die();

        // Generate item drops
        if (myDropSystem != null)
        {
            myDropSystem.GenerateDrop();
        }
    }
}

