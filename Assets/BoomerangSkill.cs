using UnityEngine;

public class BoomerangSkill : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float maxDistance = 6f; // Maksimum gidebileceği mesafe
    [SerializeField] private float returnSpeed = 18f; // Geri dönüş hızı
    [SerializeField] private float rotationSpeed = 720f; // Dönme hızı (derece/saniye)
    
    // Referanslar
    private Rigidbody2D rb;
    private Player player;
    private Vector2 startPosition;
    private bool isReturning = false;
    private Animator animator;
    
    // Layer kontrolü için
    private int groundLayer;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (rb == null)
        {
            Debug.LogError("Bumerang objesinde Rigidbody2D bulunamadı!");
            return;
        }
        
        rb.gravityScale = 0f;
        
        // Ground layer'ını al
        groundLayer = LayerMask.NameToLayer("Ground");
    }
    
    private void Start()
    {
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Sahnede Player objesi bulunamadı!");
            return;
        }
        
        startPosition = transform.position;
        int direction = player.facingdir;
        
        // Karakterin baktığı yöne fırlat
        rb.linearVelocity = new Vector2(direction * speed, 0);
        
        // Animator başlangıç durumu - eğer varsa
        if (animator != null)
        {
            animator.SetBool("BoomerangStuck", false);
        }
    }
    
    private void Update()
    {
        // Bumerangı döndür
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Geri dönmeli mi kontrol et
        if (!isReturning)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            if (distanceTraveled >= maxDistance)
            {
                StartReturning();
            }
        }
        // Geri dönüş hareketi
        else if (player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = directionToPlayer * returnSpeed;
            
            // Oyuncuya yaklaştıysa yakala
            float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
            if (distanceToPlayer <= 1.0f)
            {
                player.CatchBoomerang();
                Destroy(gameObject);
            }
        }
    }
    
    private void StartReturning()
    {
        isReturning = true;
        rb.linearVelocity = Vector2.zero; // Mevcut hızı sıfırla
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Zaten dönüyorsa işlem yapma
        if (isReturning) return;
        
        // Düşmana çarpınca hasar ver ve geri dön
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.Damage();
            if (enemy.TryGetComponent<CharacterStats>(out CharacterStats enemyStats))
            {
                enemyStats.TakeDamage(player.stats.damage.GetValue());
            }
            
            StartReturning();
        }
        // Ground layer'ına çarpınca hemen geri dön
        else if (other.gameObject.layer == groundLayer)
        {
            StartReturning();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Zaten dönüyorsa işlem yapma
        if (isReturning) return;
        
        // Ground layer'ına çarpınca hemen geri dön (Collision için de kontrol)
        if (collision.gameObject.layer == groundLayer)
        {
            StartReturning();
        }
    }
}
