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
    
    [Header("Kritik Vuruş Ayarları")]
    [SerializeField] private float criticalHitChance = 0.15f; // %15 kritik vuruş şansı
    [SerializeField] private float criticalHitMultiplier = 1.5f; // Kritik vuruş %50 daha fazla hasar verir
    
    [Header("Hasar Efektleri")]
    [SerializeField] private GameObject hitParticlePrefab; // Hasar partikül efekti
    [SerializeField] private GameObject criticalHitParticlePrefab; // Kritik vuruş partikül efekti
    
    [Header("Test Ayarları")]
    [SerializeField] private bool isTestMode = false; // Test modu
    [SerializeField] private float testDamageMin = 5f; // Min test hasarı
    [SerializeField] private float testDamageMax = 50f; // Max test hasarı
    
    [Header("Hasar Testi")]
    [SerializeField] private float testSmallDamage = 10f; // Düşük hasar (Sarı)
    [SerializeField] private float testMediumDamage = 32f; // Orta hasar (Turuncu)
    [SerializeField] private float testHighDamage = 60f; // Yüksek hasar (Kırmızı)
    [SerializeField] private float testCriticalDamage = 45f; // Kritik hasar
    [SerializeField] private float testMagicDamage = 40f; // Büyü hasarı (Mavi)
    [SerializeField] private float testHealAmount = 25f; // İyileştirme miktarı (Yeşil)
    
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
        // Editor testi - F tuşuna basılınca hasar testi başlat
        if (Input.GetKeyDown(KeyCode.F) && !testRunning)
        {
            ShowDamageTest();
        }
    }
    
    // Test için metni oynatma metodu - Unite Editörü için
    private void OnMouseDown()
    {
        if (isTestMode && !testRunning)
        {
            // Rasgele test hasarı
            float testDamage = Random.Range(testDamageMin, testDamageMax);
            
            // Rasgele: Normal, kritik veya büyü hasarı göster
            int damageType = Random.Range(0, 4);
            
            switch (damageType)
            {
                case 0:
                    TakeDamage(testDamage);
                    break;
                case 1:
                    // Kritik olarak zorla
                    TakeDamage(testDamage, true);
                    break;
                case 2:
                    TakeMagicDamage(testDamage);
                    break;
                case 3:
                    Heal(testDamage * 0.5f);
                    break;
            }
        }
        else if (!testRunning)
        {
            // Test çalışmıyorsa, test fonksiyonunu çağır
            ShowDamageTest();
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
    public void TakeDamage(float damage, bool forceCritical = false)
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
        
        // Kritik vuruş şansını hesapla (üst üste vuruşlar kritik şansını artırır)
        float adjustedCriticalChance = criticalHitChance + (hitCounter * 0.05f); // Her vuruş %5 artış
        bool isCriticalHit = forceCritical || Random.value < adjustedCriticalChance;
        
        float finalDamage = damage;
        if (isCriticalHit)
        {
            finalDamage *= criticalHitMultiplier;
            
            // Kritik vuruş partikül efekti
            if (criticalHitParticlePrefab != null)
            {
                Instantiate(criticalHitParticlePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
        else
        {
            // Normal vuruş partikül efekti
            if (hitParticlePrefab != null)
            {
                Instantiate(hitParticlePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
        
        // Can değerini güncelle
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        
        // Hasar metnini göster
        if (FloatingTextManager.Instance != null)
        {
            // Dummy'nin biraz üzerinde pozisyon belirle
            Vector3 textPosition = transform.position + Vector3.up * 1.5f;
            
            // Kritik vuruş ise farklı renkte göster
            if (isCriticalHit)
            {
                FloatingTextManager.Instance.ShowDamageText(finalDamage, textPosition, true);
                
                // Log kritik vuruş bilgisi
                Debug.Log($"Kritik vuruş! Hasar: {finalDamage}");
            }
            else
            {
                FloatingTextManager.Instance.ShowDamageText(finalDamage, textPosition);
                
                // Log normal vuruş bilgisi
                Debug.Log($"Normal vuruş. Hasar: {finalDamage}");
            }
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
    
    /// <summary>
    /// İyileşme efekti ve yeşil metin gösterir
    /// </summary>
    public void Heal(float healAmount)
    {
        // Can değerini güncelle (maksimumu geçmemesini sağla)
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        // İyileşme metnini yeşil renkte göster
        if (FloatingTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 1.5f;
            FloatingTextManager.Instance.ShowHealText(healAmount, textPosition);
            
            // Log iyileşme bilgisi
            Debug.Log($"İyileşme. Değer: {healAmount}");
        }
    }
    
    /// <summary>
    /// Test için farklı boyutlarda hasar gösterimleri
    /// </summary>
    public void ShowDamageTest()
    {
        if (!testRunning)
        {
            // Farklı boyutlarda hasarlar göster
            StartCoroutine(DamageTestRoutine());
        }
    }
    
    private IEnumerator DamageTestRoutine()
    {
        testRunning = true;
        
        Debug.Log("======= HASAR TEST BAŞLIYOR =======");
        
        // Kısa bir bekleme ile başla
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("1. Düşük Hasar (SARI)");
        TakeDamage(testSmallDamage);
        
        yield return new WaitForSeconds(1.2f);
        
        Debug.Log("2. Orta Hasar (TURUNCU)");
        TakeDamage(testMediumDamage);
        
        yield return new WaitForSeconds(1.2f);
        
        Debug.Log("3. Yüksek Hasar (KIRMIZI)");
        TakeDamage(testHighDamage);
        
        yield return new WaitForSeconds(1.2f);
        
        Debug.Log("4. Kritik Vuruş (PARLAK RENK)");
        TakeDamage(testCriticalDamage, true);
        
        yield return new WaitForSeconds(1.2f);
        
        Debug.Log("5. Büyü Hasarı (MAVİ)");
        TakeMagicDamage(testMagicDamage);
        
        yield return new WaitForSeconds(1.2f);
        
        Debug.Log("6. İyileştirme (YEŞİL)");
        Heal(testHealAmount);
        
        yield return new WaitForSeconds(1.0f);
        Debug.Log("======= HASAR TEST TAMAMLANDI =======");
        
        testRunning = false;
    }
    
    /// <summary>
    /// Üst üste vuruş sayacını belirli bir süre sonra sıfırlar
    /// </summary>
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
