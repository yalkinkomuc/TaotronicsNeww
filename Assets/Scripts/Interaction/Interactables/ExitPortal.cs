using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionPrompt prompt;
    public int mainSceneIndex;

    private void Start()
    {
        // Eğer prompt component'i atanmamışsa, çocuk objeden bul
        if (prompt == null)
            prompt = GetComponentInChildren<InteractionPrompt>();
    }

    public void Interact()
    {
        // Ana sahneye dön
        if (SceneManager.instance != null)
        {
            // Önce oyuncuyu deaktive et
            if (PlayerManager.instance?.player != null)
            {
                PlayerManager.instance.player.gameObject.SetActive(false);
            }
            
            SceneManager.instance.LoadBossArena(mainSceneIndex);
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
} 