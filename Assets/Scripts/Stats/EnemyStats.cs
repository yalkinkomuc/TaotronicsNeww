using UnityEngine;

public class EnemyStats : CharacterStats
{
    private Enemy enemy;
    private ItemDrop myDropSystem;
    
    // Override base class properties with concrete implementations
    [SerializeField] private int _vitality = 0;
    [SerializeField] private int _might = 0;
    [SerializeField] private int _defense = 0;
    [SerializeField] private int _luck = 0;
    [SerializeField] private int _maxAttributeLevel = 99;
    [SerializeField] private int _mind = 0;
    
    [SerializeField] private float _baseHealthValue = 100f;
    [SerializeField] private float _baseDamageValue = 10f;
    [SerializeField] private float _baseManaValue = 50f;
    [SerializeField] private float _baseDefenseValue = 5f;
    [SerializeField] private float _baseSpeedValue = 300f;
    
    // Override properties from base class
    protected override int vitality { get => _vitality; set => _vitality = value; }
    protected override int might { get => _might; set => _might = value; }
    protected override int defense { get => _defense; set => _defense = value; }
    protected override int luck { get => _luck; set => _luck = value; }
    protected override int maxAttributeLevel { get => _maxAttributeLevel; set => _maxAttributeLevel = value; }
    protected override int mind { get => _mind; set => _mind = value; }
    
    protected override float baseHealthValue { get => _baseHealthValue; set => _baseHealthValue = value; }
    protected override float baseDamageValue { get => _baseDamageValue; set => _baseDamageValue = value; }
    protected override float baseManaValue { get => _baseManaValue; set => _baseManaValue = value; }
    protected override float baseDefenseValue { get => _baseDefenseValue; set => _baseDefenseValue = value; }
    protected override float baseSpeedValue { get => _baseSpeedValue; set => _baseSpeedValue = value; }
    
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
    
    [Header("Attribute Distribution")]
    [SerializeField] private EnemyAttributeFocus attributeFocus = EnemyAttributeFocus.Balanced;
    [SerializeField] private float attributePointsPerLevel = 1.5f; // How many attribute points to distribute per level

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrop>();
        
        // Apply level scaling based on player level if enabled
        if (scaleWithPlayerLevel)
        {
            ScaleWithPlayerLevel();
        }

        // Distribute attributes based on enemy type and level
        DistributeAttributePoints();
        
        // Call base Start after setting attributes
        base.Start();
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
    
    // Distribute attribute points based on enemy type and level
    private void DistributeAttributePoints()
    {
        if (level <= 1) return;
        
        // Calculate total attribute points to distribute
        int totalAttributePoints = Mathf.FloorToInt((level - 1) * attributePointsPerLevel);
        
        // Distribute based on enemy type
        switch (attributeFocus)
        {
            case EnemyAttributeFocus.Balanced:
                DistributeBalanced(totalAttributePoints);
                break;
            case EnemyAttributeFocus.Tank:
                DistributeTank(totalAttributePoints);
                break;
            case EnemyAttributeFocus.Damage:
                DistributeDamage(totalAttributePoints);
                break;
            case EnemyAttributeFocus.Speed:
                DistributeSpeed(totalAttributePoints);
                break;
            case EnemyAttributeFocus.Magic:
                DistributeMagic(totalAttributePoints);
                break;
        }
    }
    
    // Different attribute distribution strategies
    private void DistributeBalanced(int points)
    {
        // Distribute evenly across all attributes
        int pointsPerAttribute = points / 5;
        int remainder = points % 5;
        _vitality += pointsPerAttribute;
        _might += pointsPerAttribute;
        _defense += pointsPerAttribute;
        _luck += pointsPerAttribute;
        _mind += pointsPerAttribute;
        // Distribute remainder
        for (int i = 0; i < remainder; i++)
        {
            switch (i % 5)
            {
                case 0: _vitality++; break;
                case 1: _might++; break;
                case 2: _defense++; break;
                case 3: _luck++; break;
                case 4: _mind++; break;
            }
        }
    }
    
    private void DistributeTank(int points)
    {
        int vitalityPoints = Mathf.FloorToInt(points * 0.4f);
        int defensePoints = Mathf.FloorToInt(points * 0.4f);
        int otherPoints = points - vitalityPoints - defensePoints;
        _vitality += vitalityPoints;
        _defense += defensePoints;
        // Distribute remaining points
        int perStat = otherPoints / 3;
        _might += perStat;
        _luck += perStat;
        _mind += otherPoints - 2 * perStat;
    }
    
    private void DistributeDamage(int points)
    {
        int mightPoints = Mathf.FloorToInt(points * 0.5f);
        int luckPoints = Mathf.FloorToInt(points * 0.3f);
        int otherPoints = points - mightPoints - luckPoints;
        _might += mightPoints;
        _luck += luckPoints;
        // Distribute remaining points
        int perStat = otherPoints / 3;
        _vitality += perStat;
        _defense += perStat;
        _mind += otherPoints - 2 * perStat;
    }
    
    private void DistributeSpeed(int points)
    {
        // No agility, just distribute all points evenly
        int perStat = points / 4;
        int remainder = points % 4;
        _vitality += perStat;
        _might += perStat;
        _defense += perStat;
        _luck += perStat;
        for (int i = 0; i < remainder; i++)
        {
            switch (i % 4)
            {
                case 0: _vitality++; break;
                case 1: _might++; break;
                case 2: _defense++; break;
                case 3: _luck++; break;
            }
        }
    }
    
    private void DistributeMagic(int points)
    {
        // No agility, focus on mind and might
        int mindPoints = Mathf.FloorToInt(points * 0.5f);
        int mightPoints = Mathf.FloorToInt(points * 0.3f);
        int otherPoints = points - mindPoints - mightPoints;
        _mind += mindPoints;
        _might += mightPoints;
        // Distribute remaining points
        int perStat = otherPoints / 3;
        _vitality += perStat;
        _defense += perStat;
        _luck += otherPoints - 2 * perStat;
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

// Enum to define enemy attribute focus
public enum EnemyAttributeFocus
{
    Balanced,   // Even distribution
    Tank,       // High vitality and defense
    Damage,     // High might and luck
    Speed,      // (Unused)
    Magic       // (Unused)
}

