using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class NoteCollectionUI : BaseUIPanel
{
    [Header("UI References")]
    [SerializeField] private Transform noteListParent;
    [SerializeField] private GameObject noteItemPrefab;
    
    [SerializeField] private TextMeshProUGUI selectedNoteContent;
    [SerializeField] private GameObject noteDetailPanel;
    
    private List<NoteItemUI> noteItems = new List<NoteItemUI>();
    private NoteTextData selectedNote;
    private NoteItemUI lastSelectedItem;
    
    protected override void Awake()
    {
        base.Awake();
        // Başlangıçta gizli
        gameObject.SetActive(false);
    }

    public void DebugTest()
    {
        Debug.Log("DebugTest");
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

        // Eğer en az bir not varsa, ilk notu otomatik seç
        if (noteItems.Count > 0)
        {
            SelectNote(noteItems[0].NoteData);
            // İstersen ilk NoteItemUI'ı EventSystem ile seçili de yapabilirsin:
            // EventSystem.current.SetSelectedGameObject(noteItems[0].gameObject);
        }
    }
    
    private void CreateNoteItem(NoteTextData noteData)
    {
        if (noteItemPrefab == null || noteListParent == null) return;
        
        GameObject noteItemObj = Instantiate(noteItemPrefab, noteListParent);
        NoteItemUI noteItem = noteItemObj.GetComponent<NoteItemUI>();
        
        if (noteItem != null)
        {
            // Kullanıcı NoteItemUI üzerindeki button'a tıkladığında, ilgili notun içeriği sağda gözükecek
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
    
     void  Update()
    {
        GameObject selectedObj = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selectedObj != null)
        {
            NoteItemUI selectedItem = selectedObj.GetComponent<NoteItemUI>();
            if (selectedItem != null && selectedItem != lastSelectedItem)
            {
                lastSelectedItem = selectedItem;
                SelectNote(selectedItem.NoteData);
            }
        }
    }

    private void SelectNote(NoteTextData noteData)
    {
        selectedNote = noteData;
        if (selectedNoteContent != null)
            selectedNoteContent.text = noteData.noteText;
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
    
    
    // ESC tuşu ile kapatma
    protected override void OnEscapePressed()
    {
        CloseCollection();
    }
    
 
} 