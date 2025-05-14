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
    
    private TextMeshProUGUI textMesh;
    
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
            Debug.LogError("FloatingText: TextMeshPro component not found on " + gameObject.name);
        }
    }
    
    void Start()
    {
        // Apply the initial position offset
        transform.position += offset;
        
        // Add slight random offset to prevent overlapping numbers
        transform.position += new Vector3(
            Random.Range(-randomizeIntensity.x, randomizeIntensity.x),
            Random.Range(-randomizeIntensity.y, randomizeIntensity.y),
            Random.Range(-randomizeIntensity.z, randomizeIntensity.z)
        );
        
        // Destroy after set time
        Destroy(gameObject, destroyTime);
        
        // Start fading out only if we have a valid textMesh
        if (textMesh != null)
        {
            StartCoroutine(FadeOut());
        }
    }
    
    private void Update()
    {
        // Move text up
        transform.position += new Vector3(0, moveUpSpeed * Time.deltaTime, 0);
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
        }
    }
    
    public void SetColor(Color color)
    {
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }
} 