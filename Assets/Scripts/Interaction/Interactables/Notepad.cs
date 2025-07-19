using UnityEngine;

public class Notepad : MonoBehaviour, IInteractable
{
    [Header("Note Settings")]
    [SerializeField] GameObject interactionPrompt;
    [SerializeField] NoteTextData noteTextData;
    
   
    
    [Header("Note Behavior")]
    [SerializeField] bool useCustomText = false; // Özel metin kullanılsın mı?
    [SerializeField] bool destroyAfterReading = false; // Okuduktan sonra yok olsun mu?
    
    
    
    public void Interact()
    {
        // NoteManager ile entegrasyon
        if (noteTextData != null && NoteManager.Instance != null)
        {
            // Eğer not zaten okunmuşsa ve sadece bir kez okunabilirse
            if (NoteManager.Instance.IsNoteRead(noteTextData.noteID) && noteTextData.canOnlyBeReadOnce)
            {
                Debug.Log("Bu not zaten okunmuş!");
                return;
            }
        }
        
        // Not verilerini hazırla
        string finalTitle = "";
        string finalText = "";
        string noteID = "";
        
        if (useCustomText)
        {
            // Inspector'da tanımlanan özel metinleri kullan
            noteID = System.Guid.NewGuid().ToString(); // Geçici ID
        }
        else if (noteTextData != null)
        {
            // ScriptableObject'ten verileri al
            finalTitle = noteTextData.noteTitle;
            finalText = noteTextData.noteText;
            noteID = noteTextData.noteID;
            
            // NoteManager'a notu ekle ve okundu olarak işaretle
            if (NoteManager.Instance != null)
            {
                NoteManager.Instance.AddNote(noteTextData);
                NoteManager.Instance.MarkNoteAsRead(noteID);
            }
        }
        
        // Not UI'sini aç ve verileri gönder
        if (!string.IsNullOrEmpty(finalText))
        {
            Notepad_UI.Instance?.ShowNote(finalTitle, finalText);
            
            if (destroyAfterReading)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("Not metni boş!");
        }
    }

    public void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    // Not verilerini dinamik olarak ayarlama metodu
    public void SetNoteData(string title, string text)
    {
        useCustomText = true;
    }
    
    // ScriptableObject'ten not verilerini ayarlama
    public void SetNoteData(NoteTextData data)
    {
        noteTextData = data;
        useCustomText = false;
    }
}
