using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemCollectionManager : MonoBehaviour
{
    public static ItemCollectionManager Instance { get; private set; }
    
    // Toplanan eşyaların ID'lerini tut
    private HashSet<string> collectedItems = new HashSet<string>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Parent objesi (Managers) zaten DontDestroyOnLoad
            
            // Toplanan eşyaları yükle
            LoadCollectedItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDisable()
    {
        // Oyun kapatılırken toplanan eşyaları kaydet
        SaveCollectedItems();
    }

    // Oyun kapatıldığında çağrılır
    private void OnApplicationQuit()
    {
        // Oyun kapatılırken toplanan eşyaları kesin olarak kaydet
        SaveCollectedItems();
        Debug.Log("Oyun kapatılıyor, toplanan eşyalar kaydedildi!");
    }

    public void MarkItemAsCollected(string itemID)
    {
        collectedItems.Add(itemID);
        SaveCollectedItems();
    }

    public bool WasItemCollected(string itemID)
    {
        return collectedItems.Contains(itemID);
    }
    
    // Toplanan eşyaları kaydet
    private void SaveCollectedItems()
    {
        string[] itemArray = collectedItems.ToArray();
        string itemsJson = JsonUtility.ToJson(new SerializableStringArray { items = itemArray });
        PlayerPrefs.SetString("CollectedItems", itemsJson);
        PlayerPrefs.Save();
    }
    
    // Toplanan eşyaları yükle
    private void LoadCollectedItems()
    {
        if (PlayerPrefs.HasKey("CollectedItems"))
        {
            string itemsJson = PlayerPrefs.GetString("CollectedItems");
            SerializableStringArray loadedItems = JsonUtility.FromJson<SerializableStringArray>(itemsJson);
            
            collectedItems = new HashSet<string>(loadedItems.items);
        }
    }
    
    // String dizisini serileştirmek için yardımcı sınıf
    [System.Serializable]
    private class SerializableStringArray
    {
        public string[] items;
    }
} 