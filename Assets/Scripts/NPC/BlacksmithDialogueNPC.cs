using UnityEngine;

public class BlacksmithDialogueNPC : DialogueNPC
{
    [SerializeField] private BlacksmithHandler blacksmithHandler;
    
    private void Start()
    {
        if (blacksmithHandler == null)
        {
            blacksmithHandler = GetComponent<BlacksmithHandler>();
        }
    }
    
    // DialogueNPC'nin Interact metodunu override ediyoruz
    public override void Interact()
    {
        // Önce normal diyaloğu başlat
        base.Interact();
        
        // Diyalog bittiğinde demirciyi aç
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += OpenBlacksmith;
        }
    }
    
    private void OpenBlacksmith()
    {
        // Event'i dinlemeyi bırak
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= OpenBlacksmith;
        }
        
        // Blacksmith UI'ı aç
        if (blacksmithHandler != null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                blacksmithHandler.OpenBlacksmith(player);
            }
        }
    }
    
    private void OnDisable()
    {
        // Script devre dışı kalırsa event'i temizle
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= OpenBlacksmith;
        }
    }
}