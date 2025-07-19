using UnityEngine;

[CreateAssetMenu(fileName = "NoteTextData", menuName = "ScriptableObjects/NoteTextData")]
public class NoteTextData : ScriptableObject
{
   [Header("Note Information")]
   [SerializeField] public string noteID; // Unique identifier
   [SerializeField] public string noteTitle;
   [TextArea(5, 15)] // Daha büyük text alanı
   [SerializeField] public string noteText;
   
   [Header("Note Settings")]
   [SerializeField] public bool canOnlyBeReadOnce = false;
   [SerializeField] public bool isImportant = false; // Önemli notlar için
   
   [Header("Visual Settings")]
   [SerializeField] public Color noteColor = Color.white;
   [SerializeField] public Sprite noteIcon; // Not için özel ikon
   
   // Not'un okunup okunmadığını kontrol etmek için
   public bool HasBeenRead { get; set; } = false;
   
   private void OnValidate()
   {
       // Eğer noteID boşsa, otomatik olarak oluştur
       if (string.IsNullOrEmpty(noteID))
       {
           noteID = System.Guid.NewGuid().ToString();
       }
   }
}
