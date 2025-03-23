using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireSpell : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 5; // Her tick'te verilecek hasar
    [SerializeField] private float tickRate = 0.2f; // Kaç saniyede bir hasar verilecek
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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if ( enemy!= null)
        {
            burningEnemies.Remove(enemy);
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            // Listede olan tüm düşmanlara periyodik hasar ver
            for (int i = burningEnemies.Count - 1; i >= 0; i--)
            {
                if (burningEnemies[i] != null)
                {
                    burningEnemies[i].DamageWithoutKnockback();
                    burningEnemies[i].stats.TakeDamage(damagePerTick);
                }
                else
                {
                    burningEnemies.RemoveAt(i);
                }
            }

            yield return new WaitForSeconds(tickRate);
        }
    }
} 