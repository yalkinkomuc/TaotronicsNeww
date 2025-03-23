using UnityEngine;

public class FireSpell : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField] private float maxChargeTime = 3.5f;
    
    private BoxCollider2D fireCollider;
    private float currentChargeTime = 0f;
    private Animator animator;

    private void Awake()
    {
        fireCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        
        if (fireCollider == null)
        {
            fireCollider = gameObject.AddComponent<BoxCollider2D>();
            fireCollider.isTrigger = true;
        }
        fireCollider.enabled = false; // Başlangıçta kapalı
    }

    private void Update()
    {
        currentChargeTime += Time.deltaTime;
        
        // Maksimum süre dolduysa yok et
        if (currentChargeTime >= maxChargeTime)
        {
            Destroy(gameObject);
        }
    }

    // Animation Event'ler için public metodlar
    public void EnableDamage()
    {
        fireCollider.enabled = true;
    }

    public void DisableDamage()
    {
        fireCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Damage();
                enemy.GetComponent<CharacterStats>()?.TakeDamage(damage);
            }
        }
    }
} 