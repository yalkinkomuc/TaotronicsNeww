using UnityEngine;

public class Dummy : MonoBehaviour
{
    private Animator anim;
    private readonly int hitIndexHash = Animator.StringToHash("HitIndex");
    
    [Header("Damage Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibilityTime = 0.5f; // Short cooldown between hits
    
    private float lastHitTime = -1f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void PlayRandomHit()
    {
        int randomHit = Random.Range(0, 3);
        anim.SetInteger(hitIndexHash, randomHit);
    }
    
    public void TakeDamage(float damage)
    {
        // Check if dummy is currently invincible
        if (isInvincible || Time.time < lastHitTime + invincibilityTime)
            return;
            
        lastHitTime = Time.time;
        
        // Update health
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Show damage text
        if (FloatingTextManager.Instance != null)
        {
            // Get position slightly above the dummy
            Vector3 textPosition = transform.position + Vector3.up * 1.5f;
            // Show damage text
            FloatingTextManager.Instance.ShowDamageText(damage, textPosition);
        }
    }

    // Animation Event ile çağrılacak
    public void ResetHitIndex()
    {
        anim.SetInteger(hitIndexHash, -1); // veya 0, ama -1 daha güvenli
    }
} 