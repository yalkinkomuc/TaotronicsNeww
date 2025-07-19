using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NoteCollectionUI : BaseUIPanel
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
    
    protected override void Awake()
    {
        base.Awake();
        
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
    
    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateNoteList();
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
    
    // BaseUIPanel override metodları
    public override void ShowPanel()
    {
        base.ShowPanel();
        UpdateNoteList();
    }
    
    public override void HidePanel()
    {
        base.HidePanel();
    }
    
    // ESC tuşu ile kapatma
    protected override void OnEscapePressed()
    {
        CloseCollection();
    }
    
    private enum NoteFilter
    {
        All,
        Read,
        Unread
    }
} 