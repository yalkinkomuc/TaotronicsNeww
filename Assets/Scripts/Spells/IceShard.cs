using UnityEngine;

public class IceShard : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    
    private BoxCollider2D iceCollider;
    private bool canDealDamage = false;

    private void Awake()
    {
        iceCollider = GetComponent<BoxCollider2D>();
        if (iceCollider == null)
        {
            iceCollider = gameObject.AddComponent<BoxCollider2D>();
            iceCollider.isTrigger = true;
        }
        // Başlangıçta kesinlikle kapalı olsun
        iceCollider.enabled = false;
        canDealDamage = false;
    }

    private void Start()
    {
        // Start'ta da kapalı olduğundan emin olalım
        if (iceCollider != null)
        {
            iceCollider.enabled = false;
            canDealDamage = false;
        }
    }

    // Bu metodu animasyon event ile çağıracağız
    public void EnableCollider()
    {
        if (iceCollider == null)
        {
            iceCollider = GetComponent<BoxCollider2D>();
        }
        
        canDealDamage = true;
        iceCollider.enabled = true;
    }

    // Animasyon event'i tarafından çağrılacak
    public void DestroyIceShard()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDealDamage) return;

        if (other.GetComponent<Enemy>() != null)
        {
            if (other.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Damage();
                enemy.GetComponent<CharacterStats>()?.TakeDamage(damage);
            }
        }
    }
} 