using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSelectionPanel : InGameUI,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] private Button[] buttons;
    
    private Vector3 initialScale = new Vector3(1,1,1);

    [SerializeField] private float scaleTime;
    [SerializeField] float targetScale = 1.20f;
    
    private bool isScalingUp = false; // Flag to prevent multiple tweens

    private void Awake()
    {
        foreach (Button button in buttons)
        {
            button.image.transform.localScale = initialScale;
        }
    }

    private void Update()
    {
        if (UserInput.IsSkillScreenBeingPressed && !isScalingUp)
        {
            isScalingUp = true;
            ScaleButtonsOnHover();
        }

        if (UserInput.WasSkillScreenReleased && isScalingUp)
        {
            isScalingUp = false;
            ScaleButtonsOffHover();
        }
    }

    private void ScaleButtonsOnHover()
    {
        
        foreach (Button button in buttons)
        {
            LeanTween.scale(button.gameObject, initialScale * targetScale, scaleTime)
                .setEase(LeanTweenType.easeOutQuad);
        }
    }

    private void ScaleButtonsOffHover()
    {
        foreach (Button button in buttons)
        {
            LeanTween.scale(button.gameObject, initialScale, scaleTime).setEase(LeanTweenType.easeInQuad);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (UserInput.IsSkillScreenBeingPressed)
        {
            ScaleButtonsOnHover();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (UserInput.IsSkillScreenBeingPressed)
        {
            ScaleButtonsOffHover();
        }
    }
}
