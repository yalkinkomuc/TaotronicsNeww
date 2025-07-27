using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireSpell : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 5f; // Increased damage per second
    [SerializeField] private float damageRampUpTime = 2f; // Time it takes to reach full damage
    [SerializeField] private float maxDamageMultiplier = 1.5f; // Maximum damage multiplier after ramp-up
    [SerializeField] private float damageInterval = 0.5f; // How often to deal damage (in seconds)
    
    [Header("Visual Settings")]
    [SerializeField] private float textDisplayInterval = 0.1f; // How often to show damage text
    
    private BoxCollider2D boxCollider2D;
    private List<Enemy> burningEnemies = new List<Enemy>();
    private Dictionary<Enemy, float> lastTextTimes = new Dictionary<Enemy, float>();
    private Dictionary<Enemy, float> enemyBurnTimes = new Dictionary<Enemy, float>(); // Track how long each enemy has been burning
    private Dictionary<Enemy, float> accumulatedDamage = new Dictionary<Enemy, float>(); // Track accumulated damage for each enemy
    private Dictionary<Enemy, float> lastTextDisplayTimes = new Dictionary<Enemy, float>(); // Track when text was last displayed

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = false;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(DamageOverTime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (var enemy in burningEnemies)
        {
            if (enemy != null)
            {
                enemy.entityFX.StopCoroutine("BurnFX");
                enemy.entityFX.ResetToOriginalMaterial();
                enemy.RemoveBurnEffect(true); // Fire spell affects movement speed
            }
        }
        burningEnemies.Clear();
        lastTextTimes.Clear();
        enemyBurnTimes.Clear();
        accumulatedDamage.Clear();
        lastTextDisplayTimes.Clear();
    }

    public void EnableDamage()
    {
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = true;
        }
    }

    public void DisableDamage()
    {
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !burningEnemies.Contains(enemy))
        {
            burningEnemies.Add(enemy);
            enemy.entityFX.StartCoroutine("BurnFX");
            enemy.ApplyBurnEffect(true); // Fire spell affects movement speed
            
            // Initialize tracking dictionaries
            lastTextTimes[enemy] = 0f;
            enemyBurnTimes[enemy] = 0f;
            accumulatedDamage[enemy] = 0f;
            lastTextDisplayTimes[enemy] = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && burningEnemies.Contains(enemy))
        {
            burningEnemies.Remove(enemy);
            
            // Güvenli bir şekilde burn effect'i kaldır
            if (enemy.entityFX != null)
            {
                enemy.entityFX.StopCoroutine("BurnFX");
                enemy.entityFX.ResetToOriginalMaterial();
            }
            enemy.RemoveBurnEffect(true); // Fire spell affects movement speed
            
            // Remove from dictionaries with null check
            CleanupEnemyFromDictionaries(enemy);
        }
    }
    
    private void CleanupEnemyFromDictionaries(Enemy enemy)
    {
        if (enemy == null) return;
        
        if (lastTextTimes.ContainsKey(enemy))
        {
            lastTextTimes.Remove(enemy);
        }
        
        if (enemyBurnTimes.ContainsKey(enemy))
        {
            enemyBurnTimes.Remove(enemy);
        }
        
        if (accumulatedDamage.ContainsKey(enemy))
        {
            accumulatedDamage.Remove(enemy);
        }
        
        if (lastTextDisplayTimes.ContainsKey(enemy))
        {
            lastTextDisplayTimes.Remove(enemy);
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            // Belirli aralıklarla hasar ver
            for (int i = burningEnemies.Count - 1; i >= 0; i--)
            {
                if (burningEnemies[i] != null)
                {
                    Enemy enemy = burningEnemies[i];
                    
                    // Update burn time for this enemy
                    if (!enemyBurnTimes.ContainsKey(enemy))
                    {
                        enemyBurnTimes[enemy] = 0f;
                    }
                    enemyBurnTimes[enemy] += damageInterval;
                    
                    // Get burn duration from enemy's EntityFX
                    float burnDuration = 1f; // Default value
                    if (enemy.entityFX != null)
                    {
                        burnDuration = enemy.entityFX.burnDuration;
                    }
                    
                    // Check if burn effect should end
                    if (enemyBurnTimes[enemy] >= burnDuration)
                    {
                        // Remove enemy from burning list and stop effects
                        burningEnemies.RemoveAt(i);
                        if (enemy.entityFX != null)
                        {
                            enemy.entityFX.StopCoroutine("BurnFX");
                            enemy.entityFX.ResetToOriginalMaterial();
                        }
                        enemy.RemoveBurnEffect(true); // Fire spell affects movement speed
                        
                        // Clean up dictionaries
                        CleanupEnemyFromDictionaries(enemy);
                        continue;
                    }
                    
                    // Calculate damage multiplier based on how long enemy has been burning
                    float burnTimeFactor = Mathf.Clamp01(enemyBurnTimes[enemy] / damageRampUpTime);
                    float currentDamageMultiplier = Mathf.Lerp(1f, maxDamageMultiplier, burnTimeFactor);
                    
                    // Get player's spell damage using WeaponDamageManager
                    Player player = PlayerManager.instance.player;
                    float spellDamage = damagePerSecond;
                    if (player != null && player.stats is PlayerStats playerStats)
                    {
                        spellDamage = WeaponDamageManager.GetSpellDamage(playerStats);
                    }
                    
                    // Calculate damage for this interval with ramp-up
                    float intervalDamage = spellDamage * currentDamageMultiplier * damageInterval;
                    enemy.stats.TakeDamage(intervalDamage, CharacterStats.DamageType.Fire);
                    
                    // Accumulate damage for counter
                    if (!accumulatedDamage.ContainsKey(enemy))
                    {
                        accumulatedDamage[enemy] = 0f;
                    }
                    accumulatedDamage[enemy] += intervalDamage;
                    
                    // Show accumulated damage as counter (every 0.1 seconds)
                    if (!lastTextDisplayTimes.ContainsKey(enemy))
                    {
                        lastTextDisplayTimes[enemy] = 0f;
                    }
                    
                    if (Time.time - lastTextDisplayTimes[enemy] >= textDisplayInterval)
                    {
                        lastTextDisplayTimes[enemy] = Time.time;
                    }
                }
                else
                {
                    burningEnemies.RemoveAt(i);
                }
            }

            yield return new WaitForSeconds(damageInterval); // Belirli aralıklarla çalış
        }
    }
} 