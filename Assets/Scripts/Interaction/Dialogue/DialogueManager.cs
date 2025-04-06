using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject dialogueCanvasPrefab;
    [SerializeField] private string dialogueTextName = "DialogueText"; // Text objesinin adı
    [SerializeField] private string nameTextName = "NameText";        // İsim text objesinin adı
    [SerializeField] private string portraitImageName = "Portrait";   // Portre image'ın adı

    private GameObject dialogueCanvas;
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI nameText;
    private Image characterPortrait;
   
    [SerializeField] private float typingSpeed = 0.05f; // Harf yazma hızı

    public bool IsDialogueActive { get; private set; }
    
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private string currentLine;
    private Coroutine typingCoroutine;
    
    // Diyalog sona erdiğinde tetiklenecek event
    public event Action OnDialogueEnd;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize()
    {
        SetupDialogueUI();
        Debug.Log("DialogueManager initialized");
    }

    private void SetupDialogueUI()
    {
        // Null kontrolü ekle
        if (this == null) return;
        if (!gameObject.activeInHierarchy) return;

        if (dialogueCanvasPrefab != null)
        {
            // Önceki canvas varsa yok et
            if (dialogueCanvas != null)
                Destroy(dialogueCanvas);
            
            // Yeni canvas oluştur
            dialogueCanvas = Instantiate(dialogueCanvasPrefab, transform);
            
            // İsimlere göre kesin referansları al
            dialogueText = dialogueCanvas.transform.Find(dialogueTextName)?.GetComponent<TextMeshProUGUI>();
            nameText = dialogueCanvas.transform.Find(nameTextName)?.GetComponent<TextMeshProUGUI>();
            characterPortrait = dialogueCanvas.transform.Find(portraitImageName)?.GetComponent<Image>();
            
            if (dialogueText == null || nameText == null || characterPortrait == null)
            {
                Debug.LogError("UI references not found by name! Check the element names in your prefab:");
                Debug.LogError($"Looking for: {dialogueTextName}, {nameTextName}, {portraitImageName}");
            }
            
            dialogueCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Dialogue Canvas Prefab is not assigned in DialogueManager!");
        }
        
        IsDialogueActive = false;
    }

    private void OnEnable()
    {
        // Scene yüklendiğinde UI'ı yeniden kur
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Scene değiştiğinde UI referansları kaybolabilir, yeniden kur
        if (dialogueCanvas == null)
        {
            SetupDialogueUI();
        }
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
        // Null kontrolü ekle
        if (this == null) return;
        if (dialogueData == null) return;

        SetupDialogueUI();
        
        currentDialogue = dialogueData;
        currentLineIndex = 0;
        IsDialogueActive = true;
        dialogueCanvas.SetActive(true);

        nameText.text = currentDialogue.characterName;
        if (currentDialogue.characterPortrait != null && characterPortrait != null)
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
            
        // Diyalog sona erdiğinde event'i tetikle
        OnDialogueEnd?.Invoke();
    }
} 