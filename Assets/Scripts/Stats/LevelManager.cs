using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    [Header("Player Leveling")]
    [SerializeField] private int maxPlayerLevel = 50;
    [SerializeField] private AnimationCurve experienceCurve;
    
    [Header("Enemy Scaling")]
    [SerializeField] private bool autoLevelEnemies = true;
    [SerializeField] private int minEnemyLevel = 1;
    [SerializeField] private int maxEnemyLevel = 50;
    [SerializeField] private float enemyLevelVariance = 2f; // Random variance in enemy levels
    
    // Cache calculated experience requirements
    private Dictionary<int, int> experienceRequirements = new Dictionary<int, int>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            
            // Pre-calculate experience requirements
            CalculateExperienceRequirements();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void CalculateExperienceRequirements()
    {
        // Base XP for level 1 to 2
        experienceRequirements[1] = 100;
        
        for (int level = 2; level < maxPlayerLevel; level++)
        {
            // If we have an experience curve, use it, otherwise use formula
            if (experienceCurve.length > 0)
            {
                // Normalize level to 0-1 range for curve
                float normalizedLevel = (float)(level - 1) / (maxPlayerLevel - 1);
                float curveValue = experienceCurve.Evaluate(normalizedLevel);
                
                // Scale curve value (0-1) to a reasonable XP range (100-10000)
                int requiredXP = Mathf.RoundToInt(100 + curveValue * 9900);
                experienceRequirements[level] = requiredXP;
            }
            else
            {
                // Exponential formula: each level requires 50% more XP than the previous
                experienceRequirements[level] = Mathf.RoundToInt(experienceRequirements[level - 1] * 1.5f);
            }
        }
    }
    
    public int GetExperienceForNextLevel(int currentLevel)
    {
        if (experienceRequirements.TryGetValue(currentLevel, out int xpRequired))
        {
            return xpRequired;
        }
        
        // Fallback formula if level not in dictionary
        return Mathf.RoundToInt(100 * Mathf.Pow(1.5f, currentLevel - 1));
    }
    
    public int GetEnemyLevelForArea(int areaLevel, float difficulty = 1.0f)
    {
        if (!autoLevelEnemies)
            return areaLevel;
            
        // Find player
        Player player = FindObjectOfType<Player>();
        if (player == null)
            return areaLevel;
            
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
            return areaLevel;
            
        // Base enemy level on player level, area level, and difficulty
        int playerLevel = playerStats.GetLevel();
        int baseEnemyLevel = Mathf.RoundToInt((playerLevel + areaLevel) / 2f * difficulty);
        
        // Apply random variance if enabled
        if (enemyLevelVariance > 0)
        {
            float variance = Random.Range(-enemyLevelVariance, enemyLevelVariance);
            baseEnemyLevel += Mathf.RoundToInt(variance);
        }
        
        // Clamp to valid range
        return Mathf.Clamp(baseEnemyLevel, minEnemyLevel, maxEnemyLevel);
    }
} 