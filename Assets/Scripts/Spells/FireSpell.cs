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
    private Dictionary<Enemy, GameObject> fireSpellFloatingTexts = new Dictionary<Enemy, GameObject>();
    private Dictionary<Enemy, float> fireSpellTotalDamages = new Dictionary<Enemy, float>();

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
        foreach (var kvp in fireSpellFloatingTexts)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }
        fireSpellFloatingTexts.Clear();
        fireSpellTotalDamages.Clear();
        Destroy(gameObject);
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
            lastTextTimes[enemy] = 0f;
            enemyBurnTimes[enemy] = 0f;
            fireSpellTotalDamages[enemy] = 0f;
            if (FloatingTextManager.Instance != null)
            {
                Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                GameObject textObj = Instantiate(FloatingTextManager.Instance.floatingTextPrefab, textPosition, Quaternion.identity, FloatingTextManager.Instance.canvasTransform);
                fireSpellFloatingTexts[enemy] = textObj;
                var floatingText = textObj.GetComponent<FloatingText>();
                if (floatingText != null)
                {
                    floatingText.SetText("0");
                    floatingText.SetColor(Color.red);
                    floatingText.SetAsMagicDamage(true);
                }
            }
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
            if (lastTextTimes.ContainsKey(enemy))
            {
                lastTextTimes.Remove(enemy);
            }
            if (enemyBurnTimes.ContainsKey(enemy))
            {
                enemyBurnTimes.Remove(enemy);
            }
            if (fireSpellFloatingTexts.ContainsKey(enemy))
            {
                if (fireSpellFloatingTexts[enemy] != null)
                    Destroy(fireSpellFloatingTexts[enemy]);
                fireSpellFloatingTexts.Remove(enemy);
            }
            if (fireSpellTotalDamages.ContainsKey(enemy))
            {
                fireSpellTotalDamages.Remove(enemy);
            }
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            for (int i = burningEnemies.Count - 1; i >= 0; i--)
            {
                if (burningEnemies[i] != null)
                {
                    Enemy enemy = burningEnemies[i];
                    if (!enemyBurnTimes.ContainsKey(enemy))
                    {
                        enemyBurnTimes[enemy] = 0f;
                    }
                    enemyBurnTimes[enemy] += Time.deltaTime;
                    float burnTimeFactor = Mathf.Clamp01(enemyBurnTimes[enemy] / damageRampUpTime);
                    float currentDamageMultiplier = Mathf.Lerp(1f, maxDamageMultiplier, burnTimeFactor);
                    Player player = PlayerManager.instance.player;
                    float elementalMultiplier = 1f;
                    if (player != null && player.stats != null)
                    {
                        elementalMultiplier = player.stats.GetTotalElementalDamageMultiplier();
                    }
                    float frameDamage = damagePerSecond * currentDamageMultiplier * elementalMultiplier * Time.deltaTime;
                    enemy.stats.TakeDamage(frameDamage, CharacterStats.DamageType.Fire);
                    if (!fireSpellTotalDamages.ContainsKey(enemy))
                        fireSpellTotalDamages[enemy] = 0f;
                    fireSpellTotalDamages[enemy] += frameDamage;
                    if (fireSpellFloatingTexts.ContainsKey(enemy) && fireSpellFloatingTexts[enemy] != null)
                    {
                        var floatingText = fireSpellFloatingTexts[enemy].GetComponent<FloatingText>();
                        if (floatingText != null)
                        {
                            floatingText.SetText(Mathf.RoundToInt(fireSpellTotalDamages[enemy]).ToString());
                        }
                        fireSpellFloatingTexts[enemy].transform.position = enemy.transform.position + Vector3.up * 1.5f;
                    }
                }
                else
                {
                    burningEnemies.RemoveAt(i);
                }
            }
            yield return null;
        }
    }
} 