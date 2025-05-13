using UnityEngine;

public class BlacksmithHandler : MonoBehaviour
{
    [SerializeField] private BlacksmithUI blacksmithUI;
   // [SerializeField] private AudioClip greetingSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
       
        
        // Find BlacksmithUI if not assigned
        if (blacksmithUI == null)
        {
            blacksmithUI = FindObjectOfType<BlacksmithUI>();
        }
    }
    
    public void OpenBlacksmith(Player player)
    {
        if (player == null)
        {
            Debug.LogError("BlacksmithHandler: Player is null!");
            return;
        }
        
        // BlacksmithManager kontrolü
        if (BlacksmithManager.Instance == null)
        {
            Debug.LogError("BlacksmithHandler: BlacksmithManager.Instance null!");
            
            // Manager'ı bulmayı dene
            BlacksmithManager manager = FindObjectOfType<BlacksmithManager>();
            if (manager != null)
            {
                Debug.Log("BlacksmithHandler: BlacksmithManager bulundu, instance atanıyor.");
                BlacksmithManager.Instance = manager;
            }
            else
            {
                Debug.LogError("BlacksmithHandler: BlacksmithManager bulunamadı, UI açılamıyor!");
                return;
            }
        }
        
        // Get player stats
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        
        if (playerStats == null)
        {
            Debug.LogError("BlacksmithHandler: Player does not have PlayerStats component!");
            return;
        }
        
        // Open blacksmith UI
        if (blacksmithUI != null)
        {
            Debug.Log("BlacksmithHandler: Opening BlacksmithUI");
            blacksmithUI.OpenBlacksmith(playerStats);
        }
        else
        {
            Debug.LogError("BlacksmithHandler: BlacksmithUI reference not set!");
        }
    }
}