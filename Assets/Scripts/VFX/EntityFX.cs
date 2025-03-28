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

   [Header("Death VFX")]
   [SerializeField] private string[] deathVFXIds;

   private void Start()
   {
      sr = GetComponentInChildren<SpriteRenderer>();
      animator = GetComponentInChildren<Animator>();
      originalMat = sr.material;
      
   }

   private IEnumerator HitFX()
   {
      sr.material = hitMat;
      
      if (hitVFXIds != null && hitVFXIds.Length > 0)
      {
          string randomVFXId = hitVFXIds[UnityEngine.Random.Range(0, hitVFXIds.Length)];
          VFXManager.Instance.PlayVFX(randomVFXId, vfxSpawnPoint.position, transform);
      }
      
      yield return new WaitForSeconds(.2f);
      sr.material = originalMat;
   }
   
   // Yeni ölüm efekti metodu
   public void PlayDeathEffect()
   {
      // Ölüm VFX oynat (eğer varsa)
      if (deathVFXIds != null && deathVFXIds.Length > 0 && vfxSpawnPoint != null)
      {
         string randomDeathVFXId = deathVFXIds[UnityEngine.Random.Range(0, deathVFXIds.Length)];
         VFXManager.Instance.PlayVFX(randomDeathVFXId, vfxSpawnPoint.position, transform);
      }
      
      // Ölüm fade efektini hemen başlatma - animasyon bitince AnimationFinishTrigger tarafından tetiklenecek
      // Bu şekilde animasyon tamamlanmadan karakter kaybolmaz
   }
   
   public void StartFadeOutAndDestroy()
   {
      if (sr != null)
      {
         float currentAnimationLength = 0f;
         if (animator != null)
         {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            currentAnimationLength = stateInfo.length;
            //Debug.Log($"Animation Length: {currentAnimationLength}");
         }

         StartCoroutine(FadeOutAndDestroyCoroutine(currentAnimationLength));
      }
   }

   private IEnumerator FadeOutAndDestroyCoroutine(float animationLength)
   {
      yield return new WaitForSeconds(animationLength);
      
      float fadeTime = 1f;
      float elapsedTime = 0f;
      Color startColor = sr.color;

      while (elapsedTime < fadeTime)
      {
         elapsedTime += Time.deltaTime;
         float newAlpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeTime);
         sr.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
         yield return null;
      }

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
       sr.material = burnMat;
       // Artık flash yok, sürekli turuncu kalacak
       while (true)
       {
           yield return null;
       }
   }

   public void ResetToOriginalMaterial()
   {
      if(sr != null)
         sr.material = originalMat;
   }
}
