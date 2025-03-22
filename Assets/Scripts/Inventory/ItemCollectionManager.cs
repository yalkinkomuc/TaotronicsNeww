using System.Collections.Generic;
using UnityEngine;

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkItemAsCollected(string itemID)
    {
        collectedItems.Add(itemID);
    }

    public bool WasItemCollected(string itemID)
    {
        return collectedItems.Contains(itemID);
    }
} 