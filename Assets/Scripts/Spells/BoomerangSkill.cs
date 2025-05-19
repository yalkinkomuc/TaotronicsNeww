using UnityEngine;

public class BoomerangSkill : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float maxDistance = 6f; // Maksimum gidebileceği mesafe
    [SerializeField] private float returnSpeed = 18f; // Geri dönüş hızı
    [SerializeField] private static float cooldownTime = 2f; // Cooldown süresi
    private static float currentCooldown = 0f; // Mevcut cooldown sayacı
    
    // Referanslar
    private Rigidbody2D rb;
    private Player player;
    private Vector2 startPosition;
    private bool isReturning = false;
    private Animator animator;
    
    // Bumerang silahı referansı
    private BoomerangWeaponStateMachine boomerangWeapon;
    
    // Layer kontrolü için
    private int groundLayer;
    
    public static bool CanThrow() => currentCooldown <= 0f; // Bumerang atılabilir mi kontrolü
    
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
        player = PlayerManager.instance.player;
        if (player == null)
        {
            Debug.LogError("Sahnede Player objesi bulunamadı!");
            return;
        }
        
        // Bumerang silahını bul
        boomerangWeapon = Object.FindAnyObjectByType<BoomerangWeaponStateMachine>();
        if (boomerangWeapon != null)
        {
            // Bumerang fırlatıldığında silahı deaktif et
            boomerangWeapon.gameObject.SetActive(false);
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
    
    // Görsel güncellemeleri Update'te yap
    private void Update()
    {
        // Cooldown sayacını güncelle
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }
        
        // Dönme animasyonu
        //transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Mesafe kontrolü
        if (!isReturning)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            if (distanceTraveled >= maxDistance)
            {
                StartReturning();
            }
        }
        
        // Yakalama kontrolü
        if (isReturning && player != null)
        {
            float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
            if (distanceToPlayer <= 1.0f)
            {
                if (boomerangWeapon != null)
                {
                    boomerangWeapon.gameObject.SetActive(true);
                }
                
                player.CatchBoomerang();
                Destroy(gameObject);
            }
        }
    }
    
    // Fizik güncellemelerini FixedUpdate'te yap
    private void FixedUpdate()
    {
        // Geri dönüş hareketi - fizik hesaplamalarını burada yap
        if (isReturning && player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = directionToPlayer * returnSpeed;
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
            //enemy.Damage();
            if (enemy.TryGetComponent<CharacterStats>(out CharacterStats enemyStats))
            {
                enemyStats.TakeDamage(0, CharacterStats.DamageType.Physical, ((PlayerStats)player.stats).boomerangDamage);
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
    
    private void OnDestroy()
    {
        // Bumerang yok edildiğinde cooldown'u başlat
        currentCooldown = cooldownTime;
    }
}
