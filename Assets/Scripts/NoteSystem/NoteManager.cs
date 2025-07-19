using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [Header("Note Management")]
    [SerializeField] private List<NoteTextData> allNotes = new List<NoteTextData>();
    
    [Header("Note Collection")]
    [SerializeField] private bool showNoteCollection = true;
    [SerializeField] private GameObject noteCollectionUI;
    
    [Header("Tab Integration")]
    [SerializeField] private TabManager tabManager;
    
    
    // Singleton pattern
    public static NoteManager Instance { get; private set; }
    
    // Okunan notları takip etmek için
    private Dictionary<string, bool> readNotes = new Dictionary<string, bool>();
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Kayıtlı notları yükle
        LoadReadNotes();
        
        // TabManager'a not koleksiyonu tab'ını ekle
        SetupNoteCollectionTab();
    }
    
    // Not ekleme
    public void AddNote(NoteTextData noteData)
    {
        if (noteData != null && !allNotes.Contains(noteData))
        {
            allNotes.Add(noteData);
        }
    }
    
    // Not okundu olarak işaretleme
    public void MarkNoteAsRead(string noteID)
    {
        if (!string.IsNullOrEmpty(noteID))
        {
            readNotes[noteID] = true;
            SaveReadNotes();
        }
    }
    
    // Not okunmuş mu kontrol etme
    public bool IsNoteRead(string noteID)
    {
        return readNotes.ContainsKey(noteID) && readNotes[noteID];
    }
    
    // Tüm okunmamış notları alma
    public List<NoteTextData> GetUnreadNotes()
    {
        List<NoteTextData> unreadNotes = new List<NoteTextData>();
        
        foreach (var note in allNotes)
        {
            if (note != null && !IsNoteRead(note.noteID))
            {
                unreadNotes.Add(note);
            }
        }
        
        return unreadNotes;
    }
    
    // Tüm okunan notları alma
    public List<NoteTextData> GetReadNotes()
    {
        List<NoteTextData> readNotesList = new List<NoteTextData>();
        
        foreach (var note in allNotes)
        {
            if (note != null && IsNoteRead(note.noteID))
            {
                readNotesList.Add(note);
            }
        }
        
        return readNotesList;
    }
    
    // Tüm notları alma
    public List<NoteTextData> GetAllNotes()
    {
        return new List<NoteTextData>(allNotes);
    }
    
    // Not koleksiyonu UI'sini açma
    public void OpenNoteCollection()
    {
        if (tabManager != null)
        {
            // TabManager'ı bul ve notlar tab'ına geç
            // Bu metod TabManager'ın kendi açma metodunu çağıracak
            // Şimdilik direkt panel'i açalım
            if (noteCollectionUI != null)
            {
                noteCollectionUI.SetActive(true);
                // Not koleksiyonu UI'sini güncelle
                NoteCollectionUI collectionUI = noteCollectionUI.GetComponent<NoteCollectionUI>();
                if (collectionUI != null)
                {
                    collectionUI.UpdateNoteList();
                }
            }
        }
        else
        {
            // Fallback: direkt panel aç
            if (noteCollectionUI != null)
            {
                noteCollectionUI.SetActive(true);
                NoteCollectionUI collectionUI = noteCollectionUI.GetComponent<NoteCollectionUI>();
                if (collectionUI != null)
                {
                    collectionUI.UpdateNoteList();
                }
            }
        }
    }
    
    // Not koleksiyonu UI'sini kapatma
    public void CloseNoteCollection()
    {
        if (noteCollectionUI != null)
        {
            noteCollectionUI.SetActive(false);
        }
    }
    
    // TabManager'a not koleksiyonu tab'ını ekleme
    private void SetupNoteCollectionTab()
    {
        if (tabManager != null && noteCollectionUI != null)
        {
            TabData noteTab = new TabData
            {
                tabName = "Notlar",
                tabPanel = noteCollectionUI,
                onTabSelected = () => {
                    // Tab seçildiğinde not listesini güncelle
                    NoteCollectionUI collectionUI = noteCollectionUI.GetComponent<NoteCollectionUI>();
                    if (collectionUI != null)
                    {
                        collectionUI.UpdateNoteList();
                    }
                }
            };
            
            tabManager.AddTab(noteTab);
            Debug.Log("NoteCollectionUI tab'ı TabManager'a eklendi!");
        }
        else
        {
            Debug.LogWarning("TabManager veya NoteCollectionUI bulunamadı! Tab entegrasyonu yapılamadı.");
        }
    }
    
    // Okunan notları kaydetme
    private void SaveReadNotes()
    {
        string json = JsonUtility.ToJson(new SerializableDictionary<string, bool>(readNotes));
        PlayerPrefs.SetString("ReadNotes", json);
        PlayerPrefs.Save();
    }
    
    // Okunan notları yükleme
    private void LoadReadNotes()
    {
        if (PlayerPrefs.HasKey("ReadNotes"))
        {
            string json = PlayerPrefs.GetString("ReadNotes");
            SerializableDictionary<string, bool> loadedNotes = JsonUtility.FromJson<SerializableDictionary<string, bool>>(json);
            readNotes = loadedNotes.ToDictionary();
        }
    }
    
    // Not verilerini serialize etmek için yardımcı sınıf
    [System.Serializable]
    private class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();
        
        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
        
        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dictionary[keys[i]] = values[i];
            }
            return dictionary;
        }
    }
} 