using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string characterName;
    [TextArea(3, 10)] // Unity inspector'da daha büyük text alanı
    public string[] dialogueLines; // Tek metin yerine dizi
    public Sprite characterPortrait;
    
    [Header("Dialogue Settings")]
    [Tooltip("Bu diyalog sadece bir kez okunabilir mi?")]
    public bool canOnlyBeReadOnce = false;
} 