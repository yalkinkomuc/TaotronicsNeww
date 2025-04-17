using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    
    [Header("UI Elements")]
    [SerializeField] private Image experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    
    [Header("Level Up Animation")]
    [SerializeField] private GameObject levelUpEffectPrefab;
    [SerializeField] private Transform levelUpEffectPosition;
    [SerializeField] private AudioClip levelUpSound;
    
    private void Start()
    {
        if (playerStats == null)
        {
            // Try to find the player automatically
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
            }
        }
        
        // Initial UI update
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (playerStats == null) return;
        
        // Get values through reflection to avoid direct public fields
        System.Type type = playerStats.GetType();
        int level = (int)type.GetField("level", System.Reflection.BindingFlags.Instance | 
                                      System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
        int experience = (int)type.GetField("experience", System.Reflection.BindingFlags.Instance | 
                                          System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
        int experienceToNextLevel = (int)type.GetField("experienceToNextLevel", System.Reflection.BindingFlags.Instance | 
                                                    System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = "Lvl " + level;
        }
        
        // Update experience bar
        if (experienceBar != null)
        {
            experienceBar.fillAmount = (float)experience / experienceToNextLevel;
        }
        
        // Update experience text
        if (experienceText != null)
        {
            experienceText.text = experience + " / " + experienceToNextLevel;
        }
    }
    
    public void PlayLevelUpEffect()
    {
        // Play sound
        if (levelUpSound != null)
        {
            AudioSource.PlayClipAtPoint(levelUpSound, Camera.main.transform.position);
        }
        
        // Spawn effect
        if (levelUpEffectPrefab != null && levelUpEffectPosition != null)
        {
            Instantiate(levelUpEffectPrefab, levelUpEffectPosition.position, Quaternion.identity);
        }
    }
} 