using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private Image manaBarFill;
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
            Debug.LogError("ManaBar: CharacterStats component'i bulunamadı!");
            return;
        }

        if (manaBarFill == null)
        {
            Debug.LogError("ManaBar: Mana Bar Fill referansı atanmamış!");
            return;
        }

        currentFillAmount = 1f;
        targetFillAmount = 1f;
        manaBarFill.color = Color.blue;
    }

    private void Update()
    {
        if (stats == null || manaBarFill == null) return;

        targetFillAmount = stats.currentMana / stats.maxMana.GetValue();
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        manaBarFill.fillAmount = currentFillAmount;
    }

    public void UpdateManaBar(float currentMana, float maxMana)
    {
        targetFillAmount = currentMana / maxMana;
    }
} 