using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemCollectionManager : MonoBehaviour
{
    public static ItemCollectionManager Instance { get; private set; }
    
    // Store IDs of collected items
    private HashSet<string> collectedItems = new HashSet<string>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Parent object (Managers) already has DontDestroyOnLoad
            
            // Load collected items
            LoadCollectedItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDisable()
    {
        // Save collected items when game is closing
        SaveCollectedItems();
    }

    // Called when application quits
    private void OnApplicationQuit()
    {
        // Ensure collected items are saved when game closes
        SaveCollectedItems();
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
    
    // Save collected items
    private void SaveCollectedItems()
    {
        string[] itemArray = collectedItems.ToArray();
        string itemsJson = JsonUtility.ToJson(new SerializableStringArray { items = itemArray });
        PlayerPrefs.SetString("CollectedItems", itemsJson);
        PlayerPrefs.Save();
    }
    
    // Load collected items
    private void LoadCollectedItems()
    {
        if (PlayerPrefs.HasKey("CollectedItems"))
        {
            string itemsJson = PlayerPrefs.GetString("CollectedItems");
            SerializableStringArray loadedItems = JsonUtility.FromJson<SerializableStringArray>(itemsJson);
            
            collectedItems = new HashSet<string>(loadedItems.items);
        }
    }
    
    // Helper class for serializing string arrays
    [System.Serializable]
    private class SerializableStringArray
    {
        public string[] items;
    }
} 