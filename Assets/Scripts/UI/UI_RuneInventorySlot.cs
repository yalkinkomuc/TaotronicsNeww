using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_RuneInventorySlot : MonoBehaviour
{
    [SerializeField] private Image runeIcon;
    [SerializeField] private TextMeshProUGUI runeNameText;
    [SerializeField] private Button selectButton;

    private Action onClick;

    public void Setup(RuneData rune, Action onClickCallback)
    {
        if (runeIcon != null)
            runeIcon.sprite = rune.icon;
        if (runeNameText != null)
            runeNameText.text = rune.itemName;
        onClick = onClickCallback;
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
} 