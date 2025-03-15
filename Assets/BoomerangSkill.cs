using UnityEngine;
using System.Collections;

public class BoomerangSkill : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 15f;
    [SerializeField] private float normalSelfDestructTime = 4f;    // Çarpmadığında silinme süresi
    [SerializeField] private float stuckSelfDestructTime = 0.2f;   // Çarptığında silinme süresi
    
    private Rigidbody2D rb;
    private Player player;
    private bool isStuck = false;
    private Transform stuckTarget;
    private Vector2 stuckPosition; // Çarpışma noktasını saklamak için
    private Animator animator;
    private Coroutine selfDestructCoroutine;
    
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
    }

    private void Start()
    {
        player = PlayerManager.instance.player;
        if (player == null)
        {
            Debug.LogError("Sahnede Player objesi bulunamadı!");
            return;
        }
        
        // Karakterin baktığı yöne fırlat
        int direction = player.facingdir;
        rb.linearVelocity = new Vector2(direction * launchSpeed, 0);
        
        // Animasyon başlangıcı - stuck bool parametresini false yap
        if (animator != null)
        {
            animator.SetBool("BoomerangStuck", false);
        }
        
        // Normal self-destruct coroutine'i başlat (çarpmama durumu için)
        selfDestructCoroutine = StartCoroutine(SelfDestructAfterDelay(normalSelfDestructTime));
    }

    private void Update()
    {
        // Bumerang bir yere yapışmışsa
        if (isStuck && stuckTarget != null)
        {
            // Bumerangı çarpma noktasında tut - hedef hareket ediyorsa onunla birlikte hareket et
            Vector3 newPosition = stuckTarget.TransformPoint(stuckTarget.InverseTransformPoint(stuckPosition));
            transform.position = newPosition;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Eğer zaten yapışmışsa kontrol etme
        if (isStuck) return;
        
        // Çarpışma noktasını al
        Vector2 contactPoint = transform.position;
        
        // Düşmana çarpınca yapış
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            StickToTarget(other.transform, contactPoint);
            
            // Hasar ver
            enemy.Damage();
            if (enemy.TryGetComponent<CharacterStats>(out CharacterStats enemyStats))
            {
                enemyStats.TakeDamage(player.stats.damage.GetValue());
            }
        }
        // Zemine veya duvara çarpınca yapış
        else if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StickToTarget(other.transform, contactPoint);
        }
        // Yapışabileceği başka bir şey varsa
      
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Trigger çalışmadığı durumlar için
        if (isStuck) return;
        
        // Çarpışma noktasını al
        Vector2 contactPoint = collision.GetContact(0).point;
        
        // Çarpışma kontrollerini yap
        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
        {
            StickToTarget(collision.transform, contactPoint);
            
            // Hasar ver
            enemy.Damage();
            if (enemy.TryGetComponent<CharacterStats>(out CharacterStats enemyStats))
            {
                enemyStats.TakeDamage(player.stats.damage.GetValue());
            }
        }
        else if (collision.gameObject.CompareTag("Ground") || 
                 collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StickToTarget(collision.transform, contactPoint);
        }
        else if (collision.gameObject.CompareTag("Interactable") || 
                 collision.gameObject.CompareTag("Prop"))
        {
            StickToTarget(collision.transform, contactPoint);
        }
    }
    
    private void StickToTarget(Transform target, Vector2 hitPoint)
    {
        // Bumerangı hedefe yapıştır
        isStuck = true;
        stuckTarget = target;
        stuckPosition = hitPoint; // Çarpışma noktasını kaydet
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.parent = target;
        
        // Bumerangı tam çarpma noktasına yerleştir
        transform.position = hitPoint;
        
        // Animator'de BoomerangStuck bool parametresini true yap
        if (animator != null)
        {
            animator.SetBool("BoomerangStuck", true);
        }
        
        // Mevcut normal silinme coroutine'i iptal et
        if (selfDestructCoroutine != null)
        {
            StopCoroutine(selfDestructCoroutine);
        }
        
        // Yeni silinme coroutine'i başlat (çarpma durumu için daha kısa sürede)
        selfDestructCoroutine = StartCoroutine(SelfDestructAfterDelay(stuckSelfDestructTime));
    }
    
    // Belirli bir süre sonra kendini yok eden coroutine
    private IEnumerator SelfDestructAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Süre dolduğunda, bumerang hala sahnedeyse sil
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
