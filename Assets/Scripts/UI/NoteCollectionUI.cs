using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NoteCollectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform noteListParent;
    [SerializeField] private GameObject noteItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Header("Note Display")]
    [SerializeField] private TextMeshProUGUI selectedNoteTitle;
    [SerializeField] private TextMeshProUGUI selectedNoteContent;
    [SerializeField] private Image selectedNoteIcon;
    [SerializeField] private GameObject noteDetailPanel;
    
    [Header("Filter Options")]
    [SerializeField] private Button allNotesButton;
    [SerializeField] private Button readNotesButton;
    [SerializeField] private Button unreadNotesButton;
    
    private List<NoteItemUI> noteItems = new List<NoteItemUI>();
    private NoteTextData selectedNote;
    
    private void Awake()
    {
        // Close button event
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCollection);
        }
        
        // Filter button events
        if (allNotesButton != null)
            allNotesButton.onClick.AddListener(() => FilterNotes(NoteFilter.All));
        if (readNotesButton != null)
            readNotesButton.onClick.AddListener(() => FilterNotes(NoteFilter.Read));
        if (unreadNotesButton != null)
            unreadNotesButton.onClick.AddListener(() => FilterNotes(NoteFilter.Unread));
        
        // Başlangıçta gizli
        gameObject.SetActive(false);
    }
    
    public void UpdateNoteList()
    {
        ClearNoteList();
        
        if (NoteManager.Instance == null) return;
        
        // Tüm notları al
        List<NoteTextData> allNotes = NoteManager.Instance.GetAllNotes();
        
        // Her not için UI item oluştur
        foreach (var note in allNotes)
        {
            if (note != null)
            {
                CreateNoteItem(note);
            }
        }
        
        // İlk notu seç
        if (noteItems.Count > 0)
        {
            SelectNote(noteItems[0].NoteData);
        }
    }
    
    private void CreateNoteItem(NoteTextData noteData)
    {
        if (noteItemPrefab == null || noteListParent == null) return;
        
        GameObject noteItemObj = Instantiate(noteItemPrefab, noteListParent);
        NoteItemUI noteItem = noteItemObj.GetComponent<NoteItemUI>();
        
        if (noteItem != null)
        {
            noteItem.Setup(noteData, OnNoteSelected);
            noteItems.Add(noteItem);
        }
    }
    
    private void ClearNoteList()
    {
        foreach (var item in noteItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        noteItems.Clear();
    }
    
    private void OnNoteSelected(NoteTextData noteData)
    {
        SelectNote(noteData);
    }
    
    private void SelectNote(NoteTextData noteData)
    {
        selectedNote = noteData;
        
        // UI'yi güncelle
        if (selectedNoteTitle != null)
            selectedNoteTitle.text = noteData.noteTitle;
            
        if (selectedNoteContent != null)
            selectedNoteContent.text = noteData.noteText;
            
        if (selectedNoteIcon != null)
        {
            if (noteData.noteIcon != null)
            {
                selectedNoteIcon.sprite = noteData.noteIcon;
                selectedNoteIcon.gameObject.SetActive(true);
            }
            else
            {
                selectedNoteIcon.gameObject.SetActive(false);
            }
        }
        
        // Detay panelini göster
        if (noteDetailPanel != null)
            noteDetailPanel.SetActive(true);
        
        // Seçili item'ı vurgula
        foreach (var item in noteItems)
        {
            if (item != null)
            {
                item.SetSelected(item.NoteData == noteData);
            }
        }
    }
    
    private void FilterNotes(NoteFilter filter)
    {
        if (NoteManager.Instance == null) return;
        
        List<NoteTextData> filteredNotes = new List<NoteTextData>();
        
        switch (filter)
        {
            case NoteFilter.All:
                filteredNotes = NoteManager.Instance.GetAllNotes();
                break;
            case NoteFilter.Read:
                filteredNotes = NoteManager.Instance.GetReadNotes();
                break;
            case NoteFilter.Unread:
                filteredNotes = NoteManager.Instance.GetUnreadNotes();
                break;
        }
        
        // UI'yi güncelle
        ClearNoteList();
        foreach (var note in filteredNotes)
        {
            if (note != null)
            {
                CreateNoteItem(note);
            }
        }
        
        // İlk notu seç
        if (noteItems.Count > 0)
        {
            SelectNote(noteItems[0].NoteData);
        }
        else
        {
            // Hiç not yoksa detay panelini gizle
            if (noteDetailPanel != null)
                noteDetailPanel.SetActive(false);
        }
    }
    
    public void CloseCollection()
    {
        gameObject.SetActive(false);
    }
    
    private enum NoteFilter
    {
        All,
        Read,
        Unread
    }
}

// Not item UI component'i
public class NoteItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    
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
        
        // Okunmuş mu kontrol et
        if (NoteManager.Instance != null)
        {
            bool isRead = NoteManager.Instance.IsNoteRead(data.noteID);
            // Okunmuş notlar için farklı renk veya ikon kullanabilirsin
            if (backgroundImage != null)
            {
                backgroundImage.color = isRead ? Color.gray : normalColor;
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }
    
    private void OnClick()
    {
        onNoteSelected?.Invoke(noteData);
    }
} 