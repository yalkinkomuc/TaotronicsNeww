using System;
using System.Collections;
using UnityEngine;

public class EntityFX : MonoBehaviour
{
   private SpriteRenderer sr;
   private Animator animator;

   [Header("FlashFX")] 
   [SerializeField] private Material hitMat;
   [SerializeField] private float flashDuration = 0.2f; // Tek flash süresi
   [SerializeField] private float flashInterval = 0.2f; // Flash'lar arası bekleme süresi
   private Material originalMat;

   [Header("Hit VFX")]
   [SerializeField] private string[] hitVFXIds;
   [SerializeField] private Transform vfxSpawnPoint; // Unity'de sürükle bırak

   [Header("BurnFX")] 
   [SerializeField] public Material burnMat; // Unity'de turuncu flash materyali atanacak
   [SerializeField] public float burnDuration = 1f; // Default burn duration
   [SerializeField] private float burnFlashInterval = 0.10f; // Flash interval during burn effect

   [Header("IceFX")]
   [SerializeField] public Material iceMat; // Unity'de buz mavisi materyal atanacak

   [Header("ElectricFX")]
   [SerializeField] public Material electricMat; // Sarı elektrik efekti materyali

   [Header("Death Effect")]
   [SerializeField] private Transform deathEffectSpawnPoint;
   [SerializeField] private GameObject explosionEffectPrefab; // Patlama efekti prefab'ı
   [SerializeField] private int explosionCount = 5; // Kaç tane patlama efekti spawnlanacak
   
   private bool isFadingOut = false; // Fade işlemi başladı mı kontrolü

   private void Start()
   {
      sr = GetComponentInChildren<SpriteRenderer>();
      animator = GetComponentInChildren<Animator>();
      originalMat = sr.material;
      
   }

   private IEnumerator HitFX()
   {
      // GameObject'in hala aktif olup olmadığını kontrol et
      if (sr == null || gameObject == null || !gameObject.activeInHierarchy)
         yield break;
         
      sr.material = hitMat;
      
      if (hitVFXIds != null && hitVFXIds.Length > 0 && vfxSpawnPoint != null)
      {
          string randomVFXId = hitVFXIds[UnityEngine.Random.Range(0, hitVFXIds.Length)];
          
          // VFXManager ve spawn point kontrolü
          if (VFXManager.Instance != null && vfxSpawnPoint.gameObject != null && vfxSpawnPoint.gameObject.activeInHierarchy)
          {
              VFXManager.Instance.PlayVFX(randomVFXId, vfxSpawnPoint.position, transform);
          }
      }
      
      yield return new WaitForSeconds(.2f);
      
      // Coroutine sonunda da GameObject kontrolü
      if (sr != null && gameObject != null && gameObject.activeInHierarchy)
      {
         sr.material = originalMat;
      }
   }
   
   // Yeni ölüm efekti metodu
   
   
   public void StartFadeOutAndDestroy()
   {
      if (sr != null && !isFadingOut)
      {
         isFadingOut = true;
         StartCoroutine(FadeOutAndDestroyCoroutine());
      }
   }

   private IEnumerator FadeOutAndDestroyCoroutine()
   {
      // Birden fazla patlama efekti oluştur
      if (explosionEffectPrefab != null)
      {
         // 4-5 adet patlama efekti oluştur (explosionCount kadar)
         for (int i = 0; i < explosionCount; i++)
         {
            // Her patlama için farklı bir offset ile rastgele konumlandır
            Vector3 explosionPosition = deathEffectSpawnPoint.position;
            explosionPosition.x += UnityEngine.Random.Range(-0.5f, 0.5f);
            explosionPosition.y += UnityEngine.Random.Range(-0.1f, 0.1f);
            
            Instantiate(explosionEffectPrefab, explosionPosition, Quaternion.identity);
            
            // Her efekt arasında çok kısa bir gecikme ekleyebiliriz
            if (i < explosionCount - 1) {
               yield return new WaitForSeconds(0.1f);
            }
         }
      }

      // Yavaşça transparan ol
      float fadeTime = .25f;
      float elapsedTime = 0f;
      Color startColor = sr.color;

      while (elapsedTime < fadeTime)
      {
         elapsedTime += Time.deltaTime;
         float newAlpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeTime);
         sr.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
         yield return null;
      }
      
      // İşlem bitti, nesneyi yok et
      isFadingOut = false; // Bayrak sıfırla (aslında gerek yok çünkü nesne yok edilecek)
      Destroy(gameObject);
   }

   public void MakeTransparent(bool _transparent)
   {
      if (_transparent)
      {
         Color newColor = sr.color;
         newColor.a = 0f;
         sr.color = newColor;
      }
      else
      {
         Color newColor = sr.color;
         newColor.a = 1f;
         sr.color = newColor;
      }
   }

   public IEnumerator FlashFX(float totalDuration)
   {
       float endTime = Time.time + totalDuration;
       
       while (Time.time < endTime)
       {
           // Flash açık
           sr.material = hitMat;
           yield return new WaitForSeconds(flashDuration);
           
           // Flash kapalı
           sr.material = originalMat;
           yield return new WaitForSeconds(flashInterval);
       }
       
       // Son durumda normal materyal
       sr.material = originalMat;
   }

   public IEnumerator BurnFX()
   {
       // Get the current time plus burn duration
       float endTime = Time.time + burnDuration;
       
       // Flash effect with burn material
       while (Time.time < endTime)
       {
           // Burn material on
           sr.material = burnMat;
           yield return new WaitForSeconds(burnFlashInterval);
           
           // Original material briefly
           sr.material = originalMat;
           yield return new WaitForSeconds(burnFlashInterval / 2);
       }
       
       // Reset to original material when finished
       sr.material = originalMat;
   }

   public IEnumerator IceFX()
   {
       float endTime = Time.time + 1.5f;
       float flashDuration = 0.1f; // Her flash'ın süresi
       float flashInterval = 0.1f; // Flash'lar arası bekleme süresi
       
       while (Time.time < endTime)
       {
           // Flash açık
           sr.material = iceMat;
           yield return new WaitForSeconds(flashDuration);
           
           // Flash kapalı
           sr.material = originalMat;
           yield return new WaitForSeconds(flashInterval);
       }
       
       // Son durumda normal materyal
       sr.material = originalMat;
   }

   public IEnumerator ElectricFX()
   {
       float endTime = Time.time + 1.2f;
       float flashDuration = 0.05f; // Daha hızlı flash (elektrik çarpması gibi)
       float flashInterval = 0.05f; // Flash'lar arası kısa bekleme
       
       while (Time.time < endTime)
       {
           // Flash açık
           sr.material = electricMat;
           yield return new WaitForSeconds(flashDuration);
           
           // Flash kapalı
           sr.material = originalMat;
           yield return new WaitForSeconds(flashInterval);
       }
       
       // Son durumda normal materyal
       sr.material = originalMat;
   }

   public void ResetToOriginalMaterial()
   {
      if(sr != null)
         sr.material = originalMat;
   }
}
