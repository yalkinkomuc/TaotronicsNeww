using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
   public static SceneSwapManager instance;

   private static bool loadFromDoor;
   
   private DoorTriggerInteraction.DoorToSpawnAt doorToSpawnTo;

   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
   }

   private void OnEnable()
   {
      SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnDisable()
   {
      SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   public static void SwapSceneFromDoorUse(SceneField myScene, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt)
   {
      loadFromDoor = true;
      instance.StartCoroutine(instance.FadeOutThenChangeScene(myScene, doorToSpawnAt));
   }
   

   private IEnumerator FadeOutThenChangeScene(SceneField myScene,
      DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt = DoorTriggerInteraction.DoorToSpawnAt.None)
   {
      
      SceneFadeManager.instance.StartFadeOut();

      while (SceneFadeManager.instance.isFadingOut)
      {
         yield return null;
      }
      
      doorToSpawnTo = doorToSpawnAt;
      SceneManager.LoadScene(myScene);
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
      SceneFadeManager.instance.StartFadeIn();

      if (loadFromDoor)
      {
         loadFromDoor = false;
      }
   }
}
