using UnityEngine;

public class DoorTrigger : MonoBehaviour, IInteractable
{
    [Header("Scene Settings")]
    [SerializeField] private int targetSceneIndex = 1;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Spawn Settings")]
    [SerializeField] private string targetSpawnPointName = "DoorSpawn";
    
    [Header("Interaction Settings")]
    [SerializeField] private InteractionPrompt interactionPrompt;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioSource audioSource;
    
    private bool playerInRange = false;
    private bool isTransitioning = false;
    private Player playerReference;
    
    private void Start()
    {
        // InteractionPrompt yoksa bu objede arayalım
        if (interactionPrompt == null)
        {
            interactionPrompt = GetComponentInChildren<InteractionPrompt>();
        }
        
        // AudioSource yoksa oluştur
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    private void Update()
    {
        // Oyuncu trigger alanındaysa ve geçiş yapılmıyorsa input kontrolü yap
        if (playerInRange && !isTransitioning)
        {
            CheckForInteractionInput();
        }
    }
    
    private void CheckForInteractionInput()
    {
        // W tuşu veya yukarı ok tuşu kontrolü
        bool upInput = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        
        if (upInput)
        {
            Interact();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isTransitioning)
        {
            playerInRange = true;
            playerReference = other.GetComponent<Player>();
            
            // IInteractable interface metodunu kullan
            ShowInteractionPrompt();
            
            Debug.Log($"Player entered door trigger. Press W or ↑ to enter scene {targetSceneIndex}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerReference = null;
            
            // IInteractable interface metodunu kullan
            HideInteractionPrompt();
            
            Debug.Log("Player left door trigger area");
        }
    }
    
    // IInteractable interface implementation
    public void Interact()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        // Interaction prompt'u gizle
        HideInteractionPrompt();
        
        // Spawn point bilgisini kaydet (unique isim ile)
        SaveDoorSpawnInfo();
        
        // Ses efekti çal
        PlayDoorSound();
        
        // Oyuncuyu durdur
        StopPlayer();
        
        // Sahne geçişini başlat
        StartSceneTransition();
        
        Debug.Log($"Entering door - transitioning to scene {targetSceneIndex}");
    }
    
    public void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.ShowPrompt();
        }
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.HidePrompt();
        }
    }
    
    private void SaveDoorSpawnInfo()
    {
        // Unique isim ile spawn point belirt
        PlayerPrefs.SetString("TargetSpawnPointName", targetSpawnPointName);
        PlayerPrefs.SetInt("UseNamedSpawnPoint", 1);
        PlayerPrefs.SetInt("UseCustomSpawn", 0);
        PlayerPrefs.Save();
        
        Debug.Log($"Door spawn info saved: Target spawn point = {targetSpawnPointName}");
    }
    
    private void PlayDoorSound()
    {
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
    }
    
    private void StopPlayer()
    {
        if (playerReference != null)
        {
            // Oyuncunun hareketini durdur
            Rigidbody2D rb = playerReference.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            // Silahları gizle
            playerReference.HideWeapons();
        }
    }
    
    private void StartSceneTransition()
    {
        // Custom SceneManager'ı bul ve kullan
        SceneManager customSceneManager = FindFirstObjectByType<SceneManager>();
        
        if (customSceneManager != null)
        {
            customSceneManager.LoadBossArena(targetSceneIndex);
        }
        else
        {
            // Unity'nin SceneManager'ını kullan
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneIndex);
        }
    }
} 