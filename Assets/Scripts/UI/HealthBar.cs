using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private float smoothSpeed = 10f;

    private CharacterStats stats;
    private float targetFillAmount;
    private float currentFillAmount;

    private void Start()
    {
        stats = GetComponent<CharacterStats>();
        if (stats == null)
        {
            stats = GetComponentInParent<CharacterStats>();
        }

        if (stats == null)
        {
            Debug.LogError("HealthBar: CharacterStats component'i bulunamadı!");
            return;
        }

        if (healthBarFill == null)
        {
            Debug.LogError("HealthBar: Health Bar Fill referansı atanmamış!");
            return;
        }

        currentFillAmount = 1f;
        targetFillAmount = 1f;
        healthBarFill.color = Color.red;
    }

    private void Update()
    {
        if (stats == null || healthBarFill == null) return;

        targetFillAmount = stats.currentHealth / stats.maxHealth.GetValue();
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        healthBarFill.fillAmount = currentFillAmount;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        targetFillAmount = currentHealth / maxHealth;
    }
} 