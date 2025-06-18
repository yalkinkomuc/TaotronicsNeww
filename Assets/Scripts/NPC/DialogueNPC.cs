using UnityEngine;

public class DialogueNPC : MonoBehaviour, IInteractable
{
    
    public string npcID;
    [SerializeField] private InteractionPrompt interactionPrompt;
    [SerializeField] private DialogueData dialogueData;
    private bool playerInRange;

    private void Start()
    {
        // DialogueManager'ın OnDialogueEnd event'ini dinle
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnded;
        }
    }

    private void OnDestroy()
    {
        // Event'i temizle
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnded;
        }
    }

    public virtual void Interact()
    {
        // DialogueManager null kontrolü
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager bulunamadı!");
            return;
        }
        
        // Eğer diyalog sadece bir kez okunabilirse ve daha önce okunmuşsa, etkileşim yapma
        if (dialogueData != null && dialogueData.canOnlyBeReadOnce && 
            DialogueManager.Instance.IsDialogueRead(dialogueData))
        {
            Debug.Log("Bu diyalog daha önce okunmuş, tekrar okunamaz.");
            return;
        }
        
        Debug.Log("Interact");
        QuestManager.instance.RaiseEvent("TalkedToNPC", npcID);

         if (playerInRange)
         {
             DialogueManager.Instance.StartDialogue(dialogueData);
         }
    }

    public virtual void ShowInteractionPrompt()
    {
        // Eğer diyalog sadece bir kez okunabilirse ve daha önce okunmuşsa prompt gösterme
        if (dialogueData != null && dialogueData.canOnlyBeReadOnce && 
            DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueRead(dialogueData))
        {
            return;
        }
        
        interactionPrompt.ShowPrompt();
    }

    public virtual void HideInteractionPrompt()
    {
        interactionPrompt.HidePrompt();
    }

    private void OnDialogueEnded()
    {
        // Diyalog bittiğinde, eğer diyalog sadece bir kez okunabilirse prompt'u gizle
        if (dialogueData != null && dialogueData.canOnlyBeReadOnce && playerInRange)
        {
            HideInteractionPrompt();
        }
        // Eğer diyalog tekrar okunabilirse ve player hala range'de ise prompt'u tekrar göster
        else if (playerInRange && (dialogueData == null || !dialogueData.canOnlyBeReadOnce))
        {
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionPrompt();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            // Eğer diyalog zaten aktifse, yeni diyalog başlatma
            if (DialogueManager.Instance.IsDialogueActive)
                return;
            
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
    }
} 