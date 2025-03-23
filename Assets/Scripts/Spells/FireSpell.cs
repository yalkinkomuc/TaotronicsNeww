using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireSpell : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 2f; // Saniyede verilen hasar
    private BoxCollider2D boxCollider2D;
    private List<Enemy> burningEnemies = new List<Enemy>();

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
                    // Time.deltaTime ile çarparak saniye başına hasarı frame'e böl
                    float frameDamage = damagePerSecond * Time.deltaTime;
                    burningEnemies[i].stats.TakeDamage(frameDamage);
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