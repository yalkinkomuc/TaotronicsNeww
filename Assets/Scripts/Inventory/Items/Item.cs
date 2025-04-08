using UnityEngine;
using System;

[Serializable]
public class Item
{
    public string id;
    public string itemName;
    public string description;
    public Sprite icon;
    
    public Item(string id, string itemName, string description, Sprite icon)
    {
        this.id = id;
        this.itemName = itemName;
        this.description = description;
        this.icon = icon;
    }
} 