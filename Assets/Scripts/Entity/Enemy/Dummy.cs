using UnityEngine;
using System.Collections;

public class Dummy : MonoBehaviour
{
    private Animator anim;
    private readonly int hitIndexHash = Animator.StringToHash("HitIndex");
    
    [Header("Hasar Ayarları")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibilityTime = 0.5f; // Vuruşlar arası kısa bekleme
    
    [Header("Hasar Efektleri")]
    [SerializeField] private GameObject hitParticlePrefab; // Hasar partikül efekti
    
    [Header("Test Ayarları")]
    [SerializeField] private bool isTestMode = false; // Test modu
    [SerializeField] private float testDamageMin = 5f; // Min test hasarı
    [SerializeField] private float testDamageMax = 50f; // Max test hasarı
    
   
    
    private float lastHitTime = -1f;
    private int hitCounter = 0; // Üst üste vuruş sayacı
    private Coroutine resetHitCounterRoutine;
    private bool testRunning = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
       
    }
    
    // Test için metni oynatma metodu - Unite Editörü için
    private void OnMouseDown()
    {
        if (isTestMode && !testRunning)
        {
            // Rasgele test hasarı
            float testDamage = Random.Range(testDamageMin, testDamageMax);
            
            // Rasgele: Normal veya büyü hasarı göster
            int damageType = Random.Range(0, 3);
            
            switch (damageType)
            {
                case 0:
                    TakeDamage(testDamage, 0); // İlk kombo
                    break;
                case 1:
                    TakeDamage(testDamage, 1); // İkinci kombo
                    break;
                case 2:
                    TakeDamage(testDamage, 2); // Üçüncü kombo
                    break;
            }
        }
       
    }

    /// <summary>
    /// Rastgele vuruş animasyonu oynatır
    /// </summary>
    public void PlayRandomHit()
    {
        int randomHit = Random.Range(0, 3);
        anim.SetInteger(hitIndexHash, randomHit);
    }
    
    /// <summary>
    /// Dummy'e hasar verir ve hasar metnini gösterir
    /// </summary>
    public void TakeDamage(float damage, int comboCount = 0,bool isCritical = false)
    {
        // Eğer dummy şu an hasar alamıyorsa çık
        if (isInvincible || Time.time < lastHitTime + invincibilityTime)
            return;
            
        lastHitTime = Time.time;
        
        // Üst üste vuruş sayacını artır
        hitCounter++;
        
        // Eğer önceki bir resetHitCounter rutini varsa iptal et
        if (resetHitCounterRoutine != null)
        {
            StopCoroutine(resetHitCounterRoutine);
        }
        
        // Yeni bir resetHitCounter rutini başlat
        resetHitCounterRoutine = StartCoroutine(ResetHitCounterAfterDelay(1.5f));
        
        // Normal vuruş partikül efekti
        if (hitParticlePrefab != null)
        {
            Instantiate(hitParticlePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        
        // Can değerini güncelle
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Hasar metnini göster
        if (FloatingTextManager.Instance != null)
        {
            // Dummy'nin biraz üzerinde pozisyon belirle
            Vector3 textPosition = transform.position + Vector3.up * 1.5f;
            
            // Kombo sayısına göre farklı renkte metin göster
            FloatingTextManager.Instance.ShowComboDamageText(damage, textPosition, comboCount);
            
            // Log vuruş bilgisi
            Debug.Log($"Kombo {comboCount + 1}. Hasar: {damage}");
        }
        
        // Vuruş animasyonu
        PlayRandomHit();
    }
    
    /// <summary>
    /// Büyü hasarı verir ve özel mavi metin gösterir
    /// </summary>
    public void TakeMagicDamage(float damage)
    {
        // Eğer dummy şu an hasar alamıyorsa çık
        if (isInvincible || Time.time < lastHitTime + invincibilityTime * 0.5f) // Büyü hasarı daha sık gelebilir
            return;
            
        lastHitTime = Time.time;
        
        // Can değerini güncelle
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Hasar metnini mavi renkte göster
        if (FloatingTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 1.5f;
            FloatingTextManager.Instance.ShowMagicDamageText(damage, textPosition);
            
            // Log büyü vuruş bilgisi
            Debug.Log($"Büyü hasarı. Değer: {damage}");
        }
        
        // Büyü hasarı animasyonu
        PlayRandomHit();
    }
    
    
    private IEnumerator ResetHitCounterAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hitCounter = 0;
        resetHitCounterRoutine = null;
    }

    // Animator Event ile çağrılır
    public void ResetHitIndex()
    {
        anim.SetInteger(hitIndexHash, -1); // veya 0, ama -1 daha güvenli
    }
} 
