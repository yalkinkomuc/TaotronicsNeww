using UnityEngine;

public class SceneTriggerInteraction : TriggerInteractionBase
{
    
    [SerializeField] public float xOffset;
    
    public enum SceneToSpawnAt
    {
        None,
        One,
        Two,
        Three,
        Four,
        Five,
    }
    
    [Header("SpawnTO")] 
    [SerializeField] private SceneToSpawnAt sceneToSpawnTo;
    [SerializeField] private SceneField sceneToLoad;
    
    public SceneToSpawnAt CurrentSceneTriggerPosition;
    
    
    public override void SceneTransition()
    {
        
        if (sceneToLoad == null)
        {
            Debug.LogError("sceneToLoad null! Inspector'da atanmamış!");
            return;
        }
        
        SceneSwapManager.SwapSceneFromTrigger(sceneToLoad, sceneToSpawnTo);
    }
}
