using UnityEngine;
using System.Collections.Generic;

public enum StatModifierType
{
    Base,
    Flat,
    Equipment,
    Buff,
    LevelBonus
}

[System.Serializable]
public class Stat 
{
    [SerializeField] private float baseValue;

    // New structure to track modifiers with their types
    private List<StatModifier> modifiers = new List<StatModifier>();

    // Default constructor for serialization
    public Stat() 
    {
        modifiers = new List<StatModifier>();
    }
    
    // Constructor with initial value
    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        modifiers = new List<StatModifier>();
    }
   
    public float GetValue()
    {
        float finalValue = baseValue;

        foreach (StatModifier modifier in modifiers)
        {
            finalValue += modifier.Value;
        }
     
        return finalValue;
    }

    public void AddModifier(float value)
    {
        modifiers.Add(new StatModifier(value, StatModifierType.Flat));
    }
    
    public void AddModifier(float value, StatModifierType type)
    {
        modifiers.Add(new StatModifier(value, type));
    }

    public void RemoveModifier(float value)
    {
        // Find and remove first modifier with this value
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].Value == value)
            {
                modifiers.RemoveAt(i);
                return;
            }
        }
    }
    
    public void RemoveAllModifiersOfType(StatModifierType type)
    {
        modifiers.RemoveAll(mod => mod.Type == type);
    }
}

[System.Serializable]
public class StatModifier
{
    public float Value { get; private set; }
    public StatModifierType Type { get; private set; }
    
    public StatModifier(float value, StatModifierType type)
    {
        Value = value;
        Type = type;
    }
}
