using UnityEngine;

public class Blacksmith : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "E - Demirci ile Konuş";
    
    [Header("UI")]
    [SerializeField] private BlacksmithUI blacksmithUI;
    
    [Header("Audio")]
    [SerializeField] private AudioClip greetingSound;
    
    private AudioSource audioSource;
    private bool playerInRange;
    private Player player;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // If interaction point is not set, use this transform
        if (interactionPoint == null)
        {
            interactionPoint = transform;
        }
        
        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Find BlacksmithUI if not assigned
        if (blacksmithUI == null)
        {
            blacksmithUI = FindObjectOfType<BlacksmithUI>();
        }
    }
    
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            playerInRange = true;
            
            // Show interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                
                // Update prompt text if there's a TMPro component
                TMPro.TextMeshProUGUI promptTextComponent = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (promptTextComponent != null)
                {
                    promptTextComponent.text = promptText;
                }
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            
            // Hide interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    public void Interact()
    {
        if (player == null)
            return;
            
        // Play greeting sound
        if (greetingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(greetingSound);
        }
        
        // Get player stats
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        
        if (playerStats == null)
        {
            Debug.LogError("Player does not have PlayerStats component!");
            return;
        }
        
        // Open blacksmith UI
        if (blacksmithUI != null)
        {
            blacksmithUI.OpenBlacksmith(playerStats);
        }
        else
        {
            Debug.LogError("BlacksmithUI reference not set!");
        }
    }

    public void ShowInteractionPrompt()
    {
        throw new System.NotImplementedException();
    }

    public void HideInteractionPrompt()
    {
        throw new System.NotImplementedException();
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Vector3 position = interactionPoint != null ? interactionPoint.position : transform.position;
        Gizmos.DrawWireSphere(position, interactionRadius);
    }
} 