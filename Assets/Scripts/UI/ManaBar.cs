using UnityEngine;
using UnityEngine.UI;

public class ManaBar : BaseUIPanel
{
    [SerializeField] private Image manaBarFill;
    [SerializeField] private float smoothSpeed = 10f;

    private CharacterStats stats;
    private float targetFillAmount;
    private float currentFillAmount;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeComponents();
        
        if (stats != null && manaBarFill != null)
        {
            currentFillAmount = stats.currentMana / stats.maxMana.GetValue();
            targetFillAmount = currentFillAmount;
            manaBarFill.fillAmount = currentFillAmount;
            manaBarFill.color = Color.blue;
        }
    }
    
    private void InitializeComponents()
    {
        if (stats != null) return;
        
        stats = GetComponent<CharacterStats>();
        if (stats == null)
        {
            stats = GetComponentInParent<CharacterStats>();
        }

        if (stats == null)
        {

        }

        if (manaBarFill == null)
        {

        }
    }

    private void Update()
    {
        if (stats == null || manaBarFill == null) return;

        targetFillAmount = Mathf.Clamp01(stats.currentMana / stats.maxMana.GetValue());
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        manaBarFill.fillAmount = currentFillAmount;
    }

    public void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaBarFill == null) return;
        
        targetFillAmount = Mathf.Clamp01(currentMana / maxMana);
        manaBarFill.fillAmount = Mathf.Lerp(manaBarFill.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed * 3f);
        
        //Debug.Log($"ManaBar updated: {currentMana}/{maxMana} = {targetFillAmount}");
    }
} 