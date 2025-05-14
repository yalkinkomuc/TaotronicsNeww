using UnityEngine;

/// <summary>
/// Automatically initializes the FloatingTextManager if it doesn't exist in the scene.
/// Attach this to a GameObject that loads early in your game.
/// </summary>
public class FloatingTextInitializer : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextManagerPrefab;
    [SerializeField] private GameObject floatingTextPrefab;
    
    private void Awake()
    {
        if (FloatingTextManager.Instance == null)
        {
            // Check if we have a prefab to instantiate
            if (floatingTextManagerPrefab != null)
            {
                Instantiate(floatingTextManagerPrefab);
            }
            else
            {
                // Create the manager object manually
                GameObject managerObject = new GameObject("FloatingTextManager");
                FloatingTextManager manager = managerObject.AddComponent<FloatingTextManager>();
                
                // Try to assign the floating text prefab if it exists
                if (floatingTextPrefab != null)
                {
                    // Use reflection to set the private field
                    var field = typeof(FloatingTextManager).GetField("floatingTextPrefab", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    
                    if (field != null)
                    {
                        field.SetValue(manager, floatingTextPrefab);
                    }
                    else
                    {
                        Debug.LogWarning("Could not find floatingTextPrefab field on FloatingTextManager");
                    }
                }
                else
                {
                    Debug.LogWarning("FloatingTextPrefab not assigned to FloatingTextInitializer");
                }
                
                // Make the manager persistent
                DontDestroyOnLoad(managerObject);
            }
        }
    }
} 