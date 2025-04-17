using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int weight = 1;
    public int minLevel = 1;
    public int maxLevel = 10;
}

[System.Serializable]
public class AreaSpawnSettings
{
    public string areaName;
    public int areaLevel = 1;
    public float difficulty = 1.0f;
    public List<EnemySpawnInfo> possibleEnemies = new List<EnemySpawnInfo>();
}

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }
    
    [SerializeField] private List<AreaSpawnSettings> areas = new List<AreaSpawnSettings>();
    [SerializeField] private string currentArea = ""; // Current area name

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetCurrentArea(string areaName)
    {
        currentArea = areaName;
    }
    
    public GameObject SpawnEnemy(Transform spawnPoint)
    {
        // Find the current area settings
        AreaSpawnSettings areaSettings = areas.Find(a => a.areaName == currentArea);
        if (areaSettings == null || areaSettings.possibleEnemies.Count == 0)
        {
            Debug.LogWarning("No spawn settings found for area: " + currentArea);
            return null;
        }
        
        // Select a random enemy type based on weights
        EnemySpawnInfo selectedEnemy = GetRandomEnemy(areaSettings.possibleEnemies);
        if (selectedEnemy == null)
            return null;
            
        // Spawn the enemy
        GameObject enemyObject = Instantiate(selectedEnemy.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Set enemy level based on area and player level
        EnemyStats enemyStats = enemyObject.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            int enemyLevel = GetEnemyLevel(areaSettings, selectedEnemy);
            enemyStats.SetLevel(enemyLevel);
            
            // Optionally add a visual level indicator
            AddLevelIndicator(enemyObject, enemyLevel);
        }
        
        return enemyObject;
    }
    
    private int GetEnemyLevel(AreaSpawnSettings areaSettings, EnemySpawnInfo enemyInfo)
    {
        // If we have a LevelManager, use its scaling system
        if (LevelManager.Instance != null)
        {
            int baseLevel = LevelManager.Instance.GetEnemyLevelForArea(areaSettings.areaLevel, areaSettings.difficulty);
            // Clamp to the enemy's allowed level range
            return Mathf.Clamp(baseLevel, enemyInfo.minLevel, enemyInfo.maxLevel);
        }
        
        // Fallback if no LevelManager
        Player player = FindObjectOfType<Player>();
        int playerLevel = player != null && player.GetComponent<PlayerStats>() != null 
            ? player.GetComponent<PlayerStats>().GetLevel() 
            : 1;
            
        // Simple formula: area level + random variance clamped to enemy's allowed range
        int level = areaSettings.areaLevel + Random.Range(-1, 2);
        return Mathf.Clamp(level, enemyInfo.minLevel, enemyInfo.maxLevel);
    }
    
    private EnemySpawnInfo GetRandomEnemy(List<EnemySpawnInfo> enemies)
    {
        int totalWeight = 0;
        foreach (var enemy in enemies)
        {
            totalWeight += enemy.weight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int weightSum = 0;
        
        foreach (var enemy in enemies)
        {
            weightSum += enemy.weight;
            if (randomValue < weightSum)
                return enemy;
        }
        
        return enemies[0]; // Fallback
    }
    
    private void AddLevelIndicator(GameObject enemyObject, int level)
    {
        // You could add a floating text or icon above the enemy showing its level
        // For example, create a TextMesh with the level number
        
        GameObject levelIndicator = new GameObject("Level Indicator");
        levelIndicator.transform.SetParent(enemyObject.transform);
        levelIndicator.transform.localPosition = new Vector3(0, 1.5f, 0); // Position above enemy
        
        TextMesh textMesh = levelIndicator.AddComponent<TextMesh>();
        textMesh.text = "Lvl " + level;
        textMesh.fontSize = 10;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = GetLevelColor(level);
    }
    
    private Color GetLevelColor(int level)
    {
        // Return different colors based on level ranges
        if (level >= 20)
            return Color.red; // High level - danger
        else if (level >= 10)
            return Color.yellow; // Mid level - caution
        else
            return Color.green; // Low level - easy
    }
} 