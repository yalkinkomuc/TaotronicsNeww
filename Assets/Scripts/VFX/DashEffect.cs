using UnityEngine;
using System.Collections;

public class DashEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    public float fadeDuration = 0.5f; // Kaç saniyede kaybolacağı

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
       
        
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            yield return null;
        }

        
    }
}