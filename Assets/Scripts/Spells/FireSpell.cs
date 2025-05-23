using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireSpell : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 5f; // Increased damage per second
    [SerializeField] private float damageRampUpTime = 2f; // Time it takes to reach full damage
    [SerializeField] private float maxDamageMultiplier = 1.5f; // Maximum damage multiplier after ramp-up
    
    [Header("Visual Settings")]
    [SerializeField] private float textDisplayInterval = 0.5f; // How often to show damage text
    
    private BoxCollider2D boxCollider2D;
    private List<Enemy> burningEnemies = new List<Enemy>();
    private Dictionary<Enemy, float> lastTextTimes = new Dictionary<Enemy, float>();
    private Dictionary<Enemy, float> enemyBurnTimes = new Dictionary<Enemy, float>(); // Track how long each enemy has been burning

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
                enemy.RemoveBurnEffect();
            }
        }
        burningEnemies.Clear();
        lastTextTimes.Clear();
        enemyBurnTimes.Clear();
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
            enemy.ApplyBurnEffect();
            
            // Initialize tracking dictionaries
            lastTextTimes[enemy] = 0f;
            enemyBurnTimes[enemy] = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            burningEnemies.Remove(enemy);
            enemy.entityFX.StopCoroutine("BurnFX");
            enemy.entityFX.ResetToOriginalMaterial();
            enemy.RemoveBurnEffect();
            
            // Remove from dictionaries
            if (lastTextTimes.ContainsKey(enemy))
            {
                lastTextTimes.Remove(enemy);
            }
            
            if (enemyBurnTimes.ContainsKey(enemy))
            {
                enemyBurnTimes.Remove(enemy);
            }
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            // Her frame'de hasar ver
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
                    enemyBurnTimes[enemy] += Time.deltaTime;
                    
                    // Calculate damage multiplier based on how long enemy has been burning
                    float burnTimeFactor = Mathf.Clamp01(enemyBurnTimes[enemy] / damageRampUpTime);
                    float currentDamageMultiplier = Mathf.Lerp(1f, maxDamageMultiplier, burnTimeFactor);
                    
                    // Get player's elemental damage multiplier
                    Player player = PlayerManager.instance.player;
                    float elementalMultiplier = 1f;
                    float spellbookBonus = 0f;
                    if (player != null && player.stats is PlayerStats playerStats)
                    {
                        elementalMultiplier = player.stats.GetTotalElementalDamageMultiplier();
                        spellbookBonus = playerStats.spellbookDamage.GetValue();
                    }
                    
                    // Calculate frame damage with ramp-up and elemental multiplier
                    float frameDamage = (damagePerSecond + spellbookBonus) * currentDamageMultiplier * elementalMultiplier * Time.deltaTime;
                    enemy.stats.TakeDamage(frameDamage, CharacterStats.DamageType.Fire);
                    
                    // Accumulated damage for text display
                    bool shouldShowText = false;
                    float accumulatedDamage = frameDamage;
                    
                    // Check if it's time to show damage text
                    if (!lastTextTimes.ContainsKey(enemy))
                    {
                        lastTextTimes[enemy] = 0f;
                        shouldShowText = true;
                    }
                    else if (Time.time - lastTextTimes[enemy] >= textDisplayInterval)
                    {
                        // Show approximate damage over interval with current multiplier
                        accumulatedDamage = (damagePerSecond + spellbookBonus) * currentDamageMultiplier * elementalMultiplier * textDisplayInterval;
                        shouldShowText = true;
                        lastTextTimes[enemy] = Time.time;
                    }
                    
                    // Display magic damage text at intervals
                    if (shouldShowText && FloatingTextManager.Instance != null)
                    {
                        Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                        FloatingTextManager.Instance.ShowMagicDamageText(accumulatedDamage, textPosition);
                    }
                }
                else
                {
                    burningEnemies.RemoveAt(i);
                }
            }

            yield return null; // Her frame'de çalış
        }
    }
} 