using UnityEngine;

// Interface for any enemy that can be parried
public interface IParryable
{
    bool IsParryWindowOpen { get; }
    void GetParried();
    Transform GetTransform();
} 