using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionEffect : MonoBehaviour
{
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;
    
    private Image fadeImage;
    
    private void Awake()
    {
        // Canvas ve Image oluştur
        CreateUIElements();
        
        // Geçiş animasyonunu başlat
        StartCoroutine(TransitionRoutine());
    }
    
    private void CreateUIElements()
    {
        // Canvas oluştur
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // En üstte göster
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Fade image oluştur
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0); // Alpha 0 ile başla
        
        // Image'ı ekranın tamamını kaplayacak şekilde ayarla
        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private IEnumerator TransitionRoutine()
    {
        // Fade in
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeInTime);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        // Sahne yüklenip OnSceneLoaded çalışana kadar bekle
        yield return new WaitForSeconds(0.2f);
        
        // Fade out
        timer = fadeOutTime;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeOutTime);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        // Efekti kaldır
        Destroy(gameObject);
    }
} 