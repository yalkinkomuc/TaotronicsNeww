using UnityEngine;

public class DialogueNPC : MonoBehaviour, IInteractable
{
    
    public string npcID;
    [SerializeField] private InteractionPrompt interactionPrompt;
    [SerializeField] private DialogueData dialogueData;
    private bool playerInRange;

    public virtual void Interact()
    {
        // DialogueManager null kontrolü
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager bulunamadı!");
            return;
        }
        
        Debug.Log("Interact");
        QuestManager.instance.RaiseEvent("TalkedToNPC", npcID);

         if (playerInRange)
         {
             DialogueManager.Instance.StartDialogue(dialogueData);
         }
    }

    public void ShowInteractionPrompt()
    {
        interactionPrompt.ShowPrompt();
    }

    public void HideInteractionPrompt()
    {
        interactionPrompt.HidePrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt();
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