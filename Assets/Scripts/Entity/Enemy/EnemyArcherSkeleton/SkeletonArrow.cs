using UnityEngine;

public class SkeletonArrow : MonoBehaviour
{
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float arrowLifetime = 5f;
    private Rigidbody2D rb;
    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, arrowLifetime); // Belirli süre sonra oku yok et
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * arrowSpeed;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Deal physical damage from ranged attack
                player.TakePlayerDamage(null, CharacterStats.DamageType.Physical);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
} 