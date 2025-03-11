using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image characterPortrait;
   
    [SerializeField] private float typingSpeed = 0.05f; // Harf yazma hızı

    public bool IsDialogueActive { get; private set; }
    
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private string currentLine;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        dialogueCanvas.SetActive(false);
        IsDialogueActive = false;
        
    }

    private void Update()
    {
        if (!IsDialogueActive) 
            return;

        // Sol tık veya Space tuşu ile diyaloğu ilerlet
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // Input'u tükettiğimizi belirtmek için
            Input.ResetInputAxes();
            
            if (isTyping)
            {
                CompleteText();
            }
            else
            {
                DisplayNextDialogue();
            }
        }
    }

    public void StartDialogue(DialogueData dialogueData)
    {
        currentDialogue = dialogueData;
        currentLineIndex = 0;
        IsDialogueActive = true;
        dialogueCanvas.SetActive(true);

        // İlk diyaloğu göster
        nameText.text = currentDialogue.characterName;
        if (currentDialogue.characterPortrait != null)
            characterPortrait.sprite = currentDialogue.characterPortrait;

        DisplayNextDialogue();
    }

    private void DisplayNextDialogue()
    {
        // Tüm diyaloglar bittiyse kapat
        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        // Önceki typing coroutine'i varsa durdur
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // Yeni diyaloğu başlat
        currentLine = currentDialogue.dialogueLines[currentLineIndex];
        typingCoroutine = StartCoroutine(TypeText(currentLine));
        currentLineIndex++;
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteText()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLine;
        isTyping = false;
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        dialogueCanvas.SetActive(false);
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
    }
} 