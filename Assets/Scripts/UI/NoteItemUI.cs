using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Not item UI component'i
public class NoteItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color readColor = Color.gray;
    
    private NoteTextData noteData;
    private System.Action<NoteTextData> onNoteSelected;
    private Button button;
    
    public NoteTextData NoteData => noteData;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void Setup(NoteTextData data, System.Action<NoteTextData> callback)
    {
        noteData = data;
        onNoteSelected = callback;
        
        if (titleText != null)
            titleText.text = data.noteTitle;
            
        if (iconImage != null)
        {
            if (data.noteIcon != null)
            {
                iconImage.sprite = data.noteIcon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }
        
        // Okunmuş mu kontrol et ve renk ayarla
        UpdateVisualState();
    }
    
    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : GetNormalColor();
        }
    }
    
    private Color GetNormalColor()
    {
        // Okunmuş mu kontrol et
        if (NoteManager.Instance != null && noteData != null)
        {
            bool isRead = NoteManager.Instance.IsNoteRead(noteData.noteID);
            return isRead ? readColor : normalColor;
        }
        
        return normalColor;
    }
    
    private void UpdateVisualState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = GetNormalColor();
        }
    }
    
    private void OnClick()
    {
        onNoteSelected?.Invoke(noteData);
    }
    
    // Not durumu değiştiğinde çağrılır (okundu olarak işaretlendiğinde)
    public void RefreshVisualState()
    {
        UpdateVisualState();
    }
} 