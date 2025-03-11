using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private int bossArenaSceneIndex = 1; // Boss arena sahnesinin build index'i
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private string bossName = "Necromancer";
    [SerializeField] private GameObject defeatedVisual; // Boss yenildiğinde gösterilecek görsel
    private const float DOOR_APPEAR_DELAY = 0.5f;
    
    private void Start()
    {
        // Boss yenildiyse görsel değişikliği yap
        if (GameProgressManager.instance != null && GameProgressManager.instance.IsBossDefeated(bossName))
        {
            if (defeatedVisual != null)
                defeatedVisual.SetActive(true);
        }
    }
    
    public void Interact()
    {
        
        
        if (PlayerManager.instance == null)
        {
            
            return;
        }

        if (PlayerManager.instance.player == null)
        {
            
            return;
        }

        if (SceneManager.instance != null)
        {
            
            PlayerManager.instance.player.gameObject.SetActive(false);
            
            
            SceneManager.instance.LoadBossArena(bossArenaSceneIndex);
        }
    }

    public void ShowInteractionPrompt()
    {
        if (prompt != null)
            prompt.ShowPrompt();
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null)
            prompt.HidePrompt();
    }

    public string GetInteractText()
    {
        throw new System.NotImplementedException();
    }

    public void ShowDefeatedDoor()
    {
        if (defeatedVisual != null)
            StartCoroutine(ShowDefeatedDoorWithDelay());
    }

    private IEnumerator ShowDefeatedDoorWithDelay()
    {
        yield return new WaitForSeconds(DOOR_APPEAR_DELAY);
        defeatedVisual.SetActive(true);
    }
} 