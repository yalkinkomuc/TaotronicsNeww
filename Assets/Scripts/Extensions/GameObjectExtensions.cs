using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// Displays a floating damage text above the game object
    /// </summary>
    /// <param name="gameObject">The game object above which to show text</param>
    /// <param name="damage">The damage amount to display</param>
    /// <param name="isCritical">Whether the damage is critical (affects color)</param>
    public static void ShowDamageText(this GameObject gameObject, float damage, bool isCritical = false)
    {
        if (FloatingTextManager.Instance != null)
        {
            // Calculate position above the object
            Vector3 position = gameObject.transform.position + Vector3.up * 1.5f;
            FloatingTextManager.Instance.ShowDamageText(damage, position, isCritical);
        }
    }
    
    /// <summary>
    /// Displays a floating text with custom content above the game object
    /// </summary>
    /// <param name="gameObject">The game object</param>
    /// <param name="text">The text to display</param>
    /// <param name="color">The color of the text</param>
    public static void ShowFloatingText(this GameObject gameObject, string text, Color color)
    {
        if (FloatingTextManager.Instance != null)
        {
            // Calculate position above the object
            Vector3 position = gameObject.transform.position + Vector3.up * 1.5f;
            // Use the implemented method
            FloatingTextManager.Instance.ShowCustomText(text, position, color);
        }
    }
} 