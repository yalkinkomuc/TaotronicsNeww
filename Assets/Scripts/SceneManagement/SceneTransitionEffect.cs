using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionEffect : MonoBehaviour
{
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private float waitAfterSceneLoad = 1f; // Sahne yüklendikten sonra beklenecek süre
    
    private Image fadeImage;
    private Canvas canvas;
    
    private void Awake()
    {
        // Canvas ve Image oluştur
        CreateUIElements();
        
        // Hemen siyah ekranı göster
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        
        // Geçiş animasyonunu başlat
        StartCoroutine(TransitionRoutine());
    }
    
    private void CreateUIElements()
    {
        // Canvas oluştur
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // En üstte göster
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Fade image oluştur
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1); // Başlangıçta tamamen siyah
        
        // Image'ı ekranın tamamını kaplayacak şekilde ayarla
        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private IEnumerator TransitionRoutine()
    {
        // Başlangıçta tamamen siyah ekran
        
        // Sahne yüklenene kadar bekle
        yield return new WaitForSeconds(0.1f);
        
        // Sahne yüklendikten sonra ekstra bekleme süresi
        yield return new WaitForSeconds(waitAfterSceneLoad);
        
        // Fade out (ekran açılması)
        float timer = fadeOutTime;
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