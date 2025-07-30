using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
   public static SceneSwapManager instance;

   private static bool loadFromDoor;
   private static bool loadFromTrigger;

   private GameObject Player;
   private Player playerScript;
   private Collider2D playerCollider;
   private Collider2D doorCollider;
   private Collider2D sceneTriggerCollider;
   private Vector3 playerSpawnPosition;
   
   
   private DoorTriggerInteraction.DoorToSpawnAt doorToSpawnTo;
   private SceneTriggerInteraction.SceneToSpawnAt SceneToSpawnTrigger;

   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      
      Player = GameObject.FindWithTag("Player");
      playerScript = Player.GetComponent<Player>();
      playerCollider = Player.GetComponent<Collider2D>();
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

   public static void SwapSceneFromTrigger(SceneField myScene, SceneTriggerInteraction.SceneToSpawnAt sceneToSpawnAt)
   {
      Debug.Log("SceneSwapManager - SwapSceneFromTrigger() çağrıldı");
      Debug.Log($"myScene: {myScene}, sceneToSpawnAt: {sceneToSpawnAt}");
      
      if (instance == null)
      {
         Debug.LogError("SceneSwapManager instance null! SceneSwapManager sahneye eklenmemiş!");
         return;
      }
      
      loadFromTrigger = true;
      Debug.Log("loadFromTrigger = true yapıldı");
      Debug.Log("FadeOutThenChangeSceneWithTrigger coroutine başlatılıyor");
      instance.StartCoroutine(instance.FadeOutThenChangeSceneWithTrigger(myScene, sceneToSpawnAt));
   }
   
   

   private IEnumerator FadeOutThenChangeScene(SceneField myScene,
      DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt = DoorTriggerInteraction.DoorToSpawnAt.None)
   {
      UserInput.DeactivatePlayerControls();
      SceneFadeManager.instance.StartFadeOut();

      while (SceneFadeManager.instance.isFadingOut)
      {
         yield return null;
      }
      
      doorToSpawnTo = doorToSpawnAt;
      SceneManager.LoadScene(myScene);
   }

   private IEnumerator ActivatePlayerControlsAfterFadeIn()
   {
      while (SceneFadeManager.instance.isFadingIn)
      {
         yield return null;
      }
      
      UserInput.ActivatePlayerControls();
   }
   
   
   private IEnumerator FadeOutThenChangeSceneWithTrigger(SceneField myScene,
      SceneTriggerInteraction.SceneToSpawnAt sceneToSpawnAt = SceneTriggerInteraction.SceneToSpawnAt.None)
   {
      if (SceneFadeManager.instance == null)
      {
         yield break;
      }
      
      InGameUI.instance.CloseInGameUI();
      UserInput.DeactivatePlayerControls();
      SceneFadeManager.instance.StartFadeOut();

      Debug.Log("Fade out bekleniyor...");
      while (SceneFadeManager.instance.isFadingOut)
      {
         yield return null;
      }
      
      SceneToSpawnTrigger = sceneToSpawnAt;
      SceneManager.LoadScene(myScene);
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
      // CameraManager'dan SceneBoundary'yi güncelle
      if (CameraManager.instance != null)
      {
         CameraManager.instance.UpdateSceneBoundary();
      }
      
      InGameUI.instance.OpenInGameUI();
      playerScript.ShowWeapons();
      
      
      SceneFadeManager.instance.StartFadeIn();
     

     

      if (loadFromDoor)
      {
         StartCoroutine(ActivatePlayerControlsAfterFadeIn());
         FindDoor(doorToSpawnTo);
         Player.transform.position = playerSpawnPosition;
         loadFromDoor = false;
      }

      if (loadFromTrigger)
      {
         StartCoroutine(ActivatePlayerControlsAfterFadeIn());
         FindSceneEntry(SceneToSpawnTrigger);
         Player.transform.position = playerSpawnPosition;
         loadFromTrigger = false;
      }
   }

   private void FindDoor(DoorTriggerInteraction.DoorToSpawnAt doorSpawnNumber)
   {
      DoorTriggerInteraction[] doors =  FindObjectsOfType<DoorTriggerInteraction>();

      for(int i = 0; i < doors.Length; i++)
      {
         if (doors[i].CurrentDoorPosition == doorSpawnNumber)
         {
            doorCollider = doors[i].gameObject.GetComponent<Collider2D>();
            
            CalculateSpawnPositionForDoor();
            return;
         }
      }
   }
   
   private void FindSceneEntry(SceneTriggerInteraction.SceneToSpawnAt sceneSpawnNumber)
   {
     SceneTriggerInteraction[] scenesTriggers =  FindObjectsOfType<SceneTriggerInteraction>();

      for(int i = 0; i < scenesTriggers.Length; i++)
      {
         if (scenesTriggers[i].CurrentSceneTriggerPosition == sceneSpawnNumber)
         {
            sceneTriggerCollider = scenesTriggers[i].gameObject.GetComponent<Collider2D>();
            
            CalculateSpawnPositionForScene();
            return;
         }
      }
   }
   
   

   private void CalculateSpawnPositionForDoor()
   {
      float colliderHeight = playerCollider.bounds.extents.y;
      playerSpawnPosition = doorCollider.transform.position + new Vector3(0f, colliderHeight * 0.35f, 0f);
   }
   
   private void CalculateSpawnPositionForScene()
   {

      float xOffset = 0;
      SceneTriggerInteraction[] scenesTriggers =  FindObjectsOfType<SceneTriggerInteraction>();
      for (int i = 0; i < scenesTriggers.Length; i++)
      {
         xOffset = scenesTriggers[i].xOffset;
      }
      
      
      float colliderHeight = playerCollider.bounds.extents.y;
      playerSpawnPosition = sceneTriggerCollider.transform.position + new Vector3(xOffset, colliderHeight * -0.75f, 0f);
   }

   
}
