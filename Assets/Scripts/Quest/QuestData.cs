using UnityEngine;

// QuestData'nın Unity Editor'da oluşturulabilmesi için
[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string id; // Görevin benzersiz ID'si
    public string title; // Görevin başlığı
    public string description; // Görev açıklaması
    
    [Header("Quest Type")]
    public QuestType questType; // Görev tipi
    
    [Header("Kill Enemies Quest Details")]
    [Tooltip("Eğer görev tipini KillEnemies seçtiyseniz doldurun")]
    public string[] targetEnemyTags; // Hedef düşman tag'leri
    public int[] enemyAmounts; // Her düşman tipi için gereken öldürme sayısı
    public bool useAllEnemiesInScene; // Sahnedeki tüm düşmanları kullan

    [Header("Collect Item Quest Details")]
    [Tooltip("Eğer görev tipini CollectItems seçtiyseniz doldurun")]
    public string itemName; // Toplanacak eşya adı
    public int itemAmount; // Toplanacak eşya miktarı
    
    [Header("Talk to NPC Quest Details")]
    [Tooltip("Eğer görev tipini TalkToNPC seçtiyseniz doldurun")]
    public string targetNPCName; // Konuşulacak NPC adı
    
    [Header("Quest Requirements")]
    public string[] requiredQuestIds; // Bu görevin başlaması için tamamlanması gereken görevlerin ID'leri
    
    [Header("Dialogue")]
    public DialogueData questStartDialogue; // Görev kabul edilirken gösterilecek diyalog
    public DialogueData questActiveDialogue; // Görev aktifken gösterilecek diyalog
    public DialogueData questCompletedDialogue; // Görev tamamlandığında gösterilecek diyalog
}

// Görev tipleri
public enum QuestType
{
    KillEnemies, // Düşman öldürme
    CollectItems, // Eşya toplama
    TalkToNPC    // NPC ile konuşma
}

// Görev durumu
public enum QuestStatus
{
    NotStarted, // Başlatılmamış
    Active,     // Aktif
    Completed   // Tamamlanmış
} 