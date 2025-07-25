using UnityEngine;

public class DoorTriggerInteraction : TriggerInteractionBase
{
   public enum DoorToSpawnAt
   {
      None,
      One,
      Two,
      Three,
      Four,
      Five,
   }

   [Header("SpawnTO")] 
   [SerializeField] private DoorToSpawnAt DoorToSpawnTo;
   [SerializeField] private SceneField sceneToLoad;

   [Space(10f)] [Header("THIS DOOR")] 
   public DoorToSpawnAt CurrentDoorPosition;
   
   
   public override void Interact()
   {
      SceneSwapManager.SwapSceneFromDoorUse(sceneToLoad, DoorToSpawnTo);
   }
}
