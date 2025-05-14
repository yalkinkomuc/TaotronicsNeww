using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float destroyTime = 1.0f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Vector3 randomizeIntensity = new Vector3(0.5f, 0, 0);
    [SerializeField] private float moveUpSpeed = 1.0f;
    
    [Header("Gelişmiş Randomizasyon")]
    [SerializeField] private float minSizeVariation = 0.8f;
    [SerializeField] private float maxSizeVariation = 1.2f;
    [SerializeField] private float colorVariation = 0.2f;
    
    [Header("Hasar Boyut Ayarları")]
    [SerializeField] private float minFontSize = 10f; // Minimum font boyutu
    [SerializeField] private float maxFontSize = 36f; // Maximum font boyutu
    [SerializeField] private float minDamageForMaxSize = 50f; // Maksimum boyut için gereken minimum hasar
    
    // Random hareket için değişkenler
    [SerializeField] private float wiggleAmount = 0.3f;
    [SerializeField] private float wiggleSpeed = 5f;
    private float randomSeed;
    
    private TextMeshProUGUI textMesh;
    private float originalFontSize;
    private Color originalColor;
    
    // Üst üste gelmeyi engellemek için özel değişkenler
    private static Vector3 lastPosition = Vector3.zero;
    private static int textCount = 0;
    
    // Hasar değeri
    private float damageValue = 0;
    
    void Awake()
    {
        // Try to get the component
        textMesh = GetComponent<TextMeshProUGUI>();
        
        // If not found, try to find it in children
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // If still not found, log an error
        if (textMesh == null)
        {
            Debug.LogError("FloatingText: TextMeshProUGUI component not found on " + gameObject.name);
        }
        else
        {
            // Orijinal font boyutunu ve rengi kaydet
            originalFontSize = textMesh.fontSize;
            originalColor = textMesh.color;
        }
        
        // Rastgele hareket için seed oluştur
        randomSeed = Random.value * 100f;
    }
    
    void Start()
    {
        // Text sayacını artır (üst üste binmeyi engellemek için)
        textCount++;
        
        // Apply the initial position offset
        transform.position += offset;
        
        // Prevent overlap with previous floating texts
        ApplyAntiOverlapOffset();
        
        // Add slight random offset to prevent overlapping numbers
        transform.position += new Vector3(
            Random.Range(-randomizeIntensity.x, randomizeIntensity.x),
            Random.Range(-randomizeIntensity.y, randomizeIntensity.y),
            Random.Range(-randomizeIntensity.z, randomizeIntensity.z)
        );
        
        // Güncellenen pozisyonu kaydet
        lastPosition = transform.position;
        
        // Hasara dayalı font boyutu ayarla
        if (textMesh != null)
        {
            // Hasara göre boyut hesapla
            float damageBasedSize = CalculateFontSizeByDamage(damageValue);
            
            // Hafif rastgele varyasyon ekle
            float sizeMultiplier = Random.Range(minSizeVariation, maxSizeVariation);
            textMesh.fontSize = damageBasedSize * sizeMultiplier;
            
            // Renk varyasyonu uygula
            if (colorVariation > 0)
            {
                float hVar = Random.Range(-colorVariation, colorVariation);
                float sVar = Random.Range(-colorVariation, colorVariation);
                float vVar = Random.Range(-colorVariation, colorVariation);
                
                Color.RGBToHSV(originalColor, out float h, out float s, out float v);
                h = Mathf.Clamp01(h + hVar);
                s = Mathf.Clamp01(s + sVar);
                v = Mathf.Clamp01(v + vVar);
                
                textMesh.color = Color.HSVToRGB(h, s, v);
            }
        }
        
        // Destroy after set time
        Destroy(gameObject, destroyTime);
        
        // Start fading out only if we have a valid textMesh
        if (textMesh != null)
        {
            StartCoroutine(FadeOut());
        }
        
        // Text sayacını azalt
        textCount--;
    }
    
    /// <summary>
    /// Hasar değerine göre font boyutunu hesaplar
    /// </summary>
    private float CalculateFontSizeByDamage(float damage)
    {
        // Hasar 0 veya eksi ise (iyileştirme gibi), minimum boyutu kullan
        if (damage <= 0)
            return minFontSize;
            
        // Hasarı 0-minDamageForMaxSize aralığına normalize et
        float normalizedDamage = Mathf.Clamp01(damage / minDamageForMaxSize);
        
        // Doğrusal interpolasyon ile font boyutunu belirle
        return Mathf.Lerp(minFontSize, maxFontSize, normalizedDamage);
    }
    
    /// <summary>
    /// Önceki hasar textleriyle üst üste binmeyi engellemek için özel offset ekler
    /// </summary>
    private void ApplyAntiOverlapOffset()
    {
        // Eğer başka textler de oluşturuluyorsa, onlarla üst üste gelmemesini sağla
        if (textCount > 0 && Vector3.Distance(transform.position, lastPosition) < 1.0f)
        {
            // Farklı yönler belirle (sol üst, sağ üst, sol alt, sağ alt)
            Vector3[] directions = new Vector3[] {
                new Vector3(-0.7f, 0.7f, 0),
                new Vector3(0.7f, 0.7f, 0),
                new Vector3(-0.7f, -0.7f, 0),
                new Vector3(0.7f, -0.7f, 0)
            };
            
            // Text sayısına göre farklı bir yön seç
            Vector3 offsetDir = directions[textCount % directions.Length];
            
            // Offset uygula
            transform.position += offsetDir * (0.3f + (textCount * 0.1f));
        }
    }
    
    private void Update()
    {
        // Yukarı doğru hareketi
        transform.position += new Vector3(0, moveUpSpeed * Time.deltaTime, 0);
        
        // Hafif sağa sola hareket ekle (sallanma efekti)
        float wiggleX = Mathf.Sin((Time.time + randomSeed) * wiggleSpeed) * wiggleAmount * Time.deltaTime;
        transform.position += new Vector3(wiggleX, 0, 0);
    }
    
    private IEnumerator FadeOut()
    {
        // Safety check
        if (textMesh == null) yield break;
        
        float startAlpha = textMesh.color.a;
        float rate = 1.0f / destroyTime;
        float progress = 0.0f;
        
        while (progress < 1.0)
        {
            Color color = textMesh.color;
            color.a = Mathf.Lerp(startAlpha, 0, progress);
            textMesh.color = color;
            
            progress += rate * Time.deltaTime;
            
            yield return null;
        }
    }
    
    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            
            // Hasar değerini kaydet (eğer sayıysa)
            if (float.TryParse(text, out float value))
            {
                damageValue = value;
            }
        }
    }
    
    public void SetColor(Color color)
    {
        if (textMesh != null)
        {
            textMesh.color = color;
            originalColor = color;
        }
    }
    
    /// <summary>
    /// Hasar değerini doğrudan ayarlar (text değerinden bağımsız olarak)
    /// </summary>
    public void SetDamageValue(float damage)
    {
        damageValue = damage;
    }
    
    /// <summary>
    /// Font boyutunu min-max aralığında ayarlar (Inspector'dan çağrılır)
    /// </summary>
    public void SetFontSizeRange(float min, float max, float damageThreshold)
    {
        minFontSize = min;
        maxFontSize = max;
        minDamageForMaxSize = damageThreshold;
    }
} 