using System;
using System.Collections;
using UnityEngine;

public class EntityFX : MonoBehaviour
{
   private SpriteRenderer sr;
   private SpriteRenderer armorSr; // Armor SpriteRenderer
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
      
      // Eğer Player ise, Armor SpriteRenderer'ı bul
      var player = GetComponent<Player>();
      if (player != null)
      {
         var armorManager = player.GetComponent<PlayerArmorManager>();
         if (armorManager != null && armorManager.armors != null && armorManager.armors.Length > 0)
         {
            int currentArmorIndex = armorManager.GetCurrentArmorIndex();
            if (currentArmorIndex >= 0 && currentArmorIndex < armorManager.armors.Length)
            {
               var currentArmor = armorManager.armors[currentArmorIndex];
               if (currentArmor != null)
               {
                  armorSr = currentArmor.GetComponent<SpriteRenderer>();
                  // Eğer armor GameObject'inin child'ında ise:
                  if (armorSr == null)
                     armorSr = currentArmor.GetComponentInChildren<SpriteRenderer>();
               }
            }
         }
      }
   }

   private IEnumerator HitFX()
   {
      // GameObject'in hala aktif olup olmadığını kontrol et
      if (sr == null || gameObject == null || !gameObject.activeInHierarchy)
         yield break;
         
      sr.material = hitMat;
      
      if (hitVFXIds != null && hitVFXIds.Length > 0 && vfxSpawnPoint != null)
      {
          string selectedVFXId;
          
          // Eğer listede tek VFX varsa random seçme, birden fazla varsa random seç
          if (hitVFXIds.Length == 1)
          {
              selectedVFXId = hitVFXIds[0];
          }
          else
          {
              selectedVFXId = hitVFXIds[UnityEngine.Random.Range(0, hitVFXIds.Length)];
          }
          
          // VFXManager ve spawn point kontrolü
          if (VFXManager.Instance != null && vfxSpawnPoint.gameObject != null && vfxSpawnPoint.gameObject.activeInHierarchy)
          {
              VFXManager.Instance.PlayVFX(selectedVFXId, vfxSpawnPoint.position, transform);
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

   // Yardımcı: Hem ana hem armor SpriteRenderer'a uygula
   private void ForEachRenderer(System.Action<SpriteRenderer> action)
   {
      if (sr != null) action(sr);
      if (armorSr != null) action(armorSr);
   }

   // Aktif armor SpriteRenderer'ını güncelle
   private void UpdateArmorSpriteRenderer()
   {
      var player = GetComponent<Player>();
      if (player != null)
      {
         var armorManager = player.GetComponentInChildren<PlayerArmorManager>();
         if (armorManager != null && armorManager.armors != null && armorManager.armors.Length > 0)
         {
            int currentArmorIndex = armorManager.GetCurrentArmorIndex();
            if (currentArmorIndex >= 0 && currentArmorIndex < armorManager.armors.Length)
            {
               var currentArmor = armorManager.armors[currentArmorIndex];
               if (currentArmor != null)
               {
                  armorSr = currentArmor.GetComponent<SpriteRenderer>();
                  if (armorSr == null)
                     armorSr = currentArmor.GetComponentInChildren<SpriteRenderer>();
               }
               else
               {
                  armorSr = null;
               }
            }
            else
            {
               armorSr = null;
            }
         }
         else
         {
            armorSr = null;
         }
      }
      else
      {
         armorSr = null;
      }
   }

   public void MakeTransparent(bool _transparent)
   {
      UpdateArmorSpriteRenderer();
      ForEachRenderer(r => {
         if (_transparent)
         {
            Color newColor = r.color;
            newColor.a = 0f;
            r.color = newColor;
         }
         else
         {
            Color newColor = r.color;
            newColor.a = 1f;
            r.color = newColor;
         }
      });
   }

   public IEnumerator FlashFX(float totalDuration)
   {
      UpdateArmorSpriteRenderer();
      float endTime = Time.time + totalDuration;
      while (Time.time < endTime)
      {
         ForEachRenderer(r => r.material = hitMat);
         yield return new WaitForSeconds(flashDuration);
         ForEachRenderer(r => r.material = originalMat);
         yield return new WaitForSeconds(flashInterval);
      }
      ForEachRenderer(r => r.material = originalMat);
   }

   public IEnumerator BurnFX()
   {
      UpdateArmorSpriteRenderer();
      float endTime = Time.time + burnDuration;
      while (Time.time < endTime)
      {
         ForEachRenderer(r => r.material = burnMat);
         yield return new WaitForSeconds(burnFlashInterval);
         ForEachRenderer(r => r.material = originalMat);
         yield return new WaitForSeconds(burnFlashInterval / 2);
      }
      ForEachRenderer(r => r.material = originalMat);
   }

   public IEnumerator IceFX()
   {
      UpdateArmorSpriteRenderer();
      float endTime = Time.time + 1.5f;
      float flashDuration = 0.1f;
      float flashInterval = 0.1f;
      while (Time.time < endTime)
      {
         ForEachRenderer(r => r.material = iceMat);
         yield return new WaitForSeconds(flashDuration);
         ForEachRenderer(r => r.material = originalMat);
         yield return new WaitForSeconds(flashInterval);
      }
      ForEachRenderer(r => r.material = originalMat);
   }

   public IEnumerator ElectricFX()
   {
      UpdateArmorSpriteRenderer();
      float endTime = Time.time + 1.2f;
      float flashDuration = 0.05f;
      float flashInterval = 0.05f;
      while (Time.time < endTime)
      {
         ForEachRenderer(r => r.material = electricMat);
         yield return new WaitForSeconds(flashDuration);
         ForEachRenderer(r => r.material = originalMat);
         yield return new WaitForSeconds(flashInterval);
      }
      ForEachRenderer(r => r.material = originalMat);
   }

   public void ResetToOriginalMaterial()
   {
      if(sr != null)
         sr.material = originalMat;
   }
}
