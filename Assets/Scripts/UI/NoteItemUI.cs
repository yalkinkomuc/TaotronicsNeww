using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using UnityEngine.EventSystems; // Artık gerek yok

// Not item UI component'i
public class NoteItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image backgroundImage;
    
    private NoteTextData noteData;
    private System.Action<NoteTextData> onNoteSelected;
    [SerializeField] private Button button;
    
    public NoteTextData NoteData => noteData;
    
    public void Setup(NoteTextData data, System.Action<NoteTextData> callback)
    {
        noteData = data;
        onNoteSelected = callback;
        
        if (titleText != null)
            titleText.text = data.noteTitle;
        
    }
    
    public void OnClick()
    {
        onNoteSelected?.Invoke(noteData);
    }
    
} 