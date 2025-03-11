using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private GameObject promptVisual; // "E" tuşu görseli vs.

    private void Awake()
    {
        HidePrompt();
    }

    public void ShowPrompt()
    {
        promptVisual.SetActive(true);
    }

    public void HidePrompt()
    {
        promptVisual.SetActive(false);
    }
} 